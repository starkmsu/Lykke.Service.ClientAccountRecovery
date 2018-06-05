using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Core
{
    public interface IRecoveryLogRepository
    {
        Task<IEnumerable<RecoveryContext>> GetAsync(string recoveryId);
        Task InsertAsync(RecoveryContext context);
        Task DeleteAsync(string recoveryId, DateTime time);
    }
}
