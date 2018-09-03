using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ClientAccountRecovery.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SessionServiceClientSettings
    {
        [HttpCheck("/api/isalive")]
        public string SessionServiceUrl { get; set; }
    }
}
