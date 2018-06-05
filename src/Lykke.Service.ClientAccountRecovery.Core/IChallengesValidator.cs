using System.Threading.Tasks;

namespace Lykke.Service.ClientAccountRecovery.Core
{
    public interface IChallengesValidator
    {
        Task ConfirmEmailCode(string clientId, string code);
        Task ConfirmSmsCode(string clientId, string code);
    }
}
