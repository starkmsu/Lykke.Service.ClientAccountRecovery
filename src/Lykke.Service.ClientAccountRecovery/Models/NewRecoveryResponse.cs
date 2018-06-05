using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class NewRecoveryResponse
    {
        [DisplayName("clientId")]
        [Required]
        public string RecoveryId { get; internal set; }
    }
}
