using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    [PublicAPI]
    public class RecoveryStatusResponse
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Challenge Challenge { get; internal set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Progress OverallProgress { get; internal set; }

        /// <summary>
        /// Addition data to perform the challenge. For example a message to be signed by the private key
        /// </summary>
        public string ChallengeInfo { get; internal set; }
    }
}
