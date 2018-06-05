using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class ResolutionRequest
    {
        [Required]
        public string RecoveryId { get;  set; }

        [Required]
        public Resolution Resolution { get;  set; }

        [Required]
        public string AgentId { get;  set; }

        [MaxLength(256)]
        public string Comment { get;  set; }

    }
}
