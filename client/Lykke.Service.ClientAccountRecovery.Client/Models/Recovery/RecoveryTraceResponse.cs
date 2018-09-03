using System;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Client.Models.Enums;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Recovery
{
    [PublicAPI]
    public class RecoveryTraceResponse
    {
        /// <summary>
        ///     A date time of the event
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        ///     A previous state
        /// </summary>
        public State PreviousState { get; set; }

        /// <summary>
        ///     An action that leaded state changing
        /// </summary>
        public Trigger Action { get; set; }

        /// <summary>
        ///     A current state
        /// </summary>
        public State NewState { get; set; }

        /// <summary>
        ///     An initiator of the event
        /// </summary>
        public string Initiator { get; set; }

        /// <summary>
        ///     Comment from the support
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        ///     Client's ip
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        ///     Client's user agent
        /// </summary>
        public string UserAgent { get; set; }
    }
}
