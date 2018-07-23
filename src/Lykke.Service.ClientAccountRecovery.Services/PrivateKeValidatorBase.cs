using System;
using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core;
using NBitcoin;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    public abstract class PrivateKeValidatorBase
    {
        private readonly IWalletCredentialsRepository _credentialsRepository;

        protected PrivateKeValidatorBase(IWalletCredentialsRepository credentialsRepository)
        {
            _credentialsRepository = credentialsRepository;
        }

        protected async Task<string> PublicKeyAddress(string clientId)
        {
            var credentials = await _credentialsRepository.GetAsync(clientId);
            if (credentials == null) // We should never be here, because the state machine must bypass this step
            {
                throw new InvalidOperationException($"Unable to find a credentials for client {clientId}");
            }
            var publicKeyAddress = credentials.Address;
            return publicKeyAddress;
        }

        protected static bool VerifyMessage(string pubKeyAddress, string message, string signedMessage)
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