using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class NewRecoveryRequest
    {
        [Required]
        public string ClientId { get; set; }
    }
}
