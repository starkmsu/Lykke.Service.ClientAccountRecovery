using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Core.Services
{
    public interface IRecoveryFlowService
    {
        RecoveryContext Context { get; }
        Task StartRecoveryAsync();
        Task SecretPhrasesCompleteAsync();
        Task SecretPhrasesSkipAsync();
        Task DeviceVerifiedCompleteAsync();
        Task DeviceVerificationSkip();
        Task SmsVerificationComplete();
        Task SmsVerificationSkip();
        Task SmsVerificationRestart();
        Task SmsVerificationFailed();
        Task EmailVerificationComplete();
        Task EmailVerificationSkip();
        Task EmailVerificationRestart();
        Task EmailVerificationFailed();
        Task SelfieVerificationRequest();
        Task SelfieVerificationSkip();
        Task SelfieVerificationFail();
        Task SelfieVerificationComplete();
        Task PinCodeVerificationComplete();
        Task PinCodeVerificationSkip();
        Task JumpToAllowAsync();
        Task JumpToSupportAsync();
        Task JumpToFrozenAsync();
        Task JumpToSuspendAsync();
        Task UpdatePasswordComplete();
        Task TryUnfreeze();
        bool IsPasswordUpdateAllowed { get; }
    }
}
