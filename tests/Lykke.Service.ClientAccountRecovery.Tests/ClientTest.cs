using System.Threading.Tasks;
using Autofac;
using Common.Log;
using Lykke.Service.ClientAccountRecovery.Client;
using Lykke.Service.ClientAccountRecovery.Client.Models;
using Xunit;

namespace Lykke.Service.ClientAccountRecovery.Tests
{
    public class ClientTest
    {
        [Fact]
        public async Task RegistrationTest()
        {
            var builder = new ContainerBuilder();
            builder.RegisterClientAccountRecoveryClient("http://localhost:5000", new LogToConsole());
            var container = builder.Build();

            var client = container.Resolve<IClientAccountRecoveryServiceClient>();
            var callResult = await client.StartNewRecoveryAsync(new NewRecoveryRequest("sdfdsf"));
        }    
        
        [Fact]
        public async Task ShouldThrowException()
        {
            var builder = new ContainerBuilder();
            builder.RegisterClientAccountRecoveryClient("http://localhost:5000", new LogToConsole());
            var container = builder.Build();

            var client = container.Resolve<IClientAccountRecoveryServiceClient>();
            var callResult = await client.StartNewRecoveryAsync(new NewRecoveryRequest());
        }
    }
}
