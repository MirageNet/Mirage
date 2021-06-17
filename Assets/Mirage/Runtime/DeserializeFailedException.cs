using System;
using System.Runtime.Serialization;

namespace Mirage
{

    [Serializable]
    public class DeserializeFailedException : Exception
    {
        public DeserializeFailedException()
        {
        }

        public DeserializeFailedException(string message) : base(message)
        {
        }

        public DeserializeFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
        protected DeserializeFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
