using System.Threading.Tasks;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ConfirmationCodes.Client;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    public class ChallengesValidator : IChallengesValidator
    {
        private readonly IRecoveryFlowService _flowService;
        private readonly IConfirmationCodesClient _conformationClient;
        private readonly IClientAccountClient _accountClient;

        public ChallengesValidator(IRecoveryFlowService flowService, IConfirmationCodesClient conformationClient, IClientAccountClient accountClient)
        {
            _flowService = flowService;
            _conformationClient = conformationClient;
            _accountClient = accountClient;
        }

        public async Task ConfirmEmailCode(string clientId, string code)
        {
            var clientModel = await _accountClient.GetByIdAsync(clientId);
            var result = await _conformationClient.VerifyEmailCode(new EmailConfirmationRequest
            {
                Email = clientModel.Email,
                PartnerId = clientModel.PartnerId
            }, code);

            if (result.IsValid)
            {
                await _flowService.EmailVerificationComplete();
            }
            else
            {
                await _flowService.EmailVerificationFailed();
            }
        }

        public async Task ConfirmSmsCode(string clientId, string code)
        {
            var clientModel = await _accountClient.GetByIdAsync(clientId);
            var result = await _conformationClient.VerifySmsCode(new SmsConfirmationRequest
            {
                Phone = clientModel.Phone,
                PartnerId = clientModel.PartnerId
            }, code);

            if (result.IsValid)
            {
                await _flowService.SmsVerificationComplete();
            }
            else
            {
                await _flowService.SmsVerificationFailed();
            }
        }
    }
}
