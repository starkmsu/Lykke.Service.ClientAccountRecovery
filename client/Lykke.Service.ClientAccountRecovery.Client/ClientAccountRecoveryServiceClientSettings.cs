using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    /// <summary>
    ///     ClientAccountRecoveryService client settings.
    /// </summary>
    [PublicAPI]
    public class ClientAccountRecoveryServiceClientSettings
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }

        /// <summary>
        ///     Api Key.
        /// </summary>
        public string ApiKey { get; set; }
    }
}
