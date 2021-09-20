using System;
using System.Runtime.Serialization;

namespace Mirage
{
    [Serializable]
    public class DeserializeFailedException : Exception
    {
        public DeserializeFailedException(string message) : base(message)
        {
        }

        protected DeserializeFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
