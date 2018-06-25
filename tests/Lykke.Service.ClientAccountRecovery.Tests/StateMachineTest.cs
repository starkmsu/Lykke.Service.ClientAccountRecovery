using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ClientAccountRecovery.Services;
using NSubstitute;
using Xunit;

namespace Lykke.Service.ClientAccountRecovery.Tests
{
    public class StateMachineTest
    {
        private readonly RecoveryFlowService _flowService;
        private readonly ISmsSender _smsSender;
        private readonly IEmailSender _emailSender;
        private readonly ISelfieNotificationSender _selfieNotificationSender;
        private readonly IStateRepository _stateRepository;
        private RecoveryContext _attr;

        public StateMachineTest()
        {
            _smsSender = Substitute.For<ISmsSender>();
            _emailSender = Substitute.For<IEmailSender>();
            _selfieNotificationSender = Substitute.For<ISelfieNotificationSender>();
            _stateRepository = Substitute.For<IStateRepository>();
            _attr = new RecoveryContext { State = State.RecoveryStarted };
            _flowService = new RecoveryFlowService(_smsSender, _emailSender, _selfieNotificationSender, _stateRepository, _attr);
        }

        [Theory]
        [InlineData(SS.Ok, SS.Ignore, SS.Ok, SS.Ok, SS.Ignore, SS.Ignore, SS.Ignore, State.PasswordChangeAllowed, 3)]
        [InlineData(SS.Ok, SS.Ignore, SS.Ok, SS.Skip, SS.Skip, SS.Ignore, SS.Ignore, State.CallSupport, 4.1)]
        [InlineData(SS.Ok, SS.Ignore, SS.Skip, SS.Ok, SS.Skip, SS.Ignore, SS.Ignore, State.CallSupport, 4.2)]
        [InlineData(SS.Ok, SS.Ignore, SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Ignore, State.PasswordChangeAllowed, 5.1)]
        [InlineData(SS.Ok, SS.Ignore, SS.Ok, SS.Skip, SS.Ok, SS.Ok, SS.Ignore, State.PasswordChangeAllowed, 5.2)]
        [InlineData(SS.Ok, SS.Ignore, SS.Ok, SS.Skip, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 5.3)]
        [InlineData(SS.Ok, SS.Ignore, SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Ignore, State.CallSupport, 6.1)]
        [InlineData(SS.Ok, SS.Ignore, SS.Skip, SS.Skip, SS.Skip, SS.Ignore, SS.Ignore, State.CallSupport, 6.2)]
        [InlineData(SS.Ok, SS.Ignore, SS.Skip, SS.Skip, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 6.3)]
        [InlineData(SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Ok, SS.Ok, SS.Ignore, State.PasswordChangeAllowed, 8.1)]
        [InlineData(SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 8.2)]
        [InlineData(SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Skip, SS.Ignore, SS.Ok, State.PasswordChangeFrozen, 9.1)]
        [InlineData(SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Skip, SS.Ignore, SS.Skip, State.CallSupport, 10)]
        [InlineData(SS.Skip, SS.Ok, SS.Ok, SS.Skip, SS.Ok, SS.Ok, SS.Ok, State.PasswordChangeFrozen, 11.1)]
        [InlineData(SS.Skip, SS.Ok, SS.Ok, SS.Skip, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 11.2)]
        [InlineData(SS.Skip, SS.Ok, SS.Ok, SS.Skip, SS.Skip, SS.Ignore, SS.Ok, State.CallSupport, 12)]
        [InlineData(SS.Skip, SS.Ok, SS.Ok, SS.Skip, SS.Skip, SS.Ignore, SS.Skip, State.CallSupport, 13)]
        [InlineData(SS.Skip, SS.Ok, SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Ignore, State.CallSupport, 14.1)]
        [InlineData(SS.Skip, SS.Ok, SS.Skip, SS.Ok, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 14.2)]
        [InlineData(SS.Skip, SS.Ok, SS.Skip, SS.Ok, SS.Skip, SS.Ignore, SS.Ignore, State.CallSupport, 15)]
        [InlineData(SS.Skip, SS.Ok, SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Ignore, State.CallSupport, 16.1)]
        [InlineData(SS.Skip, SS.Ok, SS.Skip, SS.Skip, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 16.2)]
        [InlineData(SS.Skip, SS.Ok, SS.Skip, SS.Skip, SS.Skip, SS.Ignore, SS.Ignore, State.PasswordChangeForbidden, 17)]
        [InlineData(SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Ok, SS.Ignore, State.Transfer, 19.1)]
        [InlineData(SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 19.2)]
        [InlineData(SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Skip, SS.Ignore, SS.Ok, State.Transfer, 20)]
        [InlineData(SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Skip, SS.Ignore, SS.Skip, State.CallSupport, 21)]
        [InlineData(SS.Skip, SS.Skip, SS.Ok, SS.Skip, SS.Ok, SS.Ok, SS.Ok, State.Transfer, 22.1)]
        [InlineData(SS.Skip, SS.Skip, SS.Ok, SS.Skip, SS.Ok, SS.Ok, SS.Skip, State.CallSupport, 22.2)]
        [InlineData(SS.Skip, SS.Skip, SS.Ok, SS.Skip, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 22.3)]
        [InlineData(SS.Skip, SS.Skip, SS.Ok, SS.Skip, SS.Skip, SS.Ignore, SS.Ok, State.CallSupport, 23)]
        [InlineData(SS.Skip, SS.Skip, SS.Ok, SS.Skip, SS.Skip, SS.Ignore, SS.Skip, State.CallSupport, 24)]
        [InlineData(SS.Skip, SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Ok, SS.Ignore, State.CallSupport, 25.1)]
        [InlineData(SS.Skip, SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 25.2)]
        [InlineData(SS.Skip, SS.Skip, SS.Skip, SS.Ok, SS.Skip, SS.Ignore, SS.Ignore, State.CallSupport, 26)]
        [InlineData(SS.Skip, SS.Skip, SS.Skip, SS.Skip, SS.Ok, SS.Ok, SS.Ignore, State.CallSupport, 27.1)]
        [InlineData(SS.Skip, SS.Skip, SS.Skip, SS.Skip, SS.Ok, SS.Fail, SS.Ignore, State.CallSupport, 27.2)]
        [InlineData(SS.Skip, SS.Skip, SS.Skip, SS.Skip, SS.Skip, SS.Ignore, SS.Ignore, State.PasswordChangeForbidden, 28)]
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

        [Theory]
        [InlineData(State.PasswordChangeAllowed)]
        [InlineData(State.CallSupport)]
        [InlineData(State.PasswordChangeSuspended)]
        [InlineData(State.PasswordChangeFrozen)]
        public async Task ShouldJumpFromAllAnyStateToSupportState(State supportState)
        {
            var allStates = Enum.GetValues(typeof(State)).Cast<State>().Except(new[] { supportState, State.RecoveryStarted, State.PasswordUpdated });
            foreach (var state in allStates)
            {
                var context = new RecoveryContext
                {
                    State = state
                };
                var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _selfieNotificationSender, _stateRepository, context);
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
                Assert.Equal(supportState, stateMachine.Context.State);
            }
        }

        [Fact]
        public async Task ShouldAllowToGoFinalState()
        {
            var context = new RecoveryContext
            {
                State = State.PasswordChangeAllowed
            };
            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _selfieNotificationSender, _stateRepository, context);

            await stateMachine.UpdatePasswordComplete();

            Assert.Equal(State.PasswordUpdated, stateMachine.Context.State);
        }

        [Fact]
        public async Task ShouldSealState()
        {
            var context = new RecoveryContext
            {
                State = State.PasswordUpdated
            };
            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _selfieNotificationSender, _stateRepository, context);



            await Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.UpdatePasswordComplete());
            await Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.JumpToSuspendAsync());
            await Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.JumpToAllowAsync());
            await Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.JumpToFrozenAsync());
            await Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.JumpToSupportAsync());
            await Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.StartRecoveryAsync());
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
