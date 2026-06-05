using System;

namespace Mirage.Serialization
{
    public class SerializationLimitException : Exception
    {
        public SerializationLimitException()
        {
        }

        public SerializationLimitException(string message) : base(message)
        {
        }

        public SerializationLimitException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
