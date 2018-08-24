using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Validation;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class PasswordRequest
    {
        /// <summary>
        ///     JWE token containing current state of recovery process.
        /// </summary>
        [JweToken]
        public string StateToken { get; set; }

        /// <summary>
        /// A hash of the new password
        /// </summary>
        [Required]
        [MaxLength(1024)]
        public string PasswordHash { get; set; }

        /// <summary>
        /// A new PIN
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string Pin { get; set; }

        /// <summary>
        /// A password hint
        /// </summary>
        [Required]
        [MaxLength(128)]
        public string Hint { get; set; }

        /// <summary>
        /// Client's IP
        /// </summary>
        [Required]
        public string Ip { get; set; }

        /// <summary>
        /// Client's user agent
        /// </summary>
        [MaxLength(Consts.MaxUserAgentLength)]
        public string UserAgent { get; set; }
    }
}
