using System.Net;
using System.Threading.Tasks;
using Autofac;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.ClientAccountRecovery.Client;
using Lykke.Service.ClientAccountRecovery.Client.Models.Enums;
using Lykke.Service.ClientAccountRecovery.Client.Models.Recovery;
using NUnit.Framework;
using Refit;

namespace Lykke.Service.ClientAccountRecovery.Tests
{
    [TestFixture]
    [Ignore("Only for manual running")]
    public class ClientTest
    {
        private readonly IContainer _container;

        private static readonly ClientAccountRecoveryServiceClientSettings Settings =
            new ClientAccountRecoveryServiceClientSettings
            {
                ServiceUrl = "http://localhost:5000",
                ApiKey = "1"
            };

        public ClientTest()
        {
            var builder = new ContainerBuilder();
            builder.RegisterClientAccountRecoveryServiceClient(Settings);
            _container = builder.Build();
        }

        [Test]
        public async Task RegistrationTest()
        {
            var client = _container.Resolve<IClientAccountRecoveryServiceClient>();
            var callResult = await client.RecoveryApi.StartNewRecoveryAsync(new NewRecoveryRequest
            {
                ClientId = "sdfddfsdfdfsf",
                Ip = "myIp",
                UserAgent = "MyUserAgent"
            });
            Assert.NotNull(callResult);
        }

        [Test]
        public void Approve_InvalidApiKey_ReturnApiExceptionWith401UnauthorizedStatusCode()
        {
            var builder = new ContainerBuilder();
            var settings = new ClientAccountRecoveryServiceClientSettings
            {
                ServiceUrl = "http://localhost:5000",
                ApiKey = "InvalidKey"
            };
            builder.RegisterClientAccountRecoveryServiceClient(settings);
            var container = builder.Build();

            var client = container.Resolve<IClientAccountRecoveryServiceClient>();
            var apiException = Assert.ThrowsAsync<ApiException>(() => client.RecoveryApi.ApproveChallengeAsync(
                new ApproveChallengeRequest
                {
                    AgentId = "dfdfd",
                    Challenge = Challenge.Device,
                    CheckResult = CheckResult.Approved,
                    RecoveryId = "sdffeererere"
                }));

            Assert.Equals(apiException.StatusCode, HttpStatusCode.Unauthorized);
        }

        [Test]
        public void Approve_ThrowClientExceptionWith404NotFoundStatusCode()
        {
            var client = _container.Resolve<IClientAccountRecoveryServiceClient>();
            var clientApiException = Assert.ThrowsAsync<ClientApiException>(() =>
                client.RecoveryApi.ApproveChallengeAsync(new ApproveChallengeRequest
                {
                    AgentId = "dfdfd",
                    Challenge = Challenge.Device,
                    CheckResult = CheckResult.Approved,
                    RecoveryId = "sdffeererere"
                }));

            Assert.Equals(clientApiException.HttpStatusCode, HttpStatusCode.Unauthorized);
        }
    }
}
