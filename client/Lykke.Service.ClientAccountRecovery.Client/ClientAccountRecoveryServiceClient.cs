using Lykke.HttpClientGenerator;
using Lykke.Service.ClientAccountRecovery.Client.Api;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    /// <inheritdoc />
    internal class ClientAccountRecoveryServiceClient : IClientAccountRecoveryServiceClient
    {
        /// <inheritdoc />
        public IRecoveryApi RecoveryApi { get; }

        public ClientAccountRecoveryServiceClient(IHttpClientGenerator httpClientGenerator)
        {
            RecoveryApi = httpClientGenerator.Generate<IRecoveryApi>();
        }
    }
}
