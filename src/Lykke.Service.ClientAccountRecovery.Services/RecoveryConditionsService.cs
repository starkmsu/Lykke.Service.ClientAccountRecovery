using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    /// <inheritdoc />
    public class RecoveryConditionsService : IRecoveryConditionsService
    {
        public RecoveryConditionsService(RecoveryConditions recoveryConditions)
        {
            RecoveryConditions = recoveryConditions;
        }

        /// <inheritdoc />
        public RecoveryConditions RecoveryConditions { get; }
    }
}
