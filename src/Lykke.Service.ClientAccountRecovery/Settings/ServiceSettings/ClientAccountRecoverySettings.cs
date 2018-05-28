using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class ClientAccountRecoverySettings
    {
        public DbSettings Db { get; set; }
    }
}
