using System;
using System.Diagnostics;
using System.Linq;
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
    public class StateMachineTest
    {
        private RecoveryFlowService _flowService;
        private ISmsSender _smsSender;
        private IEmailSender _emailSender;
        private IStateRepository _stateRepository;
        private RecoveryContext _attr;
        private RecoveryConditions _recoveryConditions;

        [SetUp]
        public void SetUp()
        {
            _recoveryConditions = new RecoveryConditions
            {
                EmailCodeMaxAttempts = 3,
                SmsCodeMaxAttempts = 3,
                FrozenPeriodInDays = 5
            };
            _smsSender = Substitute.For<ISmsSender>();
            _emailSender = Substitute.For<IEmailSender>();
            _stateRepository = Substitute.For<IStateRepository>();
            _attr = new RecoveryContext { State = State.RecoveryStarted };
            _flowService = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, _attr);
        }


        [TestCase(SS.Ok, SS.Ignore, SS.Ok, SS.Ok, SS.Ignore, SS.Ignore, SS.Ignore, State.PasswordChangeAllowed, 3)]
        [TestCase(SS.Ok, SS.Ignore, SS.Ok, SS.Skip, SS.Skip, SS.Ignore, SS.Ignore, State.CallSupport, 4.1)]
        [TestCase(SS.Ok, SS.Ignore, SS.Skip, SS.Ok, SS.Skip, SS.Ignore, SS.Ignore, State.CallSupport, 4.2)]
        [TestCase(SS.Ok, SS.Ignore, SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Ignore, State.PasswordChangeAllowed, 5.1)]
        [TestCase(SS.Ok, SS.Ignore, SS.Ok, SS.Skip, SS.Ok, SS.Ok, SS.Ignore, State.PasswordChangeAllowed, 5.2)]
        [TestCase(SS.Ok, SS.Ignore, SS.Ok, SS.Skip, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 5.3)]
        [TestCase(SS.Ok, SS.Ignore, SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Ignore, State.CallSupport, 6.1)]
        [TestCase(SS.Ok, SS.Ignore, SS.Skip, SS.Skip, SS.Skip, SS.Ignore, SS.Ignore, State.CallSupport, 6.2)]
        [TestCase(SS.Ok, SS.Ignore, SS.Skip, SS.Skip, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 6.3)]
        [TestCase(SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Ok, SS.Ok, SS.Ignore, State.PasswordChangeAllowed, 8.1)]
        [TestCase(SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 8.2)]
        [TestCase(SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Skip, SS.Ignore, SS.Ok, State.PasswordChangeFrozen, 9.1)]
        [TestCase(SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Skip, SS.Ignore, SS.Skip, State.CallSupport, 10)]
        [TestCase(SS.Skip, SS.Ok, SS.Ok, SS.Skip, SS.Ok, SS.Ok, SS.Ok, State.PasswordChangeFrozen, 11.1)]
        [TestCase(SS.Skip, SS.Ok, SS.Ok, SS.Skip, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 11.2)]
        [TestCase(SS.Skip, SS.Ok, SS.Ok, SS.Skip, SS.Skip, SS.Ignore, SS.Ok, State.CallSupport, 12)]
        [TestCase(SS.Skip, SS.Ok, SS.Ok, SS.Skip, SS.Skip, SS.Ignore, SS.Skip, State.CallSupport, 13)]
        [TestCase(SS.Skip, SS.Ok, SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Ignore, State.CallSupport, 14.1)]
        [TestCase(SS.Skip, SS.Ok, SS.Skip, SS.Ok, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 14.2)]
        [TestCase(SS.Skip, SS.Ok, SS.Skip, SS.Ok, SS.Skip, SS.Ignore, SS.Ignore, State.CallSupport, 15)]
        [TestCase(SS.Skip, SS.Ok, SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Ignore, State.CallSupport, 16.1)]
        [TestCase(SS.Skip, SS.Ok, SS.Skip, SS.Skip, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 16.2)]
        [TestCase(SS.Skip, SS.Ok, SS.Skip, SS.Skip, SS.Skip, SS.Ignore, SS.Ignore, State.PasswordChangeForbidden, 17)]
        [TestCase(SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Ok, SS.Ignore, State.Transfer, 19.1)]
        [TestCase(SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 19.2)]
        [TestCase(SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Skip, SS.Ignore, SS.Ok, State.Transfer, 20)]
        [TestCase(SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Skip, SS.Ignore, SS.Skip, State.CallSupport, 21)]
        [TestCase(SS.Skip, SS.Skip, SS.Ok, SS.Skip, SS.Ok, SS.Ok, SS.Ok, State.Transfer, 22.1)]
        [TestCase(SS.Skip, SS.Skip, SS.Ok, SS.Skip, SS.Ok, SS.Ok, SS.Skip, State.CallSupport, 22.2)]
        [TestCase(SS.Skip, SS.Skip, SS.Ok, SS.Skip, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 22.3)]
        [TestCase(SS.Skip, SS.Skip, SS.Ok, SS.Skip, SS.Skip, SS.Ignore, SS.Ok, State.CallSupport, 23)]
        [TestCase(SS.Skip, SS.Skip, SS.Ok, SS.Skip, SS.Skip, SS.Ignore, SS.Skip, State.CallSupport, 24)]
        [TestCase(SS.Skip, SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Ignore, State.CallSupport, 25.1)]
        [TestCase(SS.Skip, SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 25.2)]
        [TestCase(SS.Skip, SS.Skip, SS.Skip, SS.Ok, SS.Skip, SS.Ignore, SS.Ignore, State.CallSupport, 26)]
        [TestCase(SS.Skip, SS.Skip, SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Ignore, State.CallSupport, 27.1)]
        [TestCase(SS.Skip, SS.Skip, SS.Skip, SS.Skip, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 27.2)]
        [TestCase(SS.Skip, SS.Skip, SS.Skip, SS.Skip, SS.Skip, SS.Ignore, SS.Ignore, State.PasswordChangeForbidden, 28)]
        public async Task ShouldHaveCorrectFinalState(SS secretPhrases, SS deviceVerified, SS sms, SS email, SS selfieReceived, SS kycPass, SS pin, State resolution, double line)
        {
            await _flowService.StartRecoveryAsync();
            await Fire(secretPhrases, _flowService.SecretPhrasesCompleteAsync, _flowService.SecretPhrasesSkipAsync);
            await Fire(deviceVerified, _flowService.DeviceVerifiedCompleteAsync, _flowService.DeviceVerificationSkip);
            await Fire(sms, _flowService.SmsVerificationComplete, _flowService.SmsVerificationSkip, _flowService.SmsVerificationFailed, _flowService.SmsVerificationRestart);
            await Fire(email, _flowService.EmailVerificationComplete, _flowService.EmailVerificationSkip, _flowService.EmailVerificationFailed, _flowService.EmailVerificationRestart);
            await Fire(selfieReceived, _flowService.SelfieVerificationRequest, _flowService.SelfieVerificationSkip);
            await Fire(kycPass, _flowService.SelfieVerificationComplete, null, _flowService.SelfieVerificationFail);
            await Fire(pin, _flowService.PinCodeVerificationComplete, _flowService.PinCodeVerificationSkip);

            if (resolution != _flowService.Context.State)
            {
                Assert.True(false, $"Expected {resolution} but was {_flowService.Context.State} for line {line}");
            }
        }

        [Test]
        [TestCase(State.PasswordChangeAllowed)]
        [TestCase(State.CallSupport)]
        [TestCase(State.PasswordChangeSuspended)]
        [TestCase(State.PasswordChangeFrozen)]
        public async Task ShouldJumpFromAllAnyStateToSupportState(State supportState)
        {
            var allStates = Enum.GetValues(typeof(State)).Cast<State>().Except(new[] { supportState, State.RecoveryStarted, State.PasswordUpdated });
            foreach (var state in allStates)
            {
                var context = new RecoveryContext
                {
                    State = state
                };
                var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);
                switch (supportState)
                {
                    case State.PasswordChangeAllowed:
                        await stateMachine.JumpToAllowAsync();
                        break;
                    case State.CallSupport:
                        await stateMachine.JumpToSupportAsync();
                        break;
                    case State.PasswordChangeSuspended:
                        await stateMachine.JumpToSuspendAsync();
                        break;
                    case State.PasswordChangeFrozen:
                        await stateMachine.JumpToFrozenAsync();
                        break;
                }
                Assert.That(supportState, Is.EqualTo(stateMachine.Context.State));
            }
        }

        [Test]
        public async Task ShouldAllowToGoFinalState()
        {
            var context = new RecoveryContext
            {
                State = State.PasswordChangeAllowed
            };
            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            await stateMachine.UpdatePasswordComplete();

            Assert.That(State.PasswordUpdated, Is.EqualTo(stateMachine.Context.State));
        }

        [Test]
        public void ShouldSealState()
        {
            var context = new RecoveryContext
            {
                State = State.PasswordUpdated
            };
            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);



            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.UpdatePasswordComplete());
            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.JumpToSuspendAsync());
            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.JumpToAllowAsync());
            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.JumpToFrozenAsync());
            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.JumpToSupportAsync());
            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.StartRecoveryAsync());
        }


        [Test]
        public async Task ShouldSendSmsRequest()
        {
            var context = new RecoveryContext
            {
                State = State.AwaitDeviceVerification
            };
            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);
            await stateMachine.DeviceVerifiedCompleteAsync();

            await _smsSender.ReceivedWithAnyArgs().SendCodeAsync("1");
        }

        [Test]
        public async Task ShouldUnfreezeAfterPeriod()
        {
            var context = new RecoveryContext
            {
                State = State.AwaitSecretPhrases
            };
            _recoveryConditions.FrozenPeriodInDays = TimeSpan.FromSeconds(1).TotalDays;

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);
            await stateMachine.JumpToFrozenAsync();


            await Task.Delay(TimeSpan.FromSeconds(2));
            await stateMachine.TryUnfreeze();

            Assert.That(stateMachine.Context.State, Is.EqualTo(State.PasswordChangeAllowed));
        }

        [Test]
        public async Task ShouldKeepFrozen()
        {
            var context = new RecoveryContext
            {
                State = State.AwaitSecretPhrases
            };
            _recoveryConditions.FrozenPeriodInDays = TimeSpan.FromSeconds(2).TotalDays;

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);
            await stateMachine.JumpToFrozenAsync();


            await Task.Delay(TimeSpan.FromSeconds(1));
            await stateMachine.TryUnfreeze();

            Assert.That(stateMachine.Context.State, Is.EqualTo(State.PasswordChangeFrozen));
        }


        [Test]
        public void ShouldIgnoreFreezeTrigger()
        {
            var allStates = Enum.GetValues(typeof(State)).Cast<State>().Except(new[]
            {
                State.RecoveryStarted, State.PasswordChangeFrozen, State.PasswordChangeAllowed, State.PasswordUpdated
            });
            foreach (var state in allStates)
            {
                var context = new RecoveryContext
                {
                    State = state
                };
                var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

                Assert.That(state, Is.EqualTo(stateMachine.Context.State));
            }
        }

        [Test]
        public async Task ShouldSendEmailRequest()
        {
            var context = new RecoveryContext
            {
                State = State.AwaitSmsVerification
            };
            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);
            await stateMachine.SmsVerificationComplete();

            await _emailSender.ReceivedWithAnyArgs().SendCodeAsync("1");
        }

        private Task Fire(SS state, Func<Task> ok, Func<Task> skip, Func<Task> fail = null, Func<Task> restart = null)
        {
            Debug.Assert(ok != skip);
            switch (state)
            {
                case SS.Ok:
                    return ok();
                case SS.Skip:
                    if (skip == null)
                    {
                        throw new ArgumentNullException(nameof(skip));
                    }
                    return skip();
                case SS.Fail:
                    if (fail == null)
                    {
                        throw new ArgumentNullException(nameof(fail));
                    }
                    return fail();
                case SS.Restart:
                    if (restart == null)
                    {
                        throw new ArgumentNullException(nameof(restart));
                    }
                    return restart();
            }
            return Task.CompletedTask;
        }



        public enum SS // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }
    }

}
