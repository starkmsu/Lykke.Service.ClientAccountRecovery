using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class ChallengeRequest
    {
        [Required]
        [MinLength(Consts.MinRecoveryIdLength)]
        public string RecoveryId { get;  set; }
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public Challenge Challenge { get;  set; }
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public Action Action { get;  set; }
        [Required]
        public string Value { get;  set; }
    }
}
