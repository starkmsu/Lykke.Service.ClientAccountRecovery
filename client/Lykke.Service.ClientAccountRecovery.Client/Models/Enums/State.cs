using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Enums
{
    /// <summary>
    ///     A current state of the state machine.
    /// </summary>
    [PublicAPI]
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
