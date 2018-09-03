using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.ClientAccountRecovery.Client.Models.Recovery;
using Refit;

namespace Lykke.Service.ClientAccountRecovery.Client.Api
{
    [PublicAPI]
    public interface IRecoveryApi
    {
        /// <summary>
        ///     Starts password recovering process.
        /// </summary>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        ///     The cancellation token.
        /// </param>
        /// <exception cref="ClientApiException">
        ///     Thrown when something went wrong and we handle it.
        /// </exception>
        /// <exception cref="ApiException">
        ///     Thrown when something went wrong and we don not handle it.
        /// </exception>
        [Post("/api/recovery/token/start")]
        Task<NewRecoveryResponse> StartNewRecoveryAsync([Body] NewRecoveryRequest request,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Returns the current recovery state.
        /// </summary>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        ///     The cancellation token.
        /// </param>
        /// <exception cref="ClientApiException">
        ///     Thrown when something went wrong and we handle it.
        /// </exception>
        /// <exception cref="ApiException">
        ///     Thrown when something went wrong and we don not handle it.
        /// </exception>
        [Post("/api/recovery/token/status")]
        Task<RecoveryStatusResponse> GetRecoveryStatusAsync([Body] RecoveryStatusRequest request,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Submit challenge by providing value.
        /// </summary>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        ///     The cancellation token.
        /// </param>
        /// <exception cref="ClientApiException">
        ///     Thrown when something went wrong and we handle it.
        /// </exception>
        /// <exception cref="ApiException">
        ///     Thrown when something went wrong and we don not handle it.
        /// </exception>
        [Post("/api/recovery/token/challenge")]
        Task<SubmitChallengeResponse> SubmitChallengeAsync([Body] ChallengeRequest request,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Updates the user password.
        ///     Completes password recovery process.
        /// </summary>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        ///     The cancellation token.
        /// </param>
        /// <exception cref="ClientApiException">
        ///     Thrown when something went wrong and we handle it.
        /// </exception>
        /// <exception cref="ApiException">
        ///     Thrown when something went wrong and we don not handle it.
        /// </exception>
        [Post("/api/recovery/token/password")]
        Task UpdatePasswordAsync([Body] PasswordRequest request,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Approves user challenges. Only for support.
        /// </summary>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        ///     The cancellation token.
        /// </param>
        /// <exception cref="ClientApiException">
        ///     Thrown when something went wrong and we handle it.
        /// </exception>
        /// <exception cref="ApiException">
        ///     Thrown when something went wrong and we don not handle it.
        /// </exception>
        [Put("/api/recovery/challenge/challenge/checkResult")]
        Task ApproveChallengeAsync([Body] ApproveChallengeRequest request,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Updates current state of the recovery process. Only for support.
        /// </summary>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        ///     The cancellation token.
        /// </param>
        /// <exception cref="ClientApiException">
        ///     Thrown when something went wrong and we handle it.
        /// </exception>
        /// <exception cref="ApiException">
        ///     Thrown when something went wrong and we don not handle it.
        /// </exception>
        [Post("/api/recovery/challenge/resolution")]
        Task SubmitResolutionAsync([Body] ResolutionRequest request,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Returns brief information about all client's recoveries.
        /// </summary>
        /// <param name='clientId'>
        ///     The client id.
        /// </param>
        /// <param name='cancellationToken'>
        ///     The cancellation token.
        /// </param>
        /// <exception cref="ClientApiException">
        ///     Thrown when something went wrong and we handle it.
        /// </exception>
        /// <exception cref="ApiException">
        ///     Thrown when something went wrong and we don not handle it.
        /// </exception>
        [Get("/api/recovery/client/{clientId}")]
        Task<IList<ClientRecoveryHistoryResponse>> GetClientRecoveriesAsync(string clientId,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Returns detailed information about the recovery.
        /// </summary>
        /// <param name='recoveryId'>
        ///     The recovery id.
        /// </param>
        /// <param name='cancellationToken'>
        ///     The cancellation token.
        /// </param>
        /// <exception cref="ClientApiException">
        ///     Thrown when something went wrong and we handle it.
        /// </exception>
        /// <exception cref="ApiException">
        ///     Thrown when something went wrong and we don not handle it.
        /// </exception>
        [Get("/api/recovery/client/trace/{recoveryId}")]
        Task<IList<RecoveryTraceResponse>> GetRecoveryTraceAsync(string recoveryId,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Uploads image required to complete selfie challenge.
        /// </summary>
        /// <param name="stateToken">State token containing selfie challenge inside.</param>
        /// <param name="file">Selfie image file.</param>
        /// <param name="cancellationToken">
        ///     The cancellation token.
        /// </param>
        /// <returns>Selfie file id, in format: {clientId}/{selfieId}.{extension}</returns>
        [Multipart]
        [Post("/api/recovery/selfie")]
        Task<UploadSelfieResponse> UploadSelfieAsync(string stateToken, StreamPart file,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
