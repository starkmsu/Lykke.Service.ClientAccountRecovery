using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ClientAccountRecovery.Services;
using NSubstitute;
using NUnit.Framework;

namespace Lykke.Service.ClientAccountRecovery.Tests
{
    [TestFixture]
    public class ChallengeManagerTest
    {
        private ChallengeManager _challengeManager;
        private IChallengesValidator _challengesValidator;
        private ISelfieNotificationSender _selfieNotificationSender;
        private IRecoveryFlowService _flowService;

        [SetUp]
        public void SetUp()
        {
            _challengesValidator = Substitute.For<IChallengesValidator>();
            _selfieNotificationSender = Substitute.For<ISelfieNotificationSender>();
            _flowService = Substitute.For<IRecoveryFlowService>();
            _challengeManager = new ChallengeManager(_challengesValidator, _selfieNotificationSender);
        }

        [Test]
        public void ShouldSendSmsCode()
        {
            _challengeManager.ExecuteAction(Challenge.Sms, Action.Complete, "42", _flowService);

            _challengesValidator.Received().ConfirmSmsCode(_flowService, "42");
        }

        [Test]
        public void ShouldSendEmailCode()
        {
            _challengeManager.ExecuteAction(Challenge.Email, Action.Complete, "42", _flowService);

            _challengesValidator.Received().ConfirmEmailCode(_flowService, "42");
        }

        [Test]
        public void ShouldSendSelfieCode()
        {
            _challengeManager.ExecuteAction(Challenge.Selfie, Action.Complete, "42", _flowService);

            _selfieNotificationSender.Send(_flowService, "42");
        }
    }
}
