using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Client.Models.Enums;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Recovery
{
    [PublicAPI]
    public class ChallengeRequest
    {
        /// <summary>
        ///     JWE token containing current state of recovery process.
        /// </summary>
        public string StateToken { get; set; }

        /// <summary>
        ///     Resolution
        /// </summary>
        public Action Action { get; set; }

        /// <summary>
        ///     A challenge value
        /// </summary>
        public string Value { get; set; }

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
