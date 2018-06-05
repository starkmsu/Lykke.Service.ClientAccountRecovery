using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class RecoveryTraceResponse
    {
        [Required]
        public DateTime Time { get; internal set; }

        [Required]
        public State PreviousState { get; internal set; }  

        [Required]
        public Trigger Action { get; internal set; }

        [Required]
        public State NewState { get; internal set; }  
        
        [Required]
        public string Initiator { get; internal set; } 
        
        public string Comment { get; internal set; }
    }
}
