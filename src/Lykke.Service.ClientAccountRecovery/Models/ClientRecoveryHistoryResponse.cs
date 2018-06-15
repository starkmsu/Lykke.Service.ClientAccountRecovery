using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class ClientRecoveryHistoryResponse
    {
        [Required]
        public string RecoveryId { get; internal set; }

        [Required]
        public DateTime Time { get; internal set; }

        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public State State { get; internal set; }  
        
        [Required]
        public string Initiator { get; internal set; }
    }
}
