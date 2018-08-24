namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    /// <summary>
    /// A current state of the challenge
    /// </summary>
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
