using System.Threading.Tasks;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ConfirmationCodes.Client;
using Lykke.Service.ConfirmationCodes.Client.Models.Request;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfirmationCodesClient _conformationClient;
        private readonly IClientAccountClient _accountClient;

        public EmailSender(IClientAccountClient accountClient, IConfirmationCodesClient conformationClient)
        {
            _accountClient = accountClient;
            _conformationClient = conformationClient;
        }

        public async Task SendCodeAsync(string clientId)
        {
            var clientModel = await _accountClient.GetByIdAsync(clientId);
            await _conformationClient.SendEmailConfirmationAsync(new SendEmailConfirmationRequest
            {
                Email = clientModel.Email,
                PartnerId = clientModel.PartnerId,
                IsPriority = false
            });
        }
    }
}
