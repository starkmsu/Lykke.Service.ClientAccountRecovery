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

        public Task ExecuteAction(Challenge challenge, Action action, string code, IRecoveryFlowService flow)
        {
            var switcher = (challenge, action);
            switch (switcher)
            {
                case var a when a.Item1 == Challenge.Words && a.Item2 == Action.Complete:
                    return flow.SecretPhrasesCompleteAsync();
                case var a when a.Item1 == Challenge.Words && a.Item2 == Action.Skip:
                    return flow.SecretPhrasesSkipAsync();
                case var a when a.Item1 == Challenge.Device && a.Item2 == Action.Complete:
                    return flow.DeviceVerifiedCompleteAsync();
                case var a when a.Item1 == Challenge.Device && a.Item2 == Action.Skip:
                    return flow.DeviceVerificationSkip();
                case var a when a.Item1 == Challenge.Sms && a.Item2 == Action.Complete:
                    return ValidateSms(flow, code);
                case var a when a.Item1 == Challenge.Sms && a.Item2 == Action.Skip:
                    return flow.SmsVerificationSkip();
                case var a when a.Item1 == Challenge.Sms && a.Item2 == Action.Restart:
                    return flow.SmsVerificationRestart();
                case var a when a.Item1 == Challenge.Email && a.Item2 == Action.Complete:
                    return ValidateEmail(flow, code);
                case var a when a.Item1 == Challenge.Email && a.Item2 == Action.Skip:
                    return flow.EmailVerificationSkip();
                case var a when a.Item1 == Challenge.Email && a.Item2 == Action.Restart:
                    return flow.EmailVerificationRestart();
                case var a when a.Item1 == Challenge.Selfie && a.Item2 == Action.Complete:
                    return NotifySelfiePosted(flow, code);
                case var a when a.Item1 == Challenge.Selfie && a.Item2 == Action.Skip:
                    return flow.SelfieVerificationSkip();
                case var a when a.Item1 == Challenge.Pin && a.Item2 == Action.Complete:
                    return flow.PinCodeVerificationComplete();
                case var a when a.Item1 == Challenge.Pin && a.Item2 == Action.Skip:
                    return flow.PinCodeVerificationSkip();
            }

            throw new ArgumentException($"Invalid pair {challenge} {action}");
        }


        private Task ValidateEmail(IRecoveryFlowService flow, string code)
        {
            return _validator.ConfirmEmailCode(flow, code);
        }

        private Task ValidateSms(IRecoveryFlowService flow, string code)
        {
            return _validator.ConfirmSmsCode(flow, code);
        }

        private Task NotifySelfiePosted(IRecoveryFlowService flow, string code)
        {
            return _selfieNotificationSender.Send(flow, code);
        }
    }
}
