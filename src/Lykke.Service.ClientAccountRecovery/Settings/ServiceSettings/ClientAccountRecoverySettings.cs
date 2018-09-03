using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ClientAccountRecovery.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class ClientAccountRecoverySettings
    {
        public DbSettings Db { get; set; }

        public string ApiKey { get; set; }

        [Optional]
        public int? SelfieImageMaxSizeMBytes { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }

        public RecoveryConditions RecoveryConditions { get; set; }

    }
}
