using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class ResolutionRequest
    {
        [Required]
        [MinLength(Consts.MinRecoveryIdLength)]
        public string RecoveryId { get;  set; }

        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public Resolution Resolution { get;  set; }

        [Required]
        public string AgentId { get;  set; }

        [MaxLength(256)]
        public string Comment { get;  set; }

    }
}
