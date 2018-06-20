using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class NewRecoveryRequest
    {
        [Required]
        [MinLength(Consts.MinClientIdLength)]
        public string ClientId { get; set; }

        [Required]
        public string Ip { get; set; }

        [MaxLength(Consts.MaxUserAgentLength)]
        public string UserAgent { get; set; }
    }
}
