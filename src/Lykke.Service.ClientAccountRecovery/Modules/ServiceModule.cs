using Autofac;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccountRecovery.AzureRepositories;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ClientAccountRecovery.Services;
using Lykke.Service.ClientAccountRecovery.Settings;
using Lykke.Service.ConfirmationCodes.Client;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.Kyc.Client;
using Lykke.SettingsReader;

namespace Lykke.Service.ClientAccountRecovery.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies

        public ServiceModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => _settings.Nested(n => n.ClientAccountRecoveryService.RecoveryConditions));

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.RegisterInstance(_settings);

            builder.RegisterType<RecoveryStateRepository>()
                .As<IRecoveryStateRepository>();

            builder.RegisterType<StateRepository>()
                .As<IStateRepository>();

            builder.RegisterType<RecoveryLogRepository>()
                .As<IRecoveryLogRepository>();

            builder.RegisterType<RecoveryFlowServiceFactory>()
                .As<IRecoveryFlowServiceFactory>();

            builder.RegisterType<RecoveryFlowServiceFactory>()
                .As<IRecoveryFlowServiceFactory>();


            builder.RegisterType<RecoveryFlowService>()
                .As<IRecoveryFlowService>();

            builder.RegisterType<SmsSender>()
                .As<ISmsSender>();

            builder.RegisterType<EmailSender>()
                .As<IEmailSender>();

            builder.RegisterType<SelfieNotificationSender>()
                .As<ISelfieNotificationSender>();

            builder.RegisterType<ChallengeManager>()
                .As<IChallengeManager>();

            builder.RegisterType<ChallengesValidator>()
                .As<IChallengesValidator>(); 
            
            builder.RegisterType<WalletCredentialsRepository>()
                .As<IWalletCredentialsRepository>();

            builder.Register(c => new BrutForceDetector(c.Resolve<IStateRepository>(),
                    c.Resolve<IRecoveryFlowServiceFactory>(),
                    _settings.CurrentValue.ClientAccountRecoveryService.RecoveryConditions))
                .As<IBrutForceDetector>();

            RegisterStorage(builder);
            RegisterClients(builder);
        }

        private void RegisterClients(ContainerBuilder builder)
        {
            builder.RegisterLykkeServiceClient(_settings.Nested(r => r.ClientAccountClient.ServiceUrl).CurrentValue);
            builder.RegisterConfirmationCodesClient(_settings.Nested(r => r.ConfirmationCodesClient).CurrentValue, _log);
            builder.Register(c => new KycStatusServiceClient(c.Resolve<IReloadingManager<AppSettings>>().Nested(r => r.KycServiceClient).CurrentValue, c.Resolve<ILog>()))
                .As<IKycStatusService>();
        }

        private void RegisterStorage(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                return AzureTableStorage<StateTableEntity>.Create(c.Resolve<IReloadingManager<AppSettings>>().Nested(r => r.ClientAccountRecoveryService.Db.RecoveryActivitiesConnString), "AccountRecoveries", c.Resolve<ILog>());
            }).As<INoSQLTableStorage<StateTableEntity>>()
                .SingleInstance();

            builder.Register(c =>
            {
                return AzureTableStorage<LogTableEntity>.Create(c.Resolve<IReloadingManager<AppSettings>>().Nested(r => r.ClientAccountRecoveryService.Db.RecoveryActivitiesConnString), "AccountRecoveryEvents", c.Resolve<ILog>());
            }).As<INoSQLTableStorage<LogTableEntity>>()
                .SingleInstance();

            builder.Register(c => AzureTableStorage<WalletCredentialsEntity>
                    .Create(c.Resolve<IReloadingManager<AppSettings>>().Nested(r => r.ClientAccountRecoveryService.Db.ClientPersonalInfoConnString), "WalletCredentials", c.Resolve<ILog>()))
                .As<INoSQLTableStorage<WalletCredentialsEntity>>()
                .SingleInstance();
        }
    }
}
