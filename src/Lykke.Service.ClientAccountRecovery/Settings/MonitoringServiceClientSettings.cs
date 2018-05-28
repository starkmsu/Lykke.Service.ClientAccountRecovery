using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ClientAccountRecovery.Settings
{
    public class MonitoringServiceClientSettings
    {
        [HttpCheck("api/isalive", false)]
        public string MonitoringServiceUrl { get; set; }
    }
}
