using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Client.Models.Enums;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Recovery
{
    [PublicAPI]
    public class RecoveryStatusResponse
    {
        /// <summary>
        ///     An challenge to perform
        /// </summary>
        public Challenge Challenge { get; set; }

        /// <summary>
        ///     A current state of the password recovery process
        /// </summary>
        public Progress OverallProgress { get; set; }

        /// <summary>
        ///     Addition data to perform the challenge. For example a message to be signed by the private key
        /// </summary>
        public string ChallengeInfo { get; set; }
    }
}
