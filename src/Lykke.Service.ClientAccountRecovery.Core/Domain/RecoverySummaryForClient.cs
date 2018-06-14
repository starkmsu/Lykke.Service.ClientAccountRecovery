using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    public class RecoverySummaryForClient
    {
        private readonly Dictionary<string, RecoveryUnit> _recoveryUnits = new Dictionary<string, RecoveryUnit>();
        public string ClientId { get; private set; }
        public IReadOnlyList<RecoveryUnit> Log
        {
            get
            {
                if (_recoveryUnits.Count == 0)
                {
                    throw new InvalidOperationException("Client must contain at least one recovery record");
                }
                return _recoveryUnits.Values.ToArray();
            }
        }

        public void AddItem(RecoveryUnit unit)
        {
            if (ClientId == null)
            {
                ClientId = unit.ClientId;
            }
            Debug.Assert(ClientId == unit.ClientId);

            _recoveryUnits[unit.RecoveryId] = unit;
        }

    }

    public class RecoveryUnit
    {
        public readonly IReadOnlyCollection<RecoveryContext> Log;
        public bool Empty => Log.Count == 0;
        public string RecoveryId => Log.First().RecoveryId;
        public string ClientId => Log.First().ClientId;
        public RecoveryContext ActualStatus => Log.OrderByDescending(l => l.SeqNo).First();
        public RecoveryUnit(IReadOnlyCollection<RecoveryContext> log)
        {
            Log = log;
        }
    }
}
