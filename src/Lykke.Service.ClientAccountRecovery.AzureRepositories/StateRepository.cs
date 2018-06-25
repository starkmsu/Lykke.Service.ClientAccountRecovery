using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.AzureRepositories
{
    public class StateRepository : IStateRepository
    {
        private readonly IRecoveryLogRepository _logRepository;
        private readonly IRecoveryStateRepository _stateRepository;
        private readonly ILog _log;

        public StateRepository(IRecoveryLogRepository logRepository, IRecoveryStateRepository stateRepository, ILog log)
        {
            _logRepository = logRepository;
            _stateRepository = stateRepository;
            _log = log.CreateComponentScope(nameof(StateRepository));
        }

        public async Task InsertAsync(RecoveryContext context)
        {
            try
            {
                await _stateRepository.InsertOrReplaceAsync(context.ClientId, context.RecoveryId);
                await _logRepository.InsertAsync(context);
            }
            catch (Exception ex)
            {
                _log.WriteWarning(nameof(InsertAsync), null, $"Can't save a new state {context.State} for user {context.ClientId} try to rollback action", ex);
                try
                {
                    await _stateRepository.DeleteAsync(context.ClientId, context.RecoveryId);
                    await _logRepository.DeleteAsync(context.RecoveryId, context.SeqNo);
                }
                catch (Exception innerEx)
                {
                    _log.WriteError(nameof(InsertAsync), null, innerEx);
                }
                _log.WriteWarning(nameof(InsertAsync), null, $"Rollback for {context.State} for {context.ClientId} successful");
            }
        }

        public async Task<RecoverySummaryForClient> GetRecoverySummary(string clientId)
        {
            var recoveries = await _stateRepository.GetAsync(clientId);
            if (!recoveries.Any())
            {
                return null;
            }
            var logItems = recoveries.Select(async r => await _logRepository.GetAsync(r.RecoveryID)).ToArray();
            await Task.WhenAll(logItems);

            var result = new RecoverySummaryForClient();
            foreach (var logItem in logItems.Select(t => t.Result))
            {
                result.AddItem(logItem);
            }

            return result;
        }
    }
}
