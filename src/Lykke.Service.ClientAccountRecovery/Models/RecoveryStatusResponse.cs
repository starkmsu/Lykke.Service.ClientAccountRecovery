using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class RecoveryStatusResponse
    {
        public Challenge Challenge { get; internal set; }
        public Progress OverallProgress { get; internal set; }
    }
}
