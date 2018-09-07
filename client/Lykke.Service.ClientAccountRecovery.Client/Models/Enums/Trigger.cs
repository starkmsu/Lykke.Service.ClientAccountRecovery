using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Enums
{
    /// <summary>
    ///     An action that can be performed to change the state of the state machine.
    /// </summary>
    [PublicAPI]
    public enum Trigger
    {
        RecoveryRequest,
        SecretPhrasesComplete,
        SecretPhrasesSkip,
        SecretPhrasesVerificationFail,
        DeviceVerificationComplete,
        DeviceVerificationSkip,
        DeviceVerificationFail,
        SmsVerificationComplete,
        SmsVerificationRestart,
        SmsVerificationFail,
        SmsVerificationSkip,
        EmailVerificationComplete,
        EmailVerificationRestart,
        EmailVerificationFail,
        EmailVerificationSkip,
        PinComplete,
        PinSkip,
        PinFail,
        SelfieVerificationRequest,
        SelfieVerificationComplete,
        SelfieVerificationSkip,
        SelfieVerificationFail,
        JumpToSuspended,
        JumpToCallSupport,
        JumpToFrozen,
        JumpToAllowed,
        JumpToForbidden,
        UpdatePassword,
        TryUnfreeze
    }
}
