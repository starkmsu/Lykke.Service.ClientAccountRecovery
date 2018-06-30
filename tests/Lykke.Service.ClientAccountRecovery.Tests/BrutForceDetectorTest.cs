using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
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
        private const string clientID = "1";

        [SetUp]
        public void Setup()
        {
            _stateRepository = Substitute.For<IStateRepository>();
            var rc = new RecoveryConditions();
            _detector = new BrutForceDetector(_stateRepository, rc);
        }

        [Test]
        public async Task OnlyOneStateInProgressAllowed([Values]State state)
        {
            var allowed = new[]
            {
                State.PasswordChangeAllowed,
                State.PasswordUpdated,
                State.PasswordChangeSuspended,
                State.PasswordChangeForbidden
            };

            AddRecovery(state);
            var result = await _detector.IsNewRecoveryAllowedAsync(clientID);
            var expected = allowed.Contains(state);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public async Task ShouldBlockBrutForce(
            [Values(State.PasswordChangeSuspended, State.PasswordChangeForbidden)] State firstState,
            [Values(State.PasswordChangeSuspended, State.PasswordChangeForbidden)] State secondState,
            [Values(State.PasswordChangeSuspended, State.PasswordChangeForbidden)] State thirdSate,
            [Values] State forthState)
        {
            AddRecovery(firstState, secondState, thirdSate, forthState); // By default 3 unsuccessful attempts allowed
            var result = await _detector.IsNewRecoveryAllowedAsync(clientID);
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public async Task ShouldNotBlockLosers(
            [Values(State.PasswordChangeSuspended, State.PasswordChangeForbidden)] State firstState,
            [Values(State.PasswordChangeSuspended, State.PasswordChangeForbidden)] State secondState)
        {
            AddRecovery(firstState, secondState); // By default 3 unsuccessful attempts allowed
            var result = await _detector.IsNewRecoveryAllowedAsync(clientID);
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public async Task ShouldNotBlockNewcomers()
        {
            var result = await _detector.IsNewRecoveryAllowedAsync(clientID);
            Assert.That(result, Is.EqualTo(true));
        }

        private void AddRecovery(params State[] lastState)
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
        }
    }
}
