using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Settings.ServiceSettings;
using Lykke.Service.ClientAccountRecovery.Settings.SlackNotifications;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ClientAccountRecovery.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings
    {
        public ClientAccountRecoverySettings ClientAccountRecoveryService { get; set; }

        public SlackNotificationsSettings SlackNotifications { get; set; }

        [Optional]
        public MonitoringServiceClientSettings MonitoringServiceClient { get; set; }
    }
}
