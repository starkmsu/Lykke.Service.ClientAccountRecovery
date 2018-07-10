using System.Threading.Tasks;
using AzureStorage;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.ClientAccountRecovery.AzureRepositories
{
    public class WalletCredentialsEntity : TableEntity, IWalletCredentials
    {
        public string Address { get; set; }

        public static class ByClientId
        {
            public static string GeneratePartitionKey()
            {
                return "Wallet";
            }

            public static string GenerateRowKey(string clientId)
            {
                return clientId;
            }
        }
    }

    [UsedImplicitly]
    public class WalletCredentialsRepository : IWalletCredentialsRepository
    {
        private readonly INoSQLTableStorage<WalletCredentialsEntity> _tableStorage;

        public WalletCredentialsRepository(INoSQLTableStorage<WalletCredentialsEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IWalletCredentials> GetAsync(string clientId)
        {
            var partitionKey = WalletCredentialsEntity.ByClientId.GeneratePartitionKey();
            var rowKey = WalletCredentialsEntity.ByClientId.GenerateRowKey(clientId);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }
    }
}
