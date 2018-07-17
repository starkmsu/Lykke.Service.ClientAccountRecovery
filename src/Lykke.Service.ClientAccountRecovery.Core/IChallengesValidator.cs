using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core.Services;

namespace Lykke.Service.ClientAccountRecovery.Core
{
    public interface IChallengesValidator
    {
        Task<bool> ConfirmEmailCode(IRecoveryFlowService flowService, string code);
        Task<bool> ConfirmSmsCode(IRecoveryFlowService flowService, string code);
        Task<bool> ConfirmDeviceCode(IRecoveryFlowService flowService, string code);
        Task<bool> ConfirmSecretPhrasesCode(IRecoveryFlowService flowService, string code);
    }
}
