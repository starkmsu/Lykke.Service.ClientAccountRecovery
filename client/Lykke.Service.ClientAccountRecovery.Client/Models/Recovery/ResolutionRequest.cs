using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Client.Models.Enums;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Recovery
{
    [PublicAPI]
    public class ResolutionRequest
    {
        /// <summary>
        ///     An id of the recovery
        /// </summary>
        public string RecoveryId { get; set; }

        /// <summary>
        ///     A resolution from support
        /// </summary>
        public Resolution Resolution { get; set; }

        /// <summary>
        ///     Client's ip
        /// </summary>
        public string AgentId { get; set; }

        /// <summary>
        ///     Resolution comment from support.
        /// </summary>
        public string Comment { get; set; }
    }
}
