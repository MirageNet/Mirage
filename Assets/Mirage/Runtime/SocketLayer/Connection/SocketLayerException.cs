using System;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Base Exception by all errors from using SocketLayer
    /// </summary>
    public class SocketLayerException : Exception
    {
        public SocketLayerException(string message) : base(message)
        {
        }
    }

    public class BufferFullException : SocketLayerException
    {
        public BufferFullException(string message) : base(message)
        {
        }
    }

    public class MessageSizeException : SocketLayerException
    {
        public MessageSizeException(string message) : base(message)
        {
        }
    }

    public class NoConnectionException : SocketLayerException
    {
        public NoConnectionException(string message) : base(message)
        {
        }
    }

    public class NotifyTokenException : SocketLayerException
    {
        public NotifyTokenException(string message) : base(message)
        {
        }
    }
}
