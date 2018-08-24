using System;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Exceptions;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ClientAccountRecovery.Tests.RecoveryTokenService.Data;
using Lykke.Service.Session.AutorestClient.Models;
using Lykke.Service.Session.Client;
using Lykke.Service.Session.Client.Exceptions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Lykke.Service.ClientAccountRecovery.Tests.RecoveryTokenService
{
    [TestFixture]
    public class RecoveryTokenServiceTests
    {
        private IClientSessionsClient _clientSessionsClient;
        
        private IRecoveryTokenService _recoveryTokenService;

        [SetUp]
        public void SetUp()
        {
            _clientSessionsClient = Substitute.For<IClientSessionsClient>();
            _recoveryTokenService = new Services.RecoveryTokenService(_clientSessionsClient);

        }

        [Test]
        public void GetTokenPayloadAsync_StateTokenIsNull_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _recoveryTokenService.GetTokenPayloadAsync<string>(null));
        }

        [Test]
        public void GetTokenPayloadAsync_StateTokenIsInvalid_ThrowsInvalidRecoveryTokenException()
        {
            // Arrange
            _clientSessionsClient.JwtDecodeAsync(null).ThrowsForAnyArgs(info => new ErrorResponseException());

            // Assert
            Assert.ThrowsAsync<InvalidRecoveryTokenException>(
                () => _recoveryTokenService.GetTokenPayloadAsync<string>("Invalid token.")
            );
        }

        [Test]
        public void GetRecoveryIdAsync_StateTokenIsNull_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _recoveryTokenService.GetRecoveryIdAsync(null));
        }

        [Test]
        public void GetRecoveryIdAsync_StateTokenIsInvalid_ThrowsInvalidRecoveryTokenException()
        {
            // Arrange
            _clientSessionsClient.JwtDecodeAsync(null).ThrowsForAnyArgs(info => new ErrorResponseException());

            // Assert
            Assert.ThrowsAsync<InvalidRecoveryTokenException>(
                () => _recoveryTokenService.GetRecoveryIdAsync("Invalid token.")
            );
        }

        [Test]
        public void GenerateTokenAsync_ContextIsNull_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _recoveryTokenService.GenerateTokenAsync(null));
        }

        /// <summary>
        /// Verify that state maps to correct recovery token type.
        /// </summary>
        /// <param name="state">Flow state.</param>
        /// <param name="tokenType">Recovery token type.</param>
        [TestCaseSource(typeof(GenerateTokenAsyncTokenTypes))]
        public void GenerateTokenAsync_State_UseCorrectTokenType(State state, string tokenType)
        {
            // Arrange
            var expectedTokenType = tokenType;
            var actualTokenType = string.Empty;

            var flow = Substitute.For<IRecoveryFlowService>();
            var context = new RecoveryContext
            {
                State = state
            };

            flow.Context.Returns(context);

            // Act
            _clientSessionsClient
                .JwtGenerateAsync(null)
                .ReturnsForAnyArgs(x =>
                {
                    var jwtGenerateRequest = (JwtGenerateRequest) x[0];
                    actualTokenType = jwtGenerateRequest.TokenType.ToString();
                    return new JwtGenerateResponse();
                });

            _recoveryTokenService.GenerateTokenAsync(context);

            // Assert
            Assert.AreEqual(expectedTokenType, actualTokenType);
        }

    }
}
