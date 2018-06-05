using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class ApproveChallengeRequest
    {
        [Required]
        public string RecoveryId { get;  set; }

        [Required]
        public Challenge Challenge { get;  set; }

        [Required]
        public string AgentId { get;  set; }

    }
}
