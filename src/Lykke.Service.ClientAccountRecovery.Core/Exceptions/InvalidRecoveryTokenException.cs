using System;
using System.Runtime.Serialization;

namespace Lykke.Service.ClientAccountRecovery.Core.Exceptions
{
    public class InvalidRecoveryTokenException : Exception
    {
        public InvalidRecoveryTokenException()
        {
        }

        public InvalidRecoveryTokenException(string message) : base(message)
        {
        }

        public InvalidRecoveryTokenException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidRecoveryTokenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
