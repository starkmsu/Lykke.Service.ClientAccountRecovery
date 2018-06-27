using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    [PublicAPI]
    public enum CheckResult
    {
        Unknown,
        Approved,
        Rejected
    }
}
