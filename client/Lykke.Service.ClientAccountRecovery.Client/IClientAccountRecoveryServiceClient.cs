using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Client.Api;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    /// <summary>
    ///     An interface of the client recovery service.
    /// </summary>
    [PublicAPI]
    public interface IClientAccountRecoveryServiceClient
    {
        /// <summary>
        ///     Api for Recovery controller.
        /// </summary>
        IRecoveryApi RecoveryApi { get; }
    }
}
