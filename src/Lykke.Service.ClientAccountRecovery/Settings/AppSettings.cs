using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccountRecovery.Settings.ServiceSettings;
using Lykke.Service.ConfirmationCodes.Client;
using Lykke.Service.Kyc.Client;
using Lykke.Service.PersonalData.Settings;

namespace Lykke.Service.ClientAccountRecovery.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public ClientAccountRecoverySettings ClientAccountRecoveryService { get; set; }

        public ClientAccountServiceClientSettings ClientAccountClient { get; set; }

        public ConfirmationCodesServiceClientSettings ConfirmationCodesClient { get; set; }

        public KycServiceClientSettings KycServiceClient { get; set; }

        public PersonalDataServiceClientSettings PersonalDataServiceClient { get ; set; }

        public SessionServiceClientSettings SessionServiceClient { get ; set; }
    }
}
