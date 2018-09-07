using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Recovery
{
    [PublicAPI]
    public class NewRecoveryRequest
    {
        /// <summary>
        ///     An id of Client
        /// </summary>
        public string ClientId { get; set; }

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
