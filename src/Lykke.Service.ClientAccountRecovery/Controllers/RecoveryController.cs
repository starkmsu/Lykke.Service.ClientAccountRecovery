using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Lykke.Service.ClientAccountRecovery.Models;
using Microsoft.AspNetCore.Authorization;

namespace Lykke.Service.ClientAccountRecovery.Controllers
{

    [Route("api/recovery")]
    [Produces("application/json")]
    public class RecoveryController : Controller
    {
        private readonly IStateRepository _stateRepository;
        private readonly IRecoveryLogRepository _logRepository;
        private readonly IRecoveryFlowServiceFactory _factory;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IChallengeManager _challengeManager;
        private readonly ILog _log;

        public RecoveryController(IStateRepository stateRepository, IRecoveryLogRepository logRepository, IRecoveryFlowServiceFactory factory, IClientAccountClient clientAccountClient, IChallengeManager challengeManager, ILog log)
        {
            _stateRepository = stateRepository;
            _logRepository = logRepository;
            _factory = factory;
            _clientAccountClient = clientAccountClient;
            _challengeManager = challengeManager;
            _log = log.CreateComponentScope(nameof(RecoveryController));
        }

        /// <summary>
        /// Checks service is alive
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("StartNewRecovery")]
        [ProducesResponseType(typeof(NewRecoveryResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Index([FromBody]NewRecoveryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var flowService = _factory.InitiateNew(request.ClientId);
            try
            {
                await flowService.StartRecoveryAsync();
            }
            catch (InvalidActionException ex)
            {
                _log.WriteWarning("StartNewRecovery", request, "Unable to start new account recovery process", ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _log.WriteWarning("StartNewRecovery", request, "Unable to start new account recovery process", ex);
                throw;
            }

            return Ok(new NewRecoveryResponse { RecoveryId = flowService.Context.RecoveryId });
        }

        [HttpGet("{recoveryId}")]
        [SwaggerOperation("GetRecoveryStatus")]
        [ProducesResponseType(typeof(RecoveryStatusResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRecoveryStatus([FromRoute]string recoveryId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var state = await _logRepository.GetAsync(recoveryId);
            if (state.Empty)
            {
                return NotFound();
            }

            var challenge = state.ActualStatus.State.MapToChallenge();
            var progress = state.ActualStatus.State.MapToProgress();
            return Ok(new RecoveryStatusResponse() { Challenge = challenge, OverallProgress = progress });
        }

        [HttpPost("challenge")]
        [SwaggerOperation("SubmitChallenge")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> PostChallenge([FromBody]ChallengeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var flow = await _factory.FindExisted(request.RecoveryId);
            if (flow == null)
            {
                return NotFound();
            }
            await _challengeManager.ExecuteAction(request.Challenge, request.Action, request.Value, flow);
            return Ok();
        }

        [HttpPost("password")]
        [SwaggerOperation("UpdatePassword")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> PostPassword([FromBody]PasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var flowService = await _factory.FindExisted(request.RecoveryId);
                if (flowService == null)
                {
                    return NotFound();
                }
                await _clientAccountClient.ChangeClientPasswordAsync(flowService.Context.ClientId, request.PasswordHash);
                await flowService.UpdatePasswordComplete();
            }
            catch (InvalidActionException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _log.WriteWarning("UpdatePassword", request.RecoveryId, "Unable to update password", ex);
                throw;
            }

            return Ok();
        }

        [Authorize]
        [HttpPut("challenge/approval")]
        [SwaggerOperation("ApproveChallenge")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ApproveChallenge([FromBody]ApproveChallengeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var flowService = await _factory.FindExisted(request.RecoveryId);
                if (flowService == null)
                {
                    return NotFound();
                }

                flowService.Context.Initiator = request.AgentId;
                await flowService.SelfieVerificationComplete(); // In a moment supporting only selfie
            }
            catch (InvalidActionException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _log.WriteWarning("ApproveChallenge", request.RecoveryId, "Unable to approve challenge", ex);
                throw;
            }

            return Ok();
        }

        //[Authorize]
        [HttpPost("challenge/resolution")]
        [SwaggerOperation("SubmitResolution")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> SubmitResolution([FromBody]ResolutionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var flowService = await _factory.FindExisted(request.RecoveryId);
                if (flowService == null)
                {
                    return NotFound();
                }

                flowService.Context.Initiator = request.AgentId;
                flowService.Context.Comment = request.Comment;
                switch (request.Resolution)
                {
                    case Resolution.Suspend:
                        await flowService.JumpToSuspendAsync();
                        break;
                    case Resolution.Interview:
                        await flowService.JumpToSupportAsync();
                        break;
                    case Resolution.Freeze:
                        await flowService.JumpToFrozenAsync();
                        break;
                    case Resolution.Allow:
                        await flowService.JumpToAllowAsync();
                        break;
                    default:
                        return BadRequest();
                }
            }
            catch (InvalidActionException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _log.WriteWarning("SubmitResolution", request.RecoveryId, "Unable to submit resolution", ex);
                throw;
            }
            return Ok();
        }

        //[Authorize]
        [HttpGet("client/{clientId}")]
        [SwaggerOperation("GetClientRecoveries")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<ClientRecoveryHistoryResponse>))]
        public async Task<IActionResult> GetClientRecoveries([FromRoute]string clientId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var summary = await _stateRepository.GetRecoverySummary(clientId);

            if (summary == null)
            {
                return NotFound();
            }

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

        // [Authorize]
        [HttpGet("client/trace/{recoveryId}")]
        [SwaggerOperation("GetRecoveryTrace")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<RecoveryTraceResponse>))]
        public async Task<IActionResult> GetRecoveryTrace([FromRoute]string recoveryId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var unit = await _logRepository.GetAsync(recoveryId);
            if (unit == null)
            {
                return NotFound();
            }

            var response = RecoveryTraceResponse.Convert(unit);

            return Ok(response);
        }




    }


}
