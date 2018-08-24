using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Exceptions;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.Session.AutorestClient.Models;
using Lykke.Service.Session.Client;
using Lykke.Service.Session.Client.Exceptions;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    /// <inheritdoc />
    [UsedImplicitly]
    public class RecoveryTokenService : IRecoveryTokenService
    {
        private readonly IClientSessionsClient _clientSessionsClient;

        public RecoveryTokenService(IClientSessionsClient clientSessionsClient)
        {
            _clientSessionsClient = clientSessionsClient;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidRecoveryTokenException">Thrown when <paramref name="stateToken"/> is invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stateToken"/> is null or whitespace.</exception>
        public async Task<T> GetTokenPayloadAsync<T>(string stateToken)
        {
            if (string.IsNullOrWhiteSpace(stateToken))
                throw new ArgumentNullException(nameof(stateToken));

            try
            {
                var jwtDecodeRequest = new JwtDecodeRequest(stateToken);
                var jwtDecodeResponse =
                    await _clientSessionsClient.JwtDecodeAsync(jwtDecodeRequest);

                return JObject.FromObject(jwtDecodeResponse.JwtData.Payload).ToObject<T>();
            }
            catch (ErrorResponseException e)
            {
                throw new InvalidRecoveryTokenException("Invalid token.", e);
            }
        }

        /// <inheritdoc />
        /// <exception cref="InvalidRecoveryTokenException">Thrown when <paramref name="stateToken"/> is invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stateToken"/> is null or whitespace.</exception>
        public async Task<string> GetRecoveryIdAsync(string stateToken)
        {
            var payload = await GetTokenPayloadAsync<RecoveryTokenPayload>(stateToken);
            return payload.RecoveryId;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public async Task<string> GenerateTokenAsync(RecoveryContext context)
        {
            if(context == null)
                throw new ArgumentNullException(nameof(context));

            var progress = context.State.MapToProgress();
            var challenge = context.State.MapToChallenge();

            var payload = new RecoveryTokenPayload
            {
                RecoveryId = context.RecoveryId,
                Challenge = challenge,
            };

            var tokenType = GetTokenType(progress, challenge);
            var jwtData = new JwtData(null, payload);
            var jwtGenerateRequest = new JwtGenerateRequest(true, tokenType, jwtData);

            var jwtGenerateResponse = await _clientSessionsClient.JwtGenerateAsync(jwtGenerateRequest);
            
            return jwtGenerateResponse.Token;
        }

        private JwtTypeName GetTokenType(Progress progress, Challenge challenge)
        {
            // If recovery process is frozen we should generate an infinite token.
            if (progress == Progress.Frozen)
                return JwtTypeName.Infinite;

            switch (challenge)
            {
                // If challenge could take a lot of time to complete we should generate an infinite token.
                case Challenge.Selfie:
                    return JwtTypeName.Infinite;
                default:
                    return JwtTypeName.Default;
            }
        }
    }
}
