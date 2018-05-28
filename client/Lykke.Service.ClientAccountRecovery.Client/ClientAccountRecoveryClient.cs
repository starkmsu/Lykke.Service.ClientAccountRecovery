using System;
using Common.Log;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    public class ClientAccountRecoveryClient : IClientAccountRecoveryClient, IDisposable
    {
        private readonly ILog _log;

        public ClientAccountRecoveryClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
