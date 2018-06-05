using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class ChallengeRequest
    {
        [Required]
        public string RecoveryId { get;  set; }
        [Required]
        public Challenge Challenge { get;  set; }
        [Required]
        public Action Action { get;  set; }
        [Required]
        public string Value { get;  set; }
    }
}
