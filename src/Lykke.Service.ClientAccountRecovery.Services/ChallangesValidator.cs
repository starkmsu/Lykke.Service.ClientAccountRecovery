using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ConfirmationCodes.Client;
using Lykke.Service.ConfirmationCodes.Client.Models.Request;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class ChallengesValidator : IChallengesValidator
    {
        private readonly IConfirmationCodesClient _conformationClient;
        private readonly IClientAccountClient _accountClient;

        public ChallengesValidator(IConfirmationCodesClient conformationClient, IClientAccountClient accountClient)
        {

            _conformationClient = conformationClient;
            _accountClient = accountClient;
        }

        public async Task ConfirmEmailCode(IRecoveryFlowService flowService, string code)
        {
            var clientModel = await _accountClient.GetByIdAsync(flowService.Context.ClientId);
            var result = await _conformationClient.VerifyEmailCodeAsync(new VerifyEmailConfirmationRequest
            {
                Email = clientModel.Email,
                PartnerId = clientModel.PartnerId,
                Code = code
            });

            if (result.IsValid)
            {
                await flowService.EmailVerificationCompleteAsync();
            }
            else
            {
                await flowService.EmailVerificationFailedAsync();
            }
        }

        public async Task ConfirmSmsCode(IRecoveryFlowService flowService, string code)
        {
            var clientModel = await _accountClient.GetByIdAsync(flowService.Context.ClientId);
            var result = await _conformationClient.VerifySmsCodeAsync(new VerifySmsConfirmationRequest
            {
                Phone = clientModel.Phone,
                PartnerId = clientModel.PartnerId,
                Code = code
            });

            if (result.IsValid)
            {
                await flowService.SmsVerificationCompleteAsync();
            }
            else
            {
                await flowService.SmsVerificationFailedAsync();
            }
        }
    }
}
