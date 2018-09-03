using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Recovery
{
    [PublicAPI]
    public class SubmitChallengeResponse
    {
        /// <summary>
        ///     JWE token containing current state of recovery process.
        /// </summary>
        public string StateToken { get; set; }

        /// <summary>
        ///     Status of recovery operation.
        /// </summary>
        public OperationStatus OperationStatus { get; set; }
    }
}
