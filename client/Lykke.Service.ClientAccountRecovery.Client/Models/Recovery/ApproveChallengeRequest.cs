using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Client.Models.Enums;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Recovery
{
    [PublicAPI]
    public class ApproveChallengeRequest
    {
        /// <summary>
        ///     An id of the recovery
        /// </summary>
        public string RecoveryId { get; set; }

        /// <summary>
        ///     A challenge to approve
        /// </summary>
        public Challenge Challenge { get; set; }

        /// <summary>
        ///     An agent id
        /// </summary>
        public string AgentId { get; set; }

        /// <summary>
        ///     Resolution
        /// </summary>
        public CheckResult CheckResult { get; set; }
    }
}
