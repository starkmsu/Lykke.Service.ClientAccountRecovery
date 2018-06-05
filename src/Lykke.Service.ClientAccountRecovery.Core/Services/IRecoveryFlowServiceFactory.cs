using System.Threading.Tasks;

namespace Lykke.Service.ClientAccountRecovery.Core.Services
{
    public interface IRecoveryFlowServiceFactory
    {
        IRecoveryFlowService InitiateNew(string clientId);
        Task<IRecoveryFlowService> GetExisted(string recoveryId);
    }
}
