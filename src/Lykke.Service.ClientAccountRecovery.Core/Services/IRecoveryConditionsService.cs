using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Core.Services
{
    /// <summary>
    ///     Provide recovery conditions for other services.
    /// </summary>
    public interface IRecoveryConditionsService
    {
        /// <summary>
        ///     Recovery conditions.
        /// </summary>
        RecoveryConditions RecoveryConditions { get; }
    }
}
