using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.AzureRepositories
{
    public class RecoveryLogRepository : IRecoveryLogRepository
    {
        private readonly INoSQLTableStorage<LogTableEntity> _storage;

        public RecoveryLogRepository(INoSQLTableStorage<LogTableEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IEnumerable<RecoveryContext>> GetAsync(string recoveryId)
        {
            var entities = await _storage.GetDataAsync(recoveryId);
            return entities.Select(e => e.Convert());
        }


        public Task InsertAsync(RecoveryContext context)
        {
            var entity = LogTableEntity.CreateNew(context);
            return _storage.InsertAsync(entity);
        }

        public Task DeleteAsync(string recoveryId, DateTime time)
        {
            return _storage.DeleteIfExistAsync(LogTableEntity.GetPartitionKey(recoveryId), LogTableEntity.GetRowKey(time));
        }
    }
}
