using System.Threading.Tasks;
using Autofac;
using Lykke.Service.ClientAccountRecovery.Client;
using Lykke.Service.ClientAccountRecovery.Client.AutoRestClient.Models;
using Xunit;

namespace Lykke.Service.ClientAccountRecovery.Tests
{
    public class ClientTest
    {
        private IContainer _container;

        public ClientTest()
        {
            var builder = new ContainerBuilder();
            builder.RegisterClientAccountRecoveryClient("http://localhost:5000", "1");
            _container = builder.Build();
        }

        [Fact(Skip = "Manual")]
        public async Task RegistrationTest()
        {

            var client = _container.Resolve<IAccountRecoveryService>();
            var callResult = await client.StartNewRecoveryAsync(new NewRecoveryRequest("sdfddfsdfdfsf", "myIp", "MyUserAgent"));
            Assert.NotNull(callResult);
        }

        [Fact(Skip = "Manual")]
        public async Task ShouldThrowUnauthorizedException()
        {
            var builder = new ContainerBuilder();
            builder.RegisterClientAccountRecoveryClient("http://localhost:5000", "");
            var container = builder.Build();

            var client = container.Resolve<IAccountRecoveryService>();
            await Assert.ThrowsAsync<UnauthorizedException>(() => client.ApproveChallengeAsync(new ApproveChallengeRequest
            {
                AgentId = "dfdfd",
                Challenge = Challenge.Device,
                CheckResult = CheckResult.Approved,
                RecoveryId = "sdffeererere"
            }));
        }

        [Fact(Skip = "Manual")]
        public async Task ShouldThrowNotFoundException()
        {
            var client = _container.Resolve<IAccountRecoveryService>();
            await Assert.ThrowsAsync<NotFoundException>(() => client.ApproveChallengeAsync(new ApproveChallengeRequest
            {
                AgentId = "dfdfd",
                Challenge = Challenge.Device,
                CheckResult = CheckResult.Approved,
                RecoveryId = "sdffeererere"
            }));
        }
    }
}
