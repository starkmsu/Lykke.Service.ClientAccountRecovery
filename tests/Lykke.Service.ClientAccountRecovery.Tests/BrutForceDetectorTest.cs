using System;
using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ClientAccountRecovery.Services;
using NSubstitute;
using NUnit.Framework;

namespace Lykke.Service.ClientAccountRecovery.Tests
{
    [TestFixture]
    public class BrutForceDetectorTest
    {
        private BrutForceDetector _detector;
        private IStateRepository _stateRepository;
        private IRecoveryFlowServiceFactory _serviceFactory;
        private const string clientID = "1";

        [SetUp]
        public void Setup()
        {
            _stateRepository = Substitute.For<IStateRepository>();
            _serviceFactory = Substitute.For<IRecoveryFlowServiceFactory>();
            var rc = new RecoveryConditions();
            _detector = new BrutForceDetector(_stateRepository, _serviceFactory, rc);
        }

        [Test]
        public async Task ShouldBlockBrutForce([Values] State forthState)
        {
            AddRecovery(State.PasswordChangeForbidden, State.PasswordChangeForbidden, State.PasswordChangeForbidden, forthState); // By default 3 unsuccessful attempts allowed
            var result = await _detector.IsNewRecoveryAllowedAsync(clientID);
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public async Task ShouldNotBlockLosers()
        {
            AddRecovery(State.PasswordChangeForbidden, State.PasswordChangeForbidden); // By default 3 unsuccessful attempts allowed
            var result = await _detector.IsNewRecoveryAllowedAsync(clientID);
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public async Task Should_Block_PreviousRecoveries()
        {
            var flow1 = Substitute.For<IRecoveryFlowService>();
            flow1.Context.Returns(new RecoveryContext());
            var flow2 = Substitute.For<IRecoveryFlowService>();
            flow2.Context.Returns(new RecoveryContext());

            var recovery = AddRecovery(State.AwaitDeviceVerification, State.AwaitPinCode);

            _serviceFactory.FindExisted(recovery.Log[0].RecoveryId).Returns(Task.FromResult(flow1));
            _serviceFactory.FindExisted(recovery.Log[1].RecoveryId).Returns(Task.FromResult(flow2));

            await _detector.BlockPreviousRecoveries(clientID, "", "");

            await flow1.Received().JumpToForbiddenAsync();
            await flow2.Received().JumpToForbiddenAsync();
        }

        [Test]
        public async Task Should_Preserve_IpAndClientAgent()
        {
            const string agent = "MyAgent";
            const string ip = "MyIp";

            var context = new RecoveryContext();
            var flow1 = Substitute.For<IRecoveryFlowService>();
            flow1.Context.Returns(context);
            var recovery = AddRecovery(State.AwaitDeviceVerification);
            _serviceFactory.FindExisted(recovery.Log[0].RecoveryId).Returns(Task.FromResult(flow1));

            await _detector.BlockPreviousRecoveries(clientID, ip, agent);

            Assert.That(context.UserAgent, Is.EqualTo(agent));
            Assert.That(context.Ip, Is.EqualTo(ip));
        }

        [Test]
        public async Task Should_CorrectlyHandle_EmptyHistory()
        {

            await _detector.BlockPreviousRecoveries(clientID, "", "");
            await _serviceFactory.DidNotReceiveWithAnyArgs().FindExisted("");
        }

        [Test]
        public async Task ShouldNotBlockNewcomers()
        {
            var result = await _detector.IsNewRecoveryAllowedAsync(clientID);
            Assert.That(result, Is.EqualTo(true));
        }

        private RecoverySummaryForClient AddRecovery(params State[] lastState)
        {
            var summary = new RecoverySummaryForClient(clientID);

            for (var i = 0; i < lastState.Length; i++)
            {
                var state = lastState[i];
                summary.AddItem(new RecoveryUnit(new[]
                {
                    new RecoveryContext
                    {
                        RecoveryId = Guid.NewGuid().ToString(),
                        ClientId = clientID,
                        State = state,
                        SeqNo = i,
                        Time = DateTime.UtcNow.AddDays(-i)
                    }
                }));
            }

            _stateRepository.FindRecoverySummary(clientID).Returns(Task.FromResult(summary));
            return summary;
        }
    }
}
