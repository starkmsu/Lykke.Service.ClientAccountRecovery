namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    public enum State
    {
        RecoveryStarted,
        AwaitSecretPhrases,
        AwaitDeviceVerification,
        AwaitSmsVerification,
        AwaitEmailVerification,
        AwaitKycVerification,
        KycInProgress,
        AwaitPinCode,
        PasswordChangeFrozen,
        PasswordChangeSuspended,
        CallSupport,
        Transfer,
        PasswordChangeAllowed,
        PasswordChangeForbidden
    }
}
