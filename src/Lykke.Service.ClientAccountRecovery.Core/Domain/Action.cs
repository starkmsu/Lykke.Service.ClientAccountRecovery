using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    [PublicAPI]
    public enum Action
    {
        Undefined = 0,
        Complete = 1,
        Restart = 2,
        Skip = 3
    }
}
