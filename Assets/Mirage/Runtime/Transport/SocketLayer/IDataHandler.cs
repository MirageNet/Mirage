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
        /// New Message from low level
        /// <para>will contain byte from just 1 high level message</para>
        /// </summary>
        /// <param name="connection">connection that sent data</param>
        /// <param name="segment"></param>
        void ReceiveData(IConnection connection, ArraySegment<byte> segment);
    }
}
