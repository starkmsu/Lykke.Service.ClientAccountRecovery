using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Client.AutoRestClient.Models;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    [PublicAPI]
    public interface IAccountRecoveryService
    {
        /// <summary>
        /// Checks service is alive
        /// </summary>
        /// <param name='operations'>
        /// The operations group for extension method.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        Task<object> IsAliveAsync(CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Starts password recovering process
        /// </summary>
        /// <param name='operations'>
        /// The operations group for extension method.
        /// </param>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        Task<NewRecoveryResponse> StartNewRecoveryAsync(NewRecoveryRequest request, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Returns the current recovery state
        /// </summary>
        /// <param name='operations'>
        /// The operations group for extension method.
        /// </param>
        /// <param name='recoveryId'>
        /// Recovery Id
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        Task<RecoveryStatusResponse> GetRecoveryStatusAsync(string recoveryId, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Accepts challenge values
        /// </summary>
        /// <param name='operations'>
        /// The operations group for extension method.
        /// </param>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        Task<OperationStatus> SubmitChallengeAsync(ChallengeRequest request, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Updates the user password
        /// </summary>
        /// <param name='operations'>
        /// The operations group for extension method.
        /// </param>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        Task UpdatePasswordAsync(PasswordRequest request, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Approves user challenges. Only for support.
        /// </summary>
        /// <param name='operations'>
        /// The operations group for extension method.
        /// </param>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        Task ApproveChallengeAsync(ApproveChallengeRequest request, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Updates current state of the recovery process. Only for support.
        /// </summary>
        /// <param name='operations'>
        /// The operations group for extension method.
        /// </param>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        Task SubmitResolutionAsync(ResolutionRequest request , CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Returns brief information about all client's recoveries
        /// </summary>
        /// <param name='operations'>
        /// The operations group for extension method.
        /// </param>
        /// <param name='clientId'>
        /// The client id
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        Task<IList<ClientRecoveryHistoryResponse>> GetClientRecoveriesAsync(string clientId, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Returns detailed information about the recovery
        /// </summary>
        /// <param name='operations'>
        /// The operations group for extension method.
        /// </param>
        /// <param name='recoveryId'>
        /// The recovery id
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        Task<IList<RecoveryTraceResponse>> GetRecoveryTraceAsync(string recoveryId, CancellationToken cancellationToken = default(CancellationToken));

    }
}
