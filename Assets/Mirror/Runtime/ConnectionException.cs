using System;
using System.Runtime.Serialization;

namespace Mirror
{
    [Serializable]
    public class ConnectionException : Exception
    {
        public readonly NetworkConnectionToClient Connection;

        public ConnectionException()
        {
        }

        public ConnectionException(string message) : base(message)
        {
        }

        public ConnectionException(string message, NetworkConnectionToClient connection) : base(message)
        {
            this.Connection = connection;
        }

        public ConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}