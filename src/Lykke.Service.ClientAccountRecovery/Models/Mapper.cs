using System;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public static class ChallengeEx
    {
        public static Challenge MapToChallenge(this State state)
        {
            switch (state)
            {
                case State.AwaitSecretPhrases:
                    return Challenge.Words;
                case State.AwaitDeviceVerification:
                    return Challenge.Device;
                case State.AwaitSmsVerification:
                    return Challenge.Sms;
                case State.AwaitEmailVerification:
                    return Challenge.Email;
                case State.AwaitKycVerification:
                    return Challenge.Selfie;
                case State.KycInProgress:
                    return Challenge.Selfie;
                case State.AwaitPinCode:
                    return Challenge.Pin;
                case State.PasswordChangeFrozen:
                case State.PasswordChangeSuspended:
                case State.CallSupport:
                case State.Transfer:
                case State.PasswordChangeAllowed:
                case State.PasswordChangeForbidden:
                    return Challenge.None;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }

    public static class ProgressEx
    {
        public static Progress MapToProgress(this State state)
        {
            switch (state)
            {
                case State.RecoveryStarted:
                case State.AwaitSecretPhrases:
                case State.AwaitDeviceVerification:
                case State.AwaitSmsVerification:
                case State.AwaitEmailVerification:
                case State.AwaitKycVerification:
                    return Progress.Ongoing;
                case State.KycInProgress:
                    return Progress.WaitingForSupport;
                case State.AwaitPinCode:
                    return Progress.Ongoing;
                case State.PasswordChangeFrozen:
                    return Progress.Frozen;
                case State.PasswordChangeSuspended:
                    return Progress.Suspended;
                case State.CallSupport:
                case State.Transfer:
                    return Progress.WaitingForSupport;
                case State.PasswordChangeAllowed:
                    return Progress.Allowed;
                case State.PasswordChangeForbidden:
                    return Progress.Suspended;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}
