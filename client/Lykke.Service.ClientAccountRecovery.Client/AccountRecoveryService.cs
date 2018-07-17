using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Client.AutoRestClient;
using Lykke.Service.ClientAccountRecovery.Client.AutoRestClient.Models;
using Microsoft.Rest;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    internal class AccountRecoveryService : IAccountRecoveryService
    {
        private readonly IClientAccountRecoveryServiceClient _client;

        public AccountRecoveryService(Uri baseUri, HttpClient client, ServiceClientCredentials credentials)
        {
            _client = new ClientAccountRecoveryServiceClient(baseUri, client, credentials);
        }

        public Task<object> IsAliveAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _client.IsAliveAsync(cancellationToken);
        }

        private async Task<T> HandleErrorCode<T>(Func<Task<HttpOperationResponse<T>>> func)
        {
            using (var result = await func().ConfigureAwait(false))
            {
                AssertException(result);
                return result.Body;
            }
        }

        private async Task HandleErrorCode(Func<Task<HttpOperationResponse>> func)
        {
            using (var result = await func().ConfigureAwait(false))
            {
                AssertException(result);
            }
        }

        private static void AssertException(IHttpOperationResponse result)
        {
            switch (result.Response.StatusCode)
            {
                case HttpStatusCode.InternalServerError:
                    throw new ServerErrorException(result.Response.Content.AsString());
                case HttpStatusCode.NotFound:
                    throw new NotFoundException(result.Response.Content.AsString());
                case HttpStatusCode.Conflict:
                    throw new ConflictException(result.Response.Content.AsString());
                case HttpStatusCode.BadRequest:
                    throw new BadRequestException(result.Response.Content.AsString());
                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedException(result.Response.Content.AsString()); 
                case HttpStatusCode.Forbidden:
                    throw new ForbiddenException(result.Response.Content.AsString());
            }
        }

        public Task<NewRecoveryResponse> StartNewRecoveryAsync(NewRecoveryRequest request = default(NewRecoveryRequest), CancellationToken cancellationToken = default(CancellationToken))
        {
            return HandleErrorCode(() => _client.StartNewRecoveryWithHttpMessagesAsync(request, null, cancellationToken));
        }

        public Task<RecoveryStatusResponse> GetRecoveryStatusAsync(string recoveryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return HandleErrorCode(() => _client.GetRecoveryStatusWithHttpMessagesAsync(recoveryId, null, cancellationToken));
        }

        public Task<OperationStatus> SubmitChallengeAsync(ChallengeRequest request = default(ChallengeRequest), CancellationToken cancellationToken = default(CancellationToken))
        {
            return HandleErrorCode(() => _client.SubmitChallengeWithHttpMessagesAsync(request, null, cancellationToken));
        }

        public Task UpdatePasswordAsync(PasswordRequest request = default(PasswordRequest), CancellationToken cancellationToken = default(CancellationToken))
        {
            return HandleErrorCode(() => _client.UpdatePasswordWithHttpMessagesAsync(request, null, cancellationToken));
        }

        public Task ApproveChallengeAsync(ApproveChallengeRequest request = default(ApproveChallengeRequest), CancellationToken cancellationToken = default(CancellationToken))
        {
            return HandleErrorCode(() => _client.ApproveChallengeWithHttpMessagesAsync(request, null, cancellationToken));
        }

        public Task SubmitResolutionAsync(ResolutionRequest request = default(ResolutionRequest), CancellationToken cancellationToken = default(CancellationToken))
        {
            return HandleErrorCode(() => _client.SubmitResolutionWithHttpMessagesAsync(request, null, cancellationToken));
        }

        public Task<IList<ClientRecoveryHistoryResponse>> GetClientRecoveriesAsync(string clientId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return HandleErrorCode(() => _client.GetClientRecoveriesWithHttpMessagesAsync(clientId, null, cancellationToken));
        }

        public Task<IList<RecoveryTraceResponse>> GetRecoveryTraceAsync(string recoveryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return HandleErrorCode(() => _client.GetRecoveryTraceWithHttpMessagesAsync(recoveryId, null, cancellationToken));
        }
    }
}
