﻿using System.Threading.Tasks;

namespace Lykke.Service.ClientAccountRecovery.Core
{
    public interface IWalletCredentials
    {
        string Address { get; }
    }

    public interface IWalletCredentialsRepository
    {
        Task<IWalletCredentials> GetAsync(string clientId);
    }
}
