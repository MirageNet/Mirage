using System;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Handles data from SocketLayer
    /// <para>A high level script should implement this interface give it to Peer when it is created</para>
    /// </summary>
    public interface IDataHandler
    {
        /// <summary>
        /// Receives a new Packet from low level
        /// </summary>
        /// <param name="connection">connection that sent data</param>
        /// <param name="message">Single message received by peer</param>
        void ReceiveMessage(IConnection connection, ArraySegment<byte> message);
    }
}
