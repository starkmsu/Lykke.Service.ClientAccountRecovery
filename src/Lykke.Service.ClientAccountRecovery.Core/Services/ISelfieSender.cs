using System.Threading.Tasks;

namespace Lykke.Service.ClientAccountRecovery.Core.Services
{
    public interface ISelfieSender
    {
        Task Send(string clientId);
    }
}
