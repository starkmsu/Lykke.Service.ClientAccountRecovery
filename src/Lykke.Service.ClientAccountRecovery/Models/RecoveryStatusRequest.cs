using Lykke.Service.ClientAccountRecovery.Validation;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class RecoveryStatusRequest
    {
        /// <summary>
        ///     JWE token containing current state of recovery process.
        /// </summary>
        [JweToken]
        public string StateToken { get; set; }
    }
}
