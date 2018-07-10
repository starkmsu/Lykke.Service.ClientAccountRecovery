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


        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, State.PasswordChangeAllowed, 3)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, State.PasswordChangeAllowed, 3.5)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, State.CallSupport, 4.1)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, State.CallSupport, 4.15)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, State.CallSupport, 4.2)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, State.PasswordChangeAllowed, 5.1)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Skip, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, State.CallSupport, 5.15)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, State.PasswordChangeAllowed, 5.2)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, State.CallSupport, 5.25)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, State.CallSupport, 5.3)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, State.CallSupport, 5.3)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, State.CallSupport, 6.1)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Skip, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, State.CallSupport, 6.15)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, State.CallSupport, 6.2)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, State.CallSupport, 6.3)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, State.PasswordChangeAllowed, 8.1)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, State.CallSupport, 8.2)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ok, State.PasswordChangeFrozen, 9.1)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ok, State.PasswordChangeFrozen, 9.15)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Skip, State.CallSupport, 10)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Skip, State.CallSupport, 10.5)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ok, State.PasswordChangeFrozen, 11.1)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, State.CallSupport, 11.2)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ok, State.CallSupport, 12)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ok, State.CallSupport, 12.5)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Skip, State.CallSupport, 13)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Skip, State.CallSupport, 13.5)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, State.CallSupport, 14.1)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, State.CallSupport, 14.2)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, State.CallSupport, 15)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, State.CallSupport, 15.5)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, State.CallSupport, 16.1)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, State.CallSupport, 16.2)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, State.PasswordChangeForbidden, 17)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, State.PasswordChangeForbidden, 17.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, State.Transfer, 19.1)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, State.CallSupport, 19.2)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ok, State.Transfer, 20)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ok, State.Transfer, 20.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Skip, State.CallSupport, 21)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Skip, State.CallSupport, 21.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ok, State.Transfer, 22.1)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Skip, State.CallSupport, 22.2)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, State.CallSupport, 22.3)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ok, State.CallSupport, 23)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ok, State.CallSupport, 23.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Skip, State.CallSupport, 24)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Skip, State.CallSupport, 24.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, State.CallSupport, 25.1)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, State.CallSupport, 25.2)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, State.CallSupport, 26)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, State.CallSupport, 26.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, State.CallSupport, 27.1)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, State.CallSupport, 27.2)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, State.PasswordChangeForbidden, 28)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, State.PasswordChangeForbidden, 28.5)]
        public async Task StateMachine_WhenCaseApplied_FinalStateIsCorrect(Ss secretPhrases, Ss deviceVerified, Ss sms, Ss email, Ss kycPassed, Ss selfieReceived, Ss selfiePass, Ss pin, State resolution, double line)
        {
            _flowService.Context.KycPassed = kycPassed == Ss.Ok;
            await _flowService.StartRecoveryAsync();
            await Fire(secretPhrases, _flowService.SecretPhrasesCompleteAsync, _flowService.SecretPhrasesSkipAsync);
            await Fire(deviceVerified, _flowService.DeviceVerifiedCompleteAsync, _flowService.DeviceVerificationSkipAsync);
            await Fire(sms, _flowService.SmsVerificationCompleteAsync, _flowService.SmsVerificationSkipAsync, _flowService.SmsVerificationFailedAsync, _flowService.SmsVerificationRestartAsync);
            await Fire(email, _flowService.EmailVerificationCompleteAsync, _flowService.EmailVerificationSkipAsync, _flowService.EmailVerificationFailedAsync, _flowService.EmailVerificationRestartAsync);
            await Fire(selfieReceived, _flowService.SelfieVerificationRequestAsync, _flowService.SelfieVerificationSkipAsync);
            await Fire(selfiePass, _flowService.SelfieVerificationCompleteAsync, null, _flowService.SelfieVerificationFailAsync);
            await Fire(pin, _flowService.PinCodeVerificationCompleteAsync, _flowService.PinCodeVerificationSkipAsync);

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
        public async Task StateMachine_Allows_Switch_Support_States(State supportState)
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

            await stateMachine.UpdatePasswordCompleteAsync();

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



            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.UpdatePasswordCompleteAsync());
            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.JumpToSuspendAsync());
            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.JumpToAllowAsync());
            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.JumpToFrozenAsync());
            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.JumpToSupportAsync());
            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.StartRecoveryAsync());
        }

        [Test]
        public async Task Should_Set_Forbidden([Values]State initialState)
        {
            if (initialState == State.RecoveryStarted)
            {
                Assert.Pass();
            }
            var ignoredStates = new[]
            {
                State.PasswordChangeAllowed,
                State.PasswordUpdated,
                State.PasswordChangeSuspended,
                State.PasswordChangeForbidden,
            };
            var context = new RecoveryContext
            {
                State = initialState
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            await stateMachine.JumpToForbiddenAsync();

            if (ignoredStates.Contains(initialState))
            {
                Assert.That(stateMachine.Context.State, Is.EqualTo(initialState));
            }
            else
            {
                Assert.That(stateMachine.Context.State, Is.EqualTo(State.PasswordChangeForbidden));

            }
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
            await stateMachine.TryUnfreezeAsync();

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
            await stateMachine.TryUnfreezeAsync();

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
        public async Task Should_BlockRecovery_AfterMaxSmsAttempts()
        {
            var context = new RecoveryContext
            {
                State = State.AwaitSmsVerification
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            await stateMachine.SmsVerificationFailedAsync();
            await stateMachine.SmsVerificationFailedAsync();
            await stateMachine.SmsVerificationFailedAsync();
            await stateMachine.SmsVerificationFailedAsync();

            Assert.That(stateMachine.Context.State, Is.EqualTo(State.PasswordChangeForbidden));
        }

        [Test]
        public async Task Should_BlockRecovery_AfterMaxSmsRestarts()
        {
            var context = new RecoveryContext
            {
                State = State.AwaitSmsVerification
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            await stateMachine.SmsVerificationRestartAsync();
            await stateMachine.SmsVerificationRestartAsync();
            await stateMachine.SmsVerificationRestartAsync();
            await stateMachine.SmsVerificationRestartAsync();

            Assert.That(stateMachine.Context.State, Is.EqualTo(State.PasswordChangeForbidden));
        }

        [Test]
        public async Task Should_BlockRecovery_AfterMaxEmailsAttempts()
        {
            var context = new RecoveryContext
            {
                State = State.AwaitEmailVerification
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            await stateMachine.EmailVerificationFailedAsync();
            await stateMachine.EmailVerificationFailedAsync();
            await stateMachine.EmailVerificationFailedAsync();
            await stateMachine.EmailVerificationFailedAsync();

            Assert.That(stateMachine.Context.State, Is.EqualTo(State.PasswordChangeForbidden));
        }

        [Test]
        public async Task Should_BlockRecovery_AfterMaxEmailsRestarts()
        {
            var context = new RecoveryContext
            {
                State = State.AwaitEmailVerification
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            await stateMachine.EmailVerificationRestartAsync();
            await stateMachine.EmailVerificationRestartAsync();
            await stateMachine.EmailVerificationRestartAsync();
            await stateMachine.EmailVerificationRestartAsync();

            Assert.That(stateMachine.Context.State, Is.EqualTo(State.PasswordChangeForbidden));
        }


        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(4, true)]
        public async Task Should_BlockRecovery_AfterMaxSecretPhrasesAttempts(int attemptsNo, bool block)
        {
            var context = new RecoveryContext
            {
                State = State.AwaitSecretPhrases
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            for (int i = 0; i < attemptsNo; i++)
            {
                await stateMachine.SecretPhrasesVerificationFailAsync();
            }

            var expected = block ? State.PasswordChangeForbidden : State.AwaitSecretPhrases;
            Assert.That(stateMachine.Context.State, Is.EqualTo(expected));
        }

        [Test]
        public async Task Should_BlockRecovery_AfterFirstFailedDeviceRecoveryAttempts()
        {
            var context = new RecoveryContext
            {
                State = State.AwaitDeviceVerification
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            await stateMachine.DeviceVerificationFailAsync();

            Assert.That(stateMachine.Context.State, Is.EqualTo(State.PasswordChangeForbidden));
        }

        [Test]
        public void Allows_PasswordRecovery_OnlyInTerminalState([Values] State state)
        {
            var context = new RecoveryContext
            {
                State = state
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);


            if (state == State.PasswordChangeAllowed)
            {
                Assert.That(stateMachine.IsPasswordUpdateAllowed, Is.True);
            }
            else
            {
                Assert.That(stateMachine.IsPasswordUpdateAllowed, Is.False);
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
            await stateMachine.SmsVerificationCompleteAsync();

            await _emailSender.ReceivedWithAnyArgs().SendCodeAsync("1");
        }

        private Task Fire(Ss state, Func<Task> ok, Func<Task> skip, Func<Task> fail = null, Func<Task> restart = null)
        {
            Debug.Assert(ok != skip);
            switch (state)
            {
                case Ss.Ok:
                    return ok();
                case Ss.Skip:
                    if (skip == null)
                    {
                        throw new ArgumentNullException(nameof(skip));
                    }
                    return skip();
                case Ss.Fail:
                    if (fail == null)
                    {
                        throw new ArgumentNullException(nameof(fail));
                    }
                    return fail();
                case Ss.Restart:
                    if (restart == null)
                    {
                        throw new ArgumentNullException(nameof(restart));
                    }
                    return restart();
            }
            return Task.CompletedTask;
        }



        public enum Ss // Step state for short
        {

            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        private enum Words // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        private enum Device // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        private enum Sms // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        private enum Email // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        private enum Selfie // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        private enum SelfieCheck // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        private enum Pin // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        private enum KycPassed // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }
    }

}
