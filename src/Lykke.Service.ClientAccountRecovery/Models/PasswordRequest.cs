using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class PasswordRequest
    {
        [Required]
        public string RecoveryId { get;  set; }

        [Required]
        public string PasswordHash { get;  set; }
    }
}
