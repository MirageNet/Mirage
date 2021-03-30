namespace Mirage.SocketLayer
{
    internal enum PacketType
    {
        /// <summary>
        /// see <see cref="Commands"/>
        /// </summary>
        Command = 1,

        Unreliable = 2,
        Reliable = 3,
        Notify = 4,

        /// <summary>
        /// Used to keep connection alive.
        /// <para>Similar to ping/pong</para>
        /// </summary>
        KeepAlive = 10,
    }

    /// <summary>
    /// Small message used to control a connection
    /// <para>
    ///     <see cref="PacketType"/> and Commands uses their own byte/enum to split up the flow and add struture to the code.
    /// </para>
    /// </summary>
    internal enum Commands
    {
        /// <summary>
        /// Sent from client to request to connect to server
        /// </summary>
        ConnectRequest = 1,

        /// <summary>
        /// Sent when Server accepts client
        /// </summary>
        ConnectionAccepted = 2,

        /// <summary>
        /// Sent when server rejects client
        /// </summary>
        ConnectionRejected = 3,

        /// <summary>
        /// Sent from client or server to close connection
        /// </summary>
        Disconnect = 4,
    }

    /// <summary>
    /// Reason for reject sent from server
    /// </summary>
    public enum RejectReason
    {
        None = 0,
        ServerFull = 1,
        Timeout = 2,
    }

    /// <summary>
    /// Reason why a connection was disconnected
    /// </summary>
    public enum DisconnectReason
    {
        None,
        /// <summary>
        /// No message Received in timeout window
        /// </summary>
        Timeout = 1,
        /// <summary>
        /// Disconnect called by higher level
        /// </summary>
        RequestedByPeer = 2,
    }
}
