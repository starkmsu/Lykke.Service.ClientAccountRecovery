using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class ClientRecoveryHistoryResponse
    {
        [Required]
        public string RecoveryId { get; internal set; }

        [Required]
        public DateTime Time { get; internal set; }

        [Required]
        public State State { get; internal set; }  
        
        [Required]
        public string Initiator { get; internal set; }
    }
}
