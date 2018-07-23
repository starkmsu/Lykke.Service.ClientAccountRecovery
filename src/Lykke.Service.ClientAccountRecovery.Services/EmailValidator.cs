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
    public class EmailValidator : IChallengesValidator
    {
        private readonly IConfirmationCodesClient _conformationClient;
        private readonly IClientAccountClient _accountClient;

        public EmailValidator(IConfirmationCodesClient conformationClient, IClientAccountClient accountClient)
        {
            _conformationClient = conformationClient;
            _accountClient = accountClient;
        }

        public async Task<bool> Confirm(IRecoveryFlowService flowService, string code)
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
                return true;
            }

            await flowService.EmailVerificationFailedAsync();
            return false;
        }
    }
}
