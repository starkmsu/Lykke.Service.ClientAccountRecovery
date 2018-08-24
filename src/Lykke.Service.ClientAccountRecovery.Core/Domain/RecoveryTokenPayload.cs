namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    /// <summary>
    ///     Current state of password recovery process.
    /// </summary>
    public class RecoveryTokenPayload
    {
        /// <summary>
        ///     Unique id identifying password recovery attempt.
        /// </summary>
        public string RecoveryId { get; set; }

        /// <summary>
        ///     Current challenge provided for client.
        /// </summary>
        public Challenge Challenge { get; set; }
    }
}
