using System;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.Kyc.Abstractions.Domain.Verification;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.SettingsReader;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class RecoveryFlowServiceFactory : IRecoveryFlowServiceFactory
    {
        private readonly ILifetimeScope _container;
        private readonly IRecoveryLogRepository _repository;
        private readonly IKycStatusService _kycStatusService;

        public RecoveryFlowServiceFactory(ILifetimeScope container,
            IRecoveryLogRepository repository,
            IKycStatusService kycStatusService)
        {
            _container = container;
            _repository = repository;
            _kycStatusService = kycStatusService;
        }

        public async Task<IRecoveryFlowService> InitiateNew(string clientId)
        {
            var initialContext = new RecoveryContext
            {
                RecoveryId = Guid.NewGuid().ToString(),
                ClientId = clientId,
                KycPassed = await IsKycPassed(clientId)
            };
            var recoveryConditions = _container.Resolve<IReloadingManager<RecoveryConditions>>().CurrentValue;
            var service = _container.Resolve<IRecoveryFlowService>(
                TypedParameter.From(initialContext),
                TypedParameter.From(recoveryConditions));
            return service;
        }

        public async Task<IRecoveryFlowService> FindExisted(string recoveryId)
        {
            var log = await _repository.GetAsync(recoveryId);
            if (log.Empty)
            {
                return null;
            }
            var context = log.ActualStatus;
            context.KycPassed = await IsKycPassed(context.ClientId);
            var recoveryConditions = _container.Resolve<IReloadingManager<RecoveryConditions>>().CurrentValue;

            var service = _container.Resolve<IRecoveryFlowService>(
                TypedParameter.From(context),
                TypedParameter.From(recoveryConditions));
            return service;
        }


        private async Task<bool> IsKycPassed(string clientId)
        {
            var status = await _kycStatusService.GetKycStatusAsync(clientId);
            return status == KycStatus.Ok;
        }
    }
}
