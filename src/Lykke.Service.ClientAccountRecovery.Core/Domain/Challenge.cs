namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    /// <summary>
    /// Available client challenges
    /// </summary>
    public enum Challenge
    {
        Unknown = 0,
        Sms = 1,
        Email = 2,
        Selfie = 3,
        Words = 4,
        Device = 5,
        Pin = 6,
        None = 7
    }
}
