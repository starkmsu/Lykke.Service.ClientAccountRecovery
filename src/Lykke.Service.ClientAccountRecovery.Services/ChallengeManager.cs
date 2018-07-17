using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Action = Lykke.Service.ClientAccountRecovery.Core.Domain.Action;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class ChallengeManager : IChallengeManager
    {
        private readonly IChallengesValidator _validator;
        private readonly ISelfieNotificationSender _selfieNotificationSender;

        public ChallengeManager(IChallengesValidator validator, ISelfieNotificationSender selfieNotificationSender)
        {
            _validator = validator;
            _selfieNotificationSender = selfieNotificationSender;
        }

        public async Task<bool> ExecuteAction(Challenge challenge, Action action, string code, IRecoveryFlowService flow)
        {
            var switcher = (challenge, action);
            switch (switcher)
            {
                case var a when a.Item1 == Challenge.Words && a.Item2 == Action.Complete:
                    return await ValidateSecretPhrases(flow, code);
                case var a when a.Item1 == Challenge.Words && a.Item2 == Action.Skip:
                    await flow.SecretPhrasesSkipAsync();
                    return true;
                case var a when a.Item1 == Challenge.Device && a.Item2 == Action.Complete:
                    return await ValidateDevice(flow, code);
                case var a when a.Item1 == Challenge.Device && a.Item2 == Action.Skip:
                    await flow.DeviceVerificationSkipAsync();
                    return true;
                case var a when a.Item1 == Challenge.Sms && a.Item2 == Action.Complete:
                    return await ValidateSms(flow, code);
                case var a when a.Item1 == Challenge.Sms && a.Item2 == Action.Skip:
                    await flow.SmsVerificationSkipAsync();
                    return true;
                case var a when a.Item1 == Challenge.Sms && a.Item2 == Action.Restart:
                    await flow.SmsVerificationRestartAsync();
                    return true;
                case var a when a.Item1 == Challenge.Email && a.Item2 == Action.Complete:
                    return await ValidateEmail(flow, code);

                case var a when a.Item1 == Challenge.Email && a.Item2 == Action.Skip:
                    await flow.EmailVerificationSkipAsync();
                    return true;
                case var a when a.Item1 == Challenge.Email && a.Item2 == Action.Restart:
                    await flow.EmailVerificationRestartAsync();
                    return true;
                case var a when a.Item1 == Challenge.Selfie && a.Item2 == Action.Complete:
                    await NotifySelfiePosted(flow, code);
                    return true;
                case var a when a.Item1 == Challenge.Selfie && a.Item2 == Action.Skip:
                    await flow.SelfieVerificationSkipAsync();
                    return true;
                case var a when a.Item1 == Challenge.Pin && a.Item2 == Action.Complete:
                    await flow.PinCodeVerificationCompleteAsync();
                    return true;
                case var a when a.Item1 == Challenge.Pin && a.Item2 == Action.Skip:
                    await flow.PinCodeVerificationSkipAsync();
                    return true;
            }

            throw new ArgumentException($"Invalid pair {challenge} {action}");
        }


        private Task<bool> ValidateEmail(IRecoveryFlowService flow, string code)
        {
            return _validator.ConfirmEmailCode(flow, code);
        }

        private Task<bool> ValidateSms(IRecoveryFlowService flow, string code)
        {
            return _validator.ConfirmSmsCode(flow, code);
        }

        private Task<bool> ValidateDevice(IRecoveryFlowService flow, string code)
        {
            return _validator.ConfirmDeviceCode(flow, code);
        }

        private Task<bool> ValidateSecretPhrases(IRecoveryFlowService flow, string code)
        {
            return _validator.ConfirmSecretPhrasesCode(flow, code);
        }

        private Task NotifySelfiePosted(IRecoveryFlowService flow, string code)
        {
            return _selfieNotificationSender.Send(flow, code);
        }
    }
}
