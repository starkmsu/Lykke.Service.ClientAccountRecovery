using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core.Services;

namespace Lykke.Service.ClientAccountRecovery.Core
{
    public interface IChallengesValidator
    {
        Task ConfirmEmailCode(IRecoveryFlowService flowService, string code);
        Task ConfirmSmsCode(IRecoveryFlowService flowService, string code);
    }
}
