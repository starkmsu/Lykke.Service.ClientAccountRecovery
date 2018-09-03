using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Services;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    /// <inheritdoc />
    [UsedImplicitly]
    public class RecoveryFileSettings : IRecoveryFileSettings
    {
        public int SelfieImageMaxSizeMBytes { get; }

        public RecoveryFileSettings(int? selfieImageMaxSizeMBytes, ILogFactory logFactory)
        {
            var log = logFactory.CreateLog(this);

            if (selfieImageMaxSizeMBytes == null)
            {
                SelfieImageMaxSizeMBytes = Consts.SelfieImageMaxSizeMBytes;

                log.Warning(
                    $"Max size for recovery selfie image is not specified in settings! Using default max image size: {Consts.SelfieImageMaxSizeMBytes}Mb.");
            }
            else
            {
                SelfieImageMaxSizeMBytes = (int) selfieImageMaxSizeMBytes;
            }
        }
    }
}
