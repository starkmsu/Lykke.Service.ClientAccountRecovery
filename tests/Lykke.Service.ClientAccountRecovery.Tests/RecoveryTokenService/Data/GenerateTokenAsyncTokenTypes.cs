using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using State = Lykke.Service.ClientAccountRecovery.Core.Domain.State;

namespace Lykke.Service.ClientAccountRecovery.Tests.RecoveryTokenService.Data
{
    internal class GenerateTokenAsyncTokenTypes :  IEnumerable
    {
        private const string DefaultToken = "Default";
        private const string InfiniteToken = "Infinite";

        public IEnumerator GetEnumerator()
        {
            var data = new List<object[]>
            {
                new object[] {State.RecoveryStarted, DefaultToken},
                new object[] {State.AwaitSecretPhrases, DefaultToken},
                new object[] {State.AwaitDeviceVerification, DefaultToken},
                new object[] {State.AwaitSmsVerification, DefaultToken},
                new object[] {State.AwaitEmailVerification, DefaultToken},
                new object[] {State.AwaitSelfieVerification, InfiniteToken},
                new object[] {State.SelfieVerificationInProgress, InfiniteToken},
                new object[] {State.AwaitPinCode, DefaultToken},
                new object[] {State.PasswordChangeFrozen, InfiniteToken},
                new object[] {State.PasswordChangeSuspended, DefaultToken},
                new object[] {State.CallSupport, DefaultToken},
                new object[] {State.Transfer, DefaultToken},
                new object[] {State.PasswordChangeAllowed, DefaultToken},
                new object[] {State.PasswordChangeForbidden, DefaultToken},
                new object[] {State.PasswordUpdated, DefaultToken},
            };

            return data.GetEnumerator();
        }
    }
}
