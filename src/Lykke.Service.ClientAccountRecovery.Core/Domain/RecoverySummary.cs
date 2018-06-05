using System.Collections.Generic;
using System.Diagnostics;

namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    public class RecoverySummary
    {
        private readonly List<RecoveryContext> _log = new List<RecoveryContext>();
        public string ClientId { get; private set; }
        public IReadOnlyList<RecoveryContext> Log => _log;

        public void AddItem(RecoveryContext context)
        {
            if (ClientId == null)
            {
                ClientId = context.ClientId;
            }
            Debug.Assert(ClientId == context.ClientId);

            _log.Add(context);
        }
    }
}
