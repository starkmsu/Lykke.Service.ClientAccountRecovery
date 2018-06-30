using System.Threading.Tasks;

namespace Lykke.Service.ClientAccountRecovery.Core.Services
{
    public interface IBrutForceDetector
    {
        Task<bool> IsNewRecoveryAllowedAsync(string clientId);
    }
}
