using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Stateless;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class RecoveryFlowService : IRecoveryFlowService
    {
        private readonly ISmsSender _smsSender;
        private readonly IEmailSender _emailSender;
        private readonly ISelfieSender _selfieSender;
        private readonly IStateRepository _stateRepository;
        private readonly RecoveryContext _ctx;
        private readonly StateMachine<State, Trigger> _stateMachine;
        private const int MaxRecoveryAttempts = 3;

        public RecoveryContext Context => _ctx;

        public RecoveryFlowService(ISmsSender smsSender, IEmailSender emailSender, ISelfieSender selfieSender, IStateRepository stateRepository, RecoveryContext ctx)
        {
            _smsSender = smsSender;
            _emailSender = emailSender;
            _selfieSender = selfieSender;
            _stateRepository = stateRepository;
            _ctx = ctx;
            _stateMachine = new StateMachine<State, Trigger>(ctx.State);
            _stateMachine.OnTransitionedAsync(OnTransitionActionAsync);
            _stateMachine.OnUnhandledTrigger(UnhandledTriggerAction);
            Configure();
        }

        private void UnhandledTriggerAction(State state, Trigger trigger)
        {
            var msg = $"Unable to change state {state} by action {trigger}. Permitted actions {string.Join(',', _stateMachine.GetPermittedTriggers())}";
            throw new InvalidActionException(msg);
        }

        private Task OnTransitionActionAsync(StateMachine<State, Trigger>.Transition transition)
        {
            _ctx.State = transition.Destination;
            _ctx.Time = DateTime.UtcNow;
            _ctx.Action = transition.Trigger;
            _ctx.SeqNo++; // Under any condition SenNo should be incremented only one time per client call
            return _stateRepository.InsertAsync(_ctx);
        }

        private void Configure()
        {
            _stateMachine.Configure(State.RecoveryStarted)
                    .Permit(Trigger.RecoveryRequest, State.AwaitSecretPhrases);

            _stateMachine.Configure(State.AwaitSecretPhrases)
                .PermitSupportStates()
                .Permit(Trigger.SecretPhrasesComplete, State.AwaitSmsVerification) // 3 - 6
                .Permit(Trigger.SecretPhrasesSkip, State.AwaitDeviceVerification); // 8 - 28
                                                                                   //
            _stateMachine.Configure(State.AwaitDeviceVerification)
                .PermitSupportStates()
                .Permit(Trigger.DeviceVerificationComplete, State.AwaitSmsVerification) //For all cases unconditional go to SMS verification
                .Permit(Trigger.DeviceVerificationSkip, State.AwaitSmsVerification);

            _stateMachine.Configure(State.AwaitSmsVerification)
                .OnEntryAsync(SendSmsAsync)
                .PermitSupportStates()
                .Permit(Trigger.SmsVerificationComplete, State.AwaitEmailVerification) // For all cases unconditional go to email verification
                .Permit(Trigger.SmsVerificationSkip, State.AwaitEmailVerification) // For all cases unconditional go to email verification
                .PermitIf(Trigger.SmsVerificationFail, State.AwaitEmailVerification, () => _ctx.SmsRecoveryAttempts > MaxRecoveryAttempts)
                .PermitReentryIf(Trigger.SmsVerificationFail, () => _ctx.SmsRecoveryAttempts <= MaxRecoveryAttempts)
                .PermitIf(Trigger.SmsVerificationRestart, State.AwaitEmailVerification, () => _ctx.SmsRecoveryAttempts > MaxRecoveryAttempts)
                .PermitReentryIf(Trigger.SmsVerificationRestart, () => _ctx.SmsRecoveryAttempts <= MaxRecoveryAttempts);

            _stateMachine.Configure(State.AwaitEmailVerification)
                .OnEntryAsync(SendEmailAsync)
                .PermitSupportStates()
                .PermitIf(Trigger.EmailVerificationComplete, State.PasswordChangeAllowed, () => _ctx.HasSecretPhrases && _ctx.SmsVerified) // 3
                .PermitIf(Trigger.EmailVerificationComplete, State.AwaitKycVerification, () => !(_ctx.HasSecretPhrases && _ctx.SmsVerified)) // All other cases
                .PermitIf(Trigger.EmailVerificationSkip, State.AwaitKycVerification) // All cases
                .PermitIf(Trigger.EmailVerificationFail, State.AwaitKycVerification, () => _ctx.EmailRecoveryAttempts > MaxRecoveryAttempts)
                .PermitReentryIf(Trigger.EmailVerificationFail, () => _ctx.EmailRecoveryAttempts <= MaxRecoveryAttempts)
                .PermitIf(Trigger.EmailVerificationRestart, State.AwaitKycVerification, () => _ctx.EmailRecoveryAttempts > MaxRecoveryAttempts)
                .PermitReentryIf(Trigger.EmailVerificationRestart, () => _ctx.EmailRecoveryAttempts <= MaxRecoveryAttempts);

            _stateMachine.Configure(State.AwaitKycVerification)
                .PermitSupportStates()
                .PermitIf(Trigger.SelfieVerificationSkip, State.CallSupport, () => _ctx.HasSecretPhrases && !_ctx.DeviceVerificationRequested && _ctx.SmsVerified ^ _ctx.EmailVerified) // 4
                .PermitIf(Trigger.SelfieVerificationRequest, State.KycInProgress, () => _ctx.HasSecretPhrases && !_ctx.DeviceVerificationRequested && _ctx.SmsVerified ^ _ctx.EmailVerified) // 5
                .PermitIf(Trigger.SelfieVerificationRequest, State.KycInProgress, () => _ctx.HasSecretPhrases && !_ctx.DeviceVerificationRequested && !_ctx.SmsVerified && !_ctx.EmailVerified) // 6
                .PermitIf(Trigger.SelfieVerificationSkip, State.CallSupport, () => _ctx.HasSecretPhrases && !_ctx.DeviceVerificationRequested && !_ctx.SmsVerified && !_ctx.EmailVerified) // 6.1
                .PermitIf(Trigger.SelfieVerificationRequest, State.KycInProgress, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified) // 8
                .PermitIf(Trigger.SelfieVerificationSkip, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified) // 9, 10
                .PermitIf(Trigger.SelfieVerificationRequest, State.KycInProgress, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified) // 11
                .PermitIf(Trigger.SelfieVerificationSkip, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified) // 12, 13
                .PermitIf(Trigger.SelfieVerificationRequest, State.KycInProgress, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && !_ctx.SmsVerified && _ctx.EmailVerified) // 14
                .PermitIf(Trigger.SelfieVerificationSkip, State.CallSupport, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && !_ctx.SmsVerified && _ctx.EmailVerified) // 15
                .PermitIf(Trigger.SelfieVerificationRequest, State.KycInProgress, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.EmailVerified) // 16
                .PermitIf(Trigger.SelfieVerificationSkip, State.PasswordChangeForbidden, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.EmailVerified) // 17
                .PermitIf(Trigger.SelfieVerificationRequest, State.KycInProgress, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified) // 19
                .PermitIf(Trigger.SelfieVerificationSkip, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified) // 20, 21
                .PermitIf(Trigger.SelfieVerificationRequest, State.KycInProgress, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified) // 22
                .PermitIf(Trigger.SelfieVerificationSkip, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified) // 23, 24
                .PermitIf(Trigger.SelfieVerificationRequest, State.KycInProgress, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && !_ctx.SmsVerified && _ctx.EmailVerified) // 25
                .PermitIf(Trigger.SelfieVerificationSkip, State.CallSupport, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && !_ctx.SmsVerified && _ctx.EmailVerified) // 26
                .PermitIf(Trigger.SelfieVerificationRequest, State.KycInProgress, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.EmailVerified) // 27
                .PermitIf(Trigger.SelfieVerificationSkip, State.PasswordChangeForbidden, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.EmailVerified); // 28


            _stateMachine.Configure(State.KycInProgress)
                .OnEntryAsync(SendKycVerification)
                .PermitSupportStates()
                .PermitIf(Trigger.SelfieVerificationComplete, State.PasswordChangeAllowed, () => _ctx.HasSecretPhrases && !_ctx.DeviceVerificationRequested && _ctx.SmsVerified ^ _ctx.EmailVerified)  // 5
                .PermitIf(Trigger.SelfieVerificationComplete, State.CallSupport, () => _ctx.HasSecretPhrases && !_ctx.DeviceVerificationRequested && !_ctx.SmsVerified && !_ctx.EmailVerified)  // 6
                .PermitIf(Trigger.SelfieVerificationComplete, State.PasswordChangeAllowed, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified) // 8
                .PermitIf(Trigger.SelfieVerificationComplete, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified) // 11
                .PermitIf(Trigger.SelfieVerificationComplete, State.CallSupport, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && !_ctx.SmsVerified && _ctx.EmailVerified) // 14
                .PermitIf(Trigger.SelfieVerificationComplete, State.CallSupport, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.EmailVerified) // 16
                .PermitIf(Trigger.SelfieVerificationComplete, State.Transfer, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified) // 19
                .PermitIf(Trigger.SelfieVerificationComplete, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified) // 22
                .PermitIf(Trigger.SelfieVerificationComplete, State.CallSupport, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && !_ctx.SmsVerified && _ctx.EmailVerified) // 25
                .PermitIf(Trigger.SelfieVerificationComplete, State.CallSupport, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.EmailVerified) // 27
                .PermitIf(Trigger.SelfieVerificationFail, State.CallSupport); // All other cases go to support

            _stateMachine.Configure(State.AwaitPinCode)
                .PermitSupportStates()
                .PermitIf(Trigger.PinComplete, State.PasswordChangeFrozen, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified && !_ctx.KycPassed) // 9
                .PermitIf(Trigger.PinSkip, State.CallSupport, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified && !_ctx.KycPassed) // 10
                .PermitIf(Trigger.PinComplete, State.PasswordChangeFrozen, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified && _ctx.KycPassed) // 11
                .PermitIf(Trigger.PinComplete, State.CallSupport, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified && !_ctx.KycPassed) // 12
                .PermitIf(Trigger.PinSkip, State.CallSupport, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified && !_ctx.KycPassed) // 13
                .PermitIf(Trigger.PinComplete, State.Transfer, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified && !_ctx.KycPassed) // 20
                .PermitIf(Trigger.PinSkip, State.CallSupport, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified && !_ctx.KycPassed) // 21
                .PermitIf(Trigger.PinComplete, State.Transfer, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified && _ctx.KycPassed) // 22.1
                .PermitIf(Trigger.PinSkip, State.CallSupport, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified && _ctx.KycPassed) // 22.2
                .PermitIf(Trigger.PinComplete, State.CallSupport, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified && !_ctx.KycPassed) // 23
                .PermitIf(Trigger.PinSkip, State.CallSupport, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified && !_ctx.KycPassed); // 24


            _stateMachine.Configure(State.Transfer)
                .PermitSupportStates();

            _stateMachine.Configure(State.PasswordChangeForbidden)
                .PermitSupportStates();


            _stateMachine.Configure(State.PasswordChangeFrozen)
               .PermitReentry(Trigger.JumpToFrozen)
                .Permit(Trigger.JumpToAllowed, State.PasswordChangeAllowed)
                .Permit(Trigger.JumpToCallSupport, State.CallSupport)
                .Permit(Trigger.JumpToSuspended, State.PasswordChangeSuspended);

            _stateMachine.Configure(State.PasswordChangeSuspended)
                .PermitReentry(Trigger.JumpToSuspended)
                .Permit(Trigger.JumpToAllowed, State.PasswordChangeAllowed)
                .Permit(Trigger.JumpToCallSupport, State.CallSupport)
                .Permit(Trigger.JumpToFrozen, State.PasswordChangeFrozen);
            //            
            _stateMachine.Configure(State.CallSupport)
                .PermitReentry(Trigger.JumpToCallSupport)
                .Permit(Trigger.JumpToAllowed, State.PasswordChangeAllowed)
                .Permit(Trigger.JumpToFrozen, State.PasswordChangeFrozen)
                .Permit(Trigger.JumpToSuspended, State.PasswordChangeSuspended);
            //            
            _stateMachine.Configure(State.PasswordChangeAllowed)
                .PermitReentry(Trigger.JumpToAllowed)
                .Permit(Trigger.JumpToCallSupport, State.CallSupport)
                .Permit(Trigger.JumpToFrozen, State.PasswordChangeFrozen)
                .Permit(Trigger.JumpToSuspended, State.PasswordChangeSuspended)
                .Permit(Trigger.UpdatePassword, State.PasswordUpdated);
        }




        private Task SendSmsAsync()
        {
            return _smsSender.SendCodeAsync(_ctx.ClientId);
        }

        private Task SendEmailAsync()
        {
            return _emailSender.SendCodeAsync(_ctx.ClientId);
        }

        private Task SendKycVerification()
        {
            return _selfieSender.Send(_ctx.ClientId);
        }

        public Task StartRecoveryAsync()
        {
            return _stateMachine.FireAsync(Trigger.RecoveryRequest);
        }

        public Task SecretPhrasesCompleteAsync()
        {
            _ctx.HasSecretPhrases = true;
            return _stateMachine.FireAsync(Trigger.SecretPhrasesComplete);
        }

        public Task SecretPhrasesSkipAsync()
        {
            _ctx.HasSecretPhrases = false;
            return _stateMachine.FireAsync(Trigger.SecretPhrasesSkip);
        }

        public Task DeviceVerifiedCompleteAsync()
        {
            _ctx.DeviceVerified = true;
            _ctx.DeviceVerificationRequested = true;
            return _stateMachine.FireAsync(Trigger.DeviceVerificationComplete);
        }

        public Task DeviceVerificationSkip()
        {
            _ctx.DeviceVerified = false;
            _ctx.DeviceVerificationRequested = true;
            return _stateMachine.FireAsync(Trigger.DeviceVerificationSkip);
        }

        public Task SmsVerificationComplete()
        {
            _ctx.SmsVerified = true;
            return _stateMachine.FireAsync(Trigger.SmsVerificationComplete);
        }

        public Task SmsVerificationSkip()
        {
            _ctx.SmsVerified = false;
            return _stateMachine.FireAsync(Trigger.SmsVerificationSkip);
        }

        public Task SmsVerificationRestart()
        {
            _ctx.SmsRecoveryAttempts++;
            _ctx.SmsVerified = false;
            return _stateMachine.FireAsync(Trigger.SmsVerificationRestart);
        }

        public Task SmsVerificationFailed()
        {
            _ctx.SmsRecoveryAttempts++;
            _ctx.SmsVerified = false;
            return _stateMachine.FireAsync(Trigger.SmsVerificationFail);
        }

        public Task EmailVerificationComplete()
        {
            _ctx.EmailVerified = true;
            return _stateMachine.FireAsync(Trigger.EmailVerificationComplete);
        }

        public Task UpdatePasswordComplete()
        {
            return _stateMachine.FireAsync(Trigger.UpdatePassword);
        }

        public Task EmailVerificationSkip()
        {
            _ctx.EmailVerified = false;
            return _stateMachine.FireAsync(Trigger.EmailVerificationSkip);
        }

        public Task EmailVerificationRestart()
        {
            _ctx.EmailRecoveryAttempts++;
            _ctx.EmailVerified = false;
            return _stateMachine.FireAsync(Trigger.EmailVerificationRestart);
        }

        public Task EmailVerificationFailed()
        {
            _ctx.EmailRecoveryAttempts++;
            _ctx.EmailVerified = false;
            return _stateMachine.FireAsync(Trigger.EmailVerificationFail);
        }

        public Task SelfieVerificationRequest()
        {
            _ctx.KycPassed = false;
            return _stateMachine.FireAsync(Trigger.SelfieVerificationRequest);
        }

        public Task SelfieVerificationSkip()
        {
            _ctx.KycPassed = false;
            return _stateMachine.FireAsync(Trigger.SelfieVerificationSkip);
        }

        public Task SelfieVerificationFail()
        {
            _ctx.KycPassed = false;
            return _stateMachine.FireAsync(Trigger.SelfieVerificationFail);
        }

        public Task SelfieVerificationComplete()
        {
            _ctx.KycPassed = true;
            return _stateMachine.FireAsync(Trigger.SelfieVerificationComplete);
        }

        public Task PinCodeVerificationComplete()
        {
            _ctx.HasPin = true;
            return _stateMachine.FireAsync(Trigger.PinComplete);
        }

        public Task PinCodeVerificationSkip()
        {
            _ctx.HasPin = false;
            return _stateMachine.FireAsync(Trigger.PinSkip);
        }


        #region Only for support

        public Task JumpToAllowAsync()
        {
            return _stateMachine.FireAsync(Trigger.JumpToAllowed);
        }


        public Task JumpToSupportAsync()
        {
            return _stateMachine.FireAsync(Trigger.JumpToCallSupport);
        }

        public Task JumpToFrozenAsync()
        {
            return _stateMachine.FireAsync(Trigger.JumpToFrozen);
        }


        public Task JumpToSuspendAsync()
        {
            return _stateMachine.FireAsync(Trigger.JumpToSuspended);
        }
        #endregion


    }

    internal static class StateMachineEx
    {
        public static StateMachine<State, Trigger>.StateConfiguration PermitSupportStates(this StateMachine<State, Trigger>.StateConfiguration stateMachine)
        {
            var result = stateMachine
                .Permit(Trigger.JumpToAllowed, State.PasswordChangeAllowed)
                .Permit(Trigger.JumpToCallSupport, State.CallSupport)
                .Permit(Trigger.JumpToFrozen, State.PasswordChangeFrozen)
                .Permit(Trigger.JumpToSuspended, State.PasswordChangeSuspended);
            return result;
        }

        //        public static StateMachine<State, Trigger>.StateConfiguration PermitReentrySupportStates(this StateMachine<State, Trigger>.StateConfiguration stateMachine)
        //        {
        //            var result = stateMachine
        //                .PermitReentry(Trigger.JumpToAllowed)
        //                .PermitReentry(Trigger.JumpToCallSupport)
        //                .PermitReentry(Trigger.JumpToFrozen)
        //                .PermitReentry(Trigger.JumpToSuspended);
        //            return result;
        //        }
    }
}
