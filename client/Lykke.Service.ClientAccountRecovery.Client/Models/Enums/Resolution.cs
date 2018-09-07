using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Enums
{
    /// <summary>
    ///     A resolution from the support.
    /// </summary>
    [PublicAPI]
    public enum Resolution
    {
        Unknown,
        Suspend,
        Interview,
        Freeze,
        Allow
    }
}
