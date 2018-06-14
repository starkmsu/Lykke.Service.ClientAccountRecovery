using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class RecoveryStatusResponse
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Challenge Challenge { get; internal set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Progress OverallProgress { get; internal set; }
    }
}
