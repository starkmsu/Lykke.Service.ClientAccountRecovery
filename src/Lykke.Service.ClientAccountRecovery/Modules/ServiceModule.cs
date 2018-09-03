using Autofac;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccountRecovery.AzureRepositories;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ClientAccountRecovery.Services;
using Lykke.Service.ClientAccountRecovery.Settings;
using Lykke.Service.ConfirmationCodes.Client;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.Kyc.Client;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.Session.Client;
using Lykke.SettingsReader;

namespace Lykke.Service.ClientAccountRecovery.Modules
{
    internal class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public ServiceModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies


        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => _settings.CurrentValue.ClientAccountRecoveryService.RecoveryConditions);

            builder.RegisterType<RecoveryStateRepository>()
                .As<IRecoveryStateRepository>();

            builder.RegisterType<StateRepository>()
                .As<IStateRepository>();

            builder.RegisterType<RecoveryLogRepository>()
                .As<IRecoveryLogRepository>();

            builder.Register(context => new RecoveryConditionsService(_settings.CurrentValue.ClientAccountRecoveryService
                    .RecoveryConditions))
                .As<IRecoveryConditionsService>();

            builder.RegisterType<RecoveryFlowServiceFactory>()
                .As<IRecoveryFlowServiceFactory>();
                           
            builder.RegisterType<RecoveryFlowService>()
                .As<IRecoveryFlowService>();

            builder.RegisterType<RecoveryTokenService>()
                .As<IRecoveryTokenService>();

            builder.RegisterType<SmsSender>()
                .As<ISmsSender>();

            builder.RegisterType<EmailSender>()
                .As<IEmailSender>();

            builder.RegisterType<SelfieNotificationSender>()
                .As<ISelfieNotificationSender>();

            builder.RegisterType<ChallengeManager>()
                .As<IChallengeManager>();

            builder.RegisterType<SmsValidator>();
            builder.RegisterType<EmailValidator>();
            builder.RegisterType<PinValidator>();
            builder.RegisterType<SecretPhrasesValidator>();
            builder.RegisterType<DeviceValidator>();

            builder.RegisterType<ChallengeValidatorFactory>()
                .As<IChallengeValidatorFactory>();

            builder.RegisterType<WalletCredentialsRepository>()
                .As<IWalletCredentialsRepository>();

            builder.Register(c => new BruteForceDetector(c.Resolve<IStateRepository>(),
                    c.Resolve<IRecoveryFlowServiceFactory>(),
                    _settings.CurrentValue.ClientAccountRecoveryService.RecoveryConditions))
                .As<IBruteForceDetector>();

            RegisterStorage(builder);
            RegisterClients(builder);
        }

        private void RegisterClients(ContainerBuilder builder)
        {
            builder.RegisterLykkeServiceClient(_settings.Nested(r => r.ClientAccountClient.ServiceUrl).CurrentValue);
            builder.RegisterConfirmationCodesClient(_settings.Nested(r => r.ConfirmationCodesClient).CurrentValue);
            builder.Register(c =>
                {
                    //TODO pass ILogFactory to KycStatusServiceClient when it supports the new log system
                    return new KycStatusServiceClient(_settings.Nested(r => r.KycServiceClient).CurrentValue, c.Resolve<ILogFactory>().CreateLog(this));
                })
                .As<IKycStatusService>();

            builder.Register(c =>
            {
                return new PersonalDataService(_settings.Nested(r => r.PersonalDataServiceClient).CurrentValue, c.Resolve<ILogFactory>().CreateLog(this));
            }).As<IPersonalDataService>().SingleInstance();

            // TODO:@gafanasiev Temporary solution need to pass ILogFactory to RegisterClientSessionClient.
            builder.RegisterClientSessionClient(_settings.Nested(r => r.SessionServiceClient.SessionServiceUrl).CurrentValue,
                LogFactory.Create());
        }

        private void RegisterStorage(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                return AzureTableStorage<StateTableEntity>.Create(_settings.Nested(r => r.ClientAccountRecoveryService.Db.RecoveryActivitiesConnString), "AccountRecoveries", c.Resolve<ILogFactory>());
            }).As<INoSQLTableStorage<StateTableEntity>>()
                .SingleInstance();

            builder.Register(c =>
            {
                return AzureTableStorage<LogTableEntity>.Create(_settings.Nested(r => r.ClientAccountRecoveryService.Db.RecoveryActivitiesConnString), "AccountRecoveryEvents", c.Resolve<ILogFactory>());
            }).As<INoSQLTableStorage<LogTableEntity>>()
                .SingleInstance();

            builder.Register(c => AzureTableStorage<WalletCredentialsEntity>
                    .Create(_settings.Nested(r => r.ClientAccountRecoveryService.Db.ClientPersonalInfoConnString), "WalletCredentials", c.Resolve<ILogFactory>()))
                .As<INoSQLTableStorage<WalletCredentialsEntity>>()
                .SingleInstance();
        }
    }
}
