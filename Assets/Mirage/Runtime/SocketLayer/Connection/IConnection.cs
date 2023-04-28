using System;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// A connection that can send data directly to sockets
    /// <para>Only things inside socket layer should be sending raw packets. Others should use the methods inside <see cref="Connection"/></para>
    /// </summary>
    internal interface IRawConnection
    {
        /// <summary>
        /// Sends directly to socket without adding header
        /// <para>packet given to this function as assumed to already have a header</para>
        /// </summary>
        /// <param name="packet">header and messages</param>
        void SendRaw(byte[] packet, int length);
    }

    /// <summary>
    /// Connection for <see cref="Peer"/>
    /// </summary>
    public interface IConnection
    {
        IEndPoint EndPoint { get; }
        ConnectionState State { get; }

        void Disconnect();

        INotifyToken SendNotify(byte[] packet);
        INotifyToken SendNotify(byte[] packet, int offset, int length);
        INotifyToken SendNotify(ArraySegment<byte> packet);

        void SendNotify(byte[] packet, INotifyCallBack callBacks);
        void SendNotify(byte[] packet, int offset, int length, INotifyCallBack callBacks);
        void SendNotify(ArraySegment<byte> packet, INotifyCallBack callBacks);

        /// <summary>
        /// single message, batched by AckSystem
        /// </summary>
        /// <param name="message"></param>
        void SendReliable(byte[] message);
        /// <summary>
        /// single message, batched by AckSystem
        /// </summary>
        /// <param name="message"></param>
        void SendReliable(byte[] message, int offset, int length);
        /// <summary>
        /// single message, batched by AckSystem
        /// </summary>
        /// <param name="message"></param>
        void SendReliable(ArraySegment<byte> message);

        void SendUnreliable(byte[] packet);
        void SendUnreliable(byte[] packet, int offset, int length);
        void SendUnreliable(ArraySegment<byte> packet);

        /// <summary>
        /// Forces the connection to send any batched message immediately to the socket
        /// <para>
        /// Note: this will only send the packet to the socket. Some sockets may not send on main thread so might not send immediately
        /// </para>
        /// </summary>
        void FlushBatch();
    }
}
