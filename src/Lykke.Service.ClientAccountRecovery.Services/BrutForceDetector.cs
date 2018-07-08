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
        private readonly IRecoveryFlowServiceFactory _factory;
        private readonly RecoveryConditions _recoveryConditions;
        private static readonly State[] UnsuccessfulStates;
        private static readonly State[] InProgressStates;

        static BrutForceDetector()
        {
            UnsuccessfulStates = new[] { State.PasswordChangeForbidden };
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

        public BrutForceDetector(IStateRepository stateRepository, IRecoveryFlowServiceFactory factory, RecoveryConditions recoveryConditions)
        {
            _stateRepository = stateRepository;
            _factory = factory;
            _recoveryConditions = recoveryConditions;
        }

        public async Task<bool> IsNewRecoveryAllowedAsync(string clientId)
        {
            var history = await _stateRepository.FindRecoverySummary(clientId);
            if (history == null)
            {
                return true;
            }

            var noOfLastBadStatuses = history.Log.OrderByDescending(l => l.ActualStatus.Time)
                .TakeWhile(l => UnsuccessfulStates.Contains(l.ActualStatus.State)).Count();
            if (noOfLastBadStatuses >= _recoveryConditions.MaxUnsuccessfulRecoveryAttempts)
            {
                return false;
            }

            return true;
        }

        public async Task BlockPreviousRecoveries(string clientId, string ip, string userAgent)
        {
            var history = await _stateRepository.FindRecoverySummary(clientId);

            if (history == null)
            {
                return;
            }

            foreach (var recoveryUnit in history.Log.Where(l => InProgressStates.Contains(l.ActualStatus.State)))
            {
                var flow = await _factory.FindExisted(recoveryUnit.RecoveryId);
                flow.Context.Initiator = "RecoveryService";
                flow.Context.Ip = ip;
                flow.Context.UserAgent = userAgent;

                await flow.JumpToForbiddenAsync();
            }
        }
    }
}
