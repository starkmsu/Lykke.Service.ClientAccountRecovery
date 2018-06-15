using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core.Services;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class SelfieSender : ISelfieSender
    {

        public Task Send(string clientId)
        {
            throw new System.NotImplementedException();
        }
    }
}
