using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    public class BrutForceDetector : IBrutForceDetector
    {
        private readonly IStateRepository _stateRepository;
        private readonly RecoveryConditions _recoveryConditions;
        private static readonly State[] UnsuccessfulStates;
        private static readonly State[] InProgressStates;

        static BrutForceDetector()
        {
            UnsuccessfulStates = new[] { State.PasswordChangeSuspended, State.PasswordChangeForbidden };
            InProgressStates = Enum.GetValues(typeof(State)).Cast<State>().Except
            (
                new[]
                {
                    State.PasswordChangeAllowed,
                    State.PasswordUpdated,
                    State.PasswordChangeSuspended,
                    State.PasswordChangeForbidden
                }
            ).ToArray();
        }

        public BrutForceDetector(IStateRepository stateRepository, RecoveryConditions recoveryConditions)
        {
            _stateRepository = stateRepository;
            _recoveryConditions = recoveryConditions;
        }

        public async Task<bool> IsNewRecoveryAllowedAsync(string clientId)
        {
            var history = await _stateRepository.FindRecoverySummary(clientId);
            if (history == null)
            {
                return true;
            }

            var noInProgress = history.Log.Count(l => InProgressStates.Contains(l.ActualStatus.State));
            if (noInProgress > 0)
            {
                return false;
            }
            var noOfLastBadStatuses = history.Log.OrderByDescending(l => l.ActualStatus.Time)
                .TakeWhile(l => UnsuccessfulStates.Contains(l.ActualStatus.State)).Count();
            if (noOfLastBadStatuses >= _recoveryConditions.MaxUnsuccessfulRecoveryAttempts)
            {
                return false;
            }

            return true;
        }
    }
}
