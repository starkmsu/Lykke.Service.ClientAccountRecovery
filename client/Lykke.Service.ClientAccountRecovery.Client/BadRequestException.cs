using Microsoft.Rest;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    /// <summary>
    /// An exception for 409 code
    /// </summary>
    public class BadRequestException : RestException
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}