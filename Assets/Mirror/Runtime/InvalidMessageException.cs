using System;

namespace Mirror
{

    public class InvalidMessageException : Exception
    {
        public InvalidMessageException()
        {
        }

        public InvalidMessageException(string message) : base(message)
        {
        }

        public InvalidMessageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
