using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
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

            return Ok(new NewRecoveryResponse() { RecoveryId = "sdfdfe" });
        }

        [HttpGet("{recoveryId}")]
        [SwaggerOperation("GetRecoveryStatus")]
        [ProducesResponseType(typeof(RecoveryStatusResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRecoveryStatus(string recoveryId)
        {
            return Ok(new RecoveryStatusResponse() { Challenge = Core.Domain.Challenge.Email });
        }

        [HttpPost("challenge")]
        [SwaggerOperation("GetNextChallenge")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> PostChallenge([FromBody]ChallengeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

            return Ok();
        }

        //[Authorize]
        [HttpGet("client/{clientId}")]
        [SwaggerOperation("GetClientRecoveries")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<ClientRecoveryHistoryResponse>))]
        public async Task<IActionResult> GetClientRecoveries(string clientId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }

        // [Authorize]
        [HttpGet("client/trace/{recoveryId}")]
        [SwaggerOperation("GetRecoveryTrace")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<RecoveryTraceResponse>))]
        public async Task<IActionResult> GetRecoveryTrace(string recoveryId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }
    }
}
