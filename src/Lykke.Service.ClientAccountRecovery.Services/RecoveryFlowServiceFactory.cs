using System;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class RecoveryFlowServiceFactory : IRecoveryFlowServiceFactory
    {
        private readonly ILifetimeScope _container;
        private readonly IRecoveryLogRepository _repository;

        public RecoveryFlowServiceFactory(ILifetimeScope container, IRecoveryLogRepository repository)
        {
            _container = container;
            _repository = repository;
        }

        public IRecoveryFlowService InitiateNew(string clientId)
        {
            var initialContext = new RecoveryContext
            {
                RecoveryId = Guid.NewGuid().ToString(),
                ClientId = clientId
            };
            var service = _container.Resolve<IRecoveryFlowService>(TypedParameter.From(initialContext));
            return service;
        }

        public async Task<IRecoveryFlowService> FindExisted(string recoveryId)
        {
            var log = await _repository.GetAsync(recoveryId);
            if (log == null)
            {
                return null;
            }
            var context = log.ActualStatus;
            var service = _container.Resolve<IRecoveryFlowService>(TypedParameter.From(context));
            return service;
        }
    }
}
