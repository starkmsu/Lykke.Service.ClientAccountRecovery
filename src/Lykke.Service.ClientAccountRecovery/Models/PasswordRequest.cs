using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class PasswordRequest
    {
        [Required]
        [MinLength(Consts.MinRecoveryIdLength)]
        public string RecoveryId { get; set; }

        [Required]
        public string PasswordHash { get; set; }
    }
}
