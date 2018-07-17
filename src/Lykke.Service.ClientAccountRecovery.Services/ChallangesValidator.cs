using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ConfirmationCodes.Client;
using Lykke.Service.ConfirmationCodes.Client.Models.Request;
using NBitcoin;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class ChallengesValidator : IChallengesValidator
    {
        private readonly IConfirmationCodesClient _conformationClient;
        private readonly IClientAccountClient _accountClient;
        private readonly IWalletCredentialsRepository _credentialsRepository;

        public ChallengesValidator(IConfirmationCodesClient conformationClient, IClientAccountClient accountClient, IWalletCredentialsRepository credentialsRepository)
        {

            _conformationClient = conformationClient;
            _accountClient = accountClient;
            _credentialsRepository = credentialsRepository;
        }

        public async Task<bool> ConfirmEmailCode(IRecoveryFlowService flowService, string code)
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

        public async Task<bool> ConfirmSmsCode(IRecoveryFlowService flowService, string code)
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
                return true;
            }

            await flowService.SmsVerificationFailedAsync();
            return false;
        }

        public async Task<bool> ConfirmDeviceCode(IRecoveryFlowService flowService, string code)
        {
            var clientId = flowService.Context.ClientId;
            var credentials = await _credentialsRepository.GetAsync(clientId);
            var publicKeyAddress = credentials.Address;
            if (string.IsNullOrWhiteSpace(publicKeyAddress))
            {
                throw new InvalidOperationException($"Unable to validate signature because the client with Id {clientId} has no address in the credentials");
            }

            if (VerifyMessage(publicKeyAddress, flowService.Context.SignChallengeMessage, code))
            {
                await flowService.DeviceVerifiedCompleteAsync();
                return true;
            }

            await flowService.DeviceVerificationFailAsync();
            return false;
        }

        public async Task<bool> ConfirmSecretPhrasesCode(IRecoveryFlowService flowService, string code)
        {
            var clientId = flowService.Context.ClientId;
            var credentials = await _credentialsRepository.GetAsync(clientId);
            var publicKeyAddress = credentials.Address;
            if (publicKeyAddress == null)
            {
                throw new InvalidOperationException($"Unable to validate signature because the client with Id {clientId} has no address in the credentials");
            }

            if (VerifyMessage(publicKeyAddress, flowService.Context.SignChallengeMessage, code))
            {
                await flowService.SecretPhrasesCompleteAsync();
                return true;
            }

            await flowService.SecretPhrasesVerificationFailAsync();
            return false;
        }

        private static bool VerifyMessage(string pubKeyAddress, string message, string signedMessage)
        {
            var address = new BitcoinPubKeyAddress(pubKeyAddress);
            try
            {
                return address.VerifyMessage(message, signedMessage);
            }
            catch
            {
                return false;
            }
        }
    }
}
