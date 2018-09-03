using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Enums
{
    /// <summary>
    ///     A current state of the challenge.
    /// </summary>
    [PublicAPI]
    public enum Progress
    {
        Ongoing,
        WaitingForSupport,
        Frozen,
        Suspended,
        Allowed,
        Undefined
    }
}
