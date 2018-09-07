using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Enums
{
    /// <summary>
    ///     A challenge resolution provided by the support
    /// </summary>
    [PublicAPI]
    public enum CheckResult
    {
        /// <summary>
        ///     Default result.
        /// </summary>
        Unknown,

        /// <summary>
        ///     Check approved.
        /// </summary>
        Approved,

        /// <summary>
        ///     Check rejected.
        /// </summary>
        Rejected
    }
}
