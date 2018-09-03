using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Recovery
{
    /// <summary>
    ///     Status of challenge submitting operation.
    /// </summary>
    [PublicAPI]
    public class OperationStatus
    {
        /// <summary>
        ///     True if there was an error
        /// </summary>
        public bool Error { get; set; }

        /// <summary>
        ///     A description of the error or null
        /// </summary>
        public string Message { get; set; }
    }
}
