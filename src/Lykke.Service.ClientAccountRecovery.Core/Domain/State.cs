namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    public enum State
    {
        RecoveryStarted,
        AwaitSecretPhrases,
        AwaitDeviceVerification,
        AwaitSmsVerification,
        AwaitEmailVerification,
        AwaitSelfieVerification,
        SelfieVerificationInProgress,
        AwaitPinCode,
        PasswordChangeFrozen,
        PasswordChangeSuspended,
        CallSupport,
        Transfer,
        PasswordChangeAllowed,
        PasswordChangeForbidden,
        PasswordUpdated
    }
}
