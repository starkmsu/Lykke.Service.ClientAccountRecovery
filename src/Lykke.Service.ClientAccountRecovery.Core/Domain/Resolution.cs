using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    [PublicAPI]
    public enum Resolution
    {
        Unknown,
        Suspend,
        Interview,
        Freeze,
        Allow
    }
}
