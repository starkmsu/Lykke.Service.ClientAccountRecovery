using System;
using System.Runtime.Serialization;

namespace Lykke.Service.ClientAccountRecovery.Core
{
    [Serializable]
    public class InvalidActionException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidActionException()
        {
        }

        public InvalidActionException(string message) : base(message)
        {
        }

        public InvalidActionException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidActionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
