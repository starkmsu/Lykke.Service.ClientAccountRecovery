using System;
using System.Net.Http;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    public partial class ClientAccountRecoveryServiceClient
    {
        internal ClientAccountRecoveryServiceClient(Uri baseUri, HttpClient client) : base(client)
        {
            Initialize();

            BaseUri = baseUri ?? throw new ArgumentNullException("baseUri");
        }
    }
}
