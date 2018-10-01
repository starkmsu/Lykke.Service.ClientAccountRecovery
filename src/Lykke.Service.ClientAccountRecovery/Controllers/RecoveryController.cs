using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Contract;
using Lykke.Common.Log;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Exceptions;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ClientAccountRecovery.Middleware;
using Lykke.Service.ClientAccountRecovery.Models;
using Lykke.Service.ClientAccountRecovery.Validation;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Refit;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.ClientAccountRecovery.Controllers
{
    [Route("api/recovery")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class RecoveryController : Controller
    {
        private readonly IStateRepository _stateRepository;
        private readonly IRecoveryLogRepository _logRepository;
        private readonly IRecoveryFlowServiceFactory _factory;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IChallengeManager _challengeManager;
        private readonly IBruteForceDetector _bruteForceDetector;
        private readonly IPersonalDataService _personalDataService;
        private readonly ILog _log;
        private readonly IRecoveryTokenService _recoveryTokenService;
        private readonly IPersonalDataServiceClient _personalDataClient;

        public RecoveryController(IStateRepository stateRepository,
            IRecoveryLogRepository logRepository,
            IRecoveryFlowServiceFactory factory,
            IClientAccountClient clientAccountClient,
            IChallengeManager challengeManager,
            IBruteForceDetector bruteForceDetector,
            ILogFactory logFactory,
            IPersonalDataService personalDataService,
            IRecoveryTokenService recoveryTokenService,
            IPersonalDataServiceClient personalDataClient)
        {
            _stateRepository = stateRepository;
            _logRepository = logRepository;
            _factory = factory;
            _clientAccountClient = clientAccountClient;
            _challengeManager = challengeManager;
            _bruteForceDetector = bruteForceDetector;
            _personalDataService = personalDataService;
            _recoveryTokenService = recoveryTokenService;
            _personalDataClient = personalDataClient;
            _log = logFactory.CreateLog(this);
        }

        /// <summary>
        ///     Starts password recovering process
        /// </summary>
        [HttpPost("token/start")]
        [SwaggerOperation("StartNewRecovery")]
        [ProducesResponseType(typeof(NewRecoveryResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Conflict)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> StartNewRecovery([FromBody] NewRecoveryRequest request)
        {
            var clientId = request.ClientId;

            var client = await _clientAccountClient.GetByIdAsync(clientId);

            if (client == null)
                return NotFound(ErrorResponse.Create("Client not found."));

            await SealPreviousRecoveries(clientId, request.Ip, request.UserAgent);

            if (!await _bruteForceDetector.IsNewRecoveryAllowedAsync(clientId))
                return StatusCode((int) HttpStatusCode.Forbidden, ErrorResponse.Create("Recovery attempts limits reached"));

            var flow = await _factory.InitiateNew(clientId);
            flow.Context.Initiator = Consts.InitiatorUser;
            flow.Context.Ip = request.Ip;
            flow.Context.UserAgent = request.UserAgent;

            try
            {
                await flow.StartRecoveryAsync();
            }
            catch (InvalidActionException ex)
            {
                _log.Warning("StartNewRecovery", "Unable to start new account recovery process", ex, request);
                return Conflict(ErrorResponse.Create(ex.Message));
            }

            var stateToken = await _recoveryTokenService.GenerateTokenAsync(flow.Context);

            return Ok(new NewRecoveryResponse
            {
                StateToken = stateToken
            });
        }

        /// <summary>
        ///     Returns the current recovery state
        /// </summary>
        [HttpPost("token/status")]
        [SwaggerOperation("GetRecoveryStatus")]
        [ProducesResponseType(typeof(RecoveryStatusResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRecoveryStatus([FromBody] RecoveryStatusRequest model)
        {
            try
            {
                var recoveryId = await _recoveryTokenService.GetRecoveryIdAsync(model.StateToken);
                var flow = await _factory.FindExisted(recoveryId);
                if (flow == null) 
                    return NotFound(ErrorResponse.Create("Recovery not found."));

                await flow.TryUnfreezeAsync();
                var challenge = flow.Context.State.MapToChallenge();
                var progress = flow.Context.State.MapToProgress();
                var message = flow.Context.SignChallengeMessage;
                return Ok(new RecoveryStatusResponse
                {
                    Challenge = challenge,
                    OverallProgress = progress,
                    ChallengeInfo = message
                });
            }
            catch (InvalidRecoveryTokenException e)
            {
                _log.Warning(nameof(GetRecoveryStatus), "Unable to get recovery status", e);
                ModelState.AddModelError(nameof(RecoveryStatusRequest.StateToken), e.Message);

                return BadRequest(ErrorResponseFactory.Create(ModelState));
            }
        }

        /// <summary>
        ///     Accepts challenge values
        /// </summary>
        [HttpPost("token/challenge")]
        [SwaggerOperation("SubmitChallenge")]
        [ProducesResponseType(typeof(SubmitChallengeResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SubmitChallenge([FromBody] ChallengeRequest request)
        {
            try
            {
                var oldState =
                    await _recoveryTokenService.GetTokenPayloadAsync<RecoveryTokenPayload>(request.StateToken);

                var recoveryId = oldState.RecoveryId;
                var challenge = oldState.Challenge;

                var flow = await _factory.FindExisted(recoveryId);
                if (flow == null) 
                    return NotFound(ErrorResponse.Create("Recovery not found."));

                flow.Context.Initiator = Consts.InitiatorUser;
                flow.Context.Ip = request.Ip;
                flow.Context.UserAgent = request.UserAgent;

                var challengeSuccessful =
                    await _challengeManager.ExecuteAction(challenge, request.Action, request.Value, flow);

                if (!challengeSuccessful)
                    return Ok(SubmitChallengeResponse.CreateError("The challenge failed"));

                var newStateToken = await _recoveryTokenService.GenerateTokenAsync(flow.Context);

                return Ok(SubmitChallengeResponse.CreateSuccess(newStateToken));
            }
            catch (Exception e)
            {
                _log.Warning(nameof(SubmitChallenge), "Unable to submit challenge", e);

                switch (e)
                {
                    case InvalidRecoveryTokenException _:
                        ModelState.AddModelError(nameof(ChallengeRequest.StateToken), e.Message);
                        break;
                    case InvalidActionException _:
                        ModelState.AddModelError(nameof(ChallengeRequest.Action), e.Message);
                        break;
                    default:
                        throw;
                }

                return BadRequest(ErrorResponseFactory.Create(ModelState));
            }
        }

        /// <summary>
        ///     Updates the user password
        /// </summary>
        [HttpPost("token/password")]
        [SwaggerOperation("UpdatePassword")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Conflict)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordRequest request)
        {
            try
            {
                var recoveryId = await _recoveryTokenService.GetRecoveryIdAsync(request.StateToken);

                var flow = await _factory.FindExisted(recoveryId);
                if (flow == null) 
                    return NotFound(ErrorResponse.Create("Recovery not found."));

                if (!flow.IsPasswordUpdateAllowed)
                    return Conflict(ErrorResponse.Create($"Unable to update the password from this state: {flow.Context.State}"));

                flow.Context.Initiator = Consts.InitiatorUser;
                flow.Context.Comment = null;
                flow.Context.Ip = request.Ip;
                flow.Context.UserAgent = request.UserAgent;
                await _clientAccountClient.ChangeClientPasswordAsync(flow.Context.ClientId, request.PasswordHash);
                await _clientAccountClient.ChangeClientPinAsync(flow.Context.ClientId, request.Pin);
                await _personalDataService.ChangePasswordHintAsync(flow.Context.ClientId, request.Hint);

                await flow.UpdatePasswordCompleteAsync();

                return Ok();
            }
            catch (Exception e)
            {
                const string logMessage = "Unable to update password";

                switch (e)
                {
                    case InvalidActionException _:
                        _log.Warning(nameof(UpdatePassword), logMessage, e);
                        return Conflict(ErrorResponse.Create(e.Message));
                    case InvalidRecoveryTokenException _:
                        _log.Warning(nameof(UpdatePassword), logMessage, e);
                        ModelState.AddModelError(nameof(PasswordRequest.StateToken), e.Message);
                        return BadRequest(ErrorResponseFactory.Create(ModelState));
                    default:
                        throw;
                }
            }
        }

        /// <summary>
        ///     Approves user challenges. Only for support.
        /// </summary>
        [HttpPut("challenge/challenge/checkResult")]
        [SwaggerOperation("ApproveChallenge")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ApiKeyAuth]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Conflict)]
        [ProducesResponseType((int) HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ApproveChallenge([FromBody] ApproveChallengeRequest request)
        {
            if (request.CheckResult == CheckResult.Unknown ||
                request.Challenge != Core.Domain.Challenge.Selfie)
                return BadRequest(ErrorResponse.Create("Invalid data."));

            var flow = await _factory.FindExisted(request.RecoveryId);
            if (flow == null)
                return NotFound(ErrorResponse.Create("Recovery not found."));


            flow.Context.Initiator = request.AgentId;
            flow.Context.Ip = null;
            flow.Context.UserAgent = null;
            try
            {
                if (request.CheckResult == CheckResult.Approved)
                    await flow.SelfieVerificationCompleteAsync(); // In a moment supporting only selfie
                else
                    await flow.SelfieVerificationFailAsync();
            }
            catch (InvalidActionException ex)
            {
                return Conflict(ErrorResponse.Create(ex.Message));
            }

            return Ok();
        }

        /// <summary>
        ///     Updates current state of the recovery process. Only for support.
        /// </summary>
        [HttpPost("challenge/resolution")]
        [SwaggerOperation("SubmitResolution")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ApiKeyAuth]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Conflict)]
        [ProducesResponseType((int) HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SubmitResolution([FromBody] ResolutionRequest request)
        {
            var flow = await _factory.FindExisted(request.RecoveryId);
            if (flow == null)
                return NotFound(ErrorResponse.Create("Recovery not found."));

            flow.Context.Initiator = request.AgentId;
            flow.Context.Comment = request.Comment;
            flow.Context.Ip = null;
            flow.Context.UserAgent = null;
            try
            {
                switch (request.Resolution)
                {
                    case Resolution.Suspend:
                        await flow.JumpToSuspendAsync();
                        break;
                    case Resolution.Interview:
                        await flow.JumpToSupportAsync();
                        break;
                    case Resolution.Allow:
                        await flow.JumpToAllowAsync();
                        break;
                    default:
                        return BadRequest();
                }
            }
            catch (InvalidActionException ex)
            {
                return Conflict(ErrorResponse.Create(ex.Message));
            }

            return Ok();
        }

        /// <summary>
        ///     Returns brief information about all client's recoveries
        /// </summary>
        /// <param name="clientId">The client id</param>
        [HttpGet("client/{clientId}")]
        [SwaggerOperation("GetClientRecoveries")]
        [ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(IEnumerable<ClientRecoveryHistoryResponse>))]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetClientRecoveries([FromRoute] string clientId)
        {
            var summary = await _stateRepository.FindRecoverySummary(clientId);

            if (summary == null)
                    return NotFound(ErrorResponse.Create("Summary not found."));

            var result = from unit in summary.Log
                let actual = unit.ActualStatus
                select new ClientRecoveryHistoryResponse
                {
                    Initiator = actual.Initiator,
                    RecoveryId = actual.RecoveryId,
                    State = actual.State,
                    Time = actual.Time
                };
            return Ok(result);
        }

        /// <summary>
        ///     Returns detailed information about the recovery
        /// </summary>
        /// <param name="recoveryId">The recovery id</param>
        [HttpGet("client/trace/{recoveryId}")]
        [SwaggerOperation("GetRecoveryTrace")]
        [ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(IEnumerable<RecoveryTraceResponse>))]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRecoveryTrace([FromRoute] string recoveryId)
        {
            var unit = await _logRepository.GetAsync(recoveryId);
            if (unit.Empty) 
                return NotFound(ErrorResponse.Create("Recovery unit not found."));

            var response = RecoveryTraceResponse.Convert(unit);

            return Ok(response);
        }

        /// <summary>Upload selfie file.</summary>
        [SelfieMaxSize]
        [Consumes("multipart/form-data")]
        [HttpPost("selfie")]
        [SwaggerOperation("UploadSelfie")]
        [ProducesResponseType(typeof(UploadSelfieResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Conflict)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UploadSelfie([BindRequired, FromForm] string stateToken, [BindRequired, FromForm] IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest(ErrorResponse.Create("File could not be null."));

                var state =
                    await _recoveryTokenService.GetTokenPayloadAsync<RecoveryTokenPayload>(stateToken);

                // Check if token contains correct challenge.
                if (state.Challenge != Core.Domain.Challenge.Selfie)
                    return BadRequest(ErrorResponse.Create("Token contains invalid challenge."));

                var recoveryId = await _recoveryTokenService.GetRecoveryIdAsync(stateToken);

                var flow = await _factory.FindExisted(recoveryId);
                if (flow == null) 
                    return NotFound(ErrorResponse.Create("Recovery not found."));

                // Check if state machine is in correct state.
                if (!flow.IsSelfieUploadAllowed)
                    return Conflict(ErrorResponse.Create($"Unable to upload selfie from this state: {flow.Context.State}"));

                using (var imageStream = file.OpenReadStream())
                {
                    var streamPart = new StreamPart(imageStream, file.FileName, file.ContentType);

                    var clientId = flow.Context.ClientId;

                    var fileId = await _personalDataClient.ClientAccountRecoveryApi.UploadSelfieAsync(clientId, streamPart);
                    
                    var response = new UploadSelfieResponse
                    {
                        FileId = fileId
                    };

                    return Ok(response);
                }
            }
            catch (Exception e)
            {

                _log.Warning(
                    $"Unable to upload file. FileName: {file?.FileName}; Length: {file?.Length} bytes; ContentType: {file?.ContentType};",
                    e);

                switch (e)
                {
                    case ApiException apiException:
                        if (apiException.StatusCode == HttpStatusCode.BadRequest)
                            return BadRequest(ErrorResponse.Create(apiException.Message));
                        break;
                    case InvalidRecoveryTokenException tokenException:
                        return BadRequest(ErrorResponse.Create(tokenException.Message));
                }

                throw;
            }
        }

        private async Task SealPreviousRecoveries(string clientId, string ip, string userAgent)
        {
            var history = await _bruteForceDetector.GetRecoveriesToSeal(clientId);
            foreach (var recoveryUnit in history)
            {
                var flow = await _factory.FindExisted(recoveryUnit.RecoveryId);
                flow.Context.Initiator = Consts.InitiatorService;
                flow.Context.Ip = ip;
                flow.Context.UserAgent = userAgent;

                await flow.JumpToForbiddenAsync();
            }
        }
    }
}
