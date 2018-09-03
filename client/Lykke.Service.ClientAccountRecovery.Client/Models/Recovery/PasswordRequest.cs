using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Recovery
{
    [PublicAPI]
    public class PasswordRequest
    {
        /// <summary>
        ///     JWE token containing current state of recovery process.
        /// </summary>
        public string StateToken { get; set; }

        /// <summary>
        ///     A hash of the new password
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        ///     A new PIN
        /// </summary>
        public string Pin { get; set; }

        /// <summary>
        ///     A password hint
        /// </summary>
        public string Hint { get; set; }

        /// <summary>
        ///     Client's IP
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        ///     Client's user agent
        /// </summary>
        public string UserAgent { get; set; }
    }
}
