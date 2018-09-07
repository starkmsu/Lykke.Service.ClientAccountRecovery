using System;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Client.Models.Enums;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Recovery
{
    [PublicAPI]
    public class ClientRecoveryHistoryResponse
    {
        /// <summary>
        ///     An id of the recovery
        /// </summary>
        public string RecoveryId { get; set; }

        /// <summary>
        ///     An event date time
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        ///     An event state
        /// </summary>
        public State State { get; set; }

        /// <summary>
        ///     The initiator of the event
        /// </summary>
        public string Initiator { get; set; }
    }
}
