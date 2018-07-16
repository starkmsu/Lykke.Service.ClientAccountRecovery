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
        [MaxLength(1024)]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(10)]
        public string Pin { get; set; }

        [Required]
        [MaxLength(128)]
        public string Hint { get; set; }

        [Required]
        public string Ip { get; set; }

        [MaxLength(Consts.MaxUserAgentLength)]
        public string UserAgent { get; set; }
    }
}
