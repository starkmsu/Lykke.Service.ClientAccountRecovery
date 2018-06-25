using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.AzureRepositories
{
    [UsedImplicitly]
    public class RecoveryLogRepository : IRecoveryLogRepository
    {
        private readonly INoSQLTableStorage<LogTableEntity> _storage;

        public RecoveryLogRepository(INoSQLTableStorage<LogTableEntity> storage)
        {
            _storage = storage;
        }

        public async Task<RecoveryUnit> GetAsync(string recoveryId)
        {
            var entities = await _storage.GetDataAsync(recoveryId);
            var log = entities.Select(e => e.Convert());
            return new RecoveryUnit(log.ToArray());
        }


        public Task InsertAsync(RecoveryContext context)
        {
            var entity = LogTableEntity.CreateNew(context);
            return _storage.InsertAsync(entity);
        }

        public Task DeleteAsync(string recoveryId, int seqNo)
        {
            return _storage.DeleteIfExistAsync(LogTableEntity.GetPartitionKey(recoveryId), LogTableEntity.GetRowKey(seqNo));
        }
    }
}
