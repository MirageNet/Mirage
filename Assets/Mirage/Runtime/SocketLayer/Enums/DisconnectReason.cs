namespace Mirage.SocketLayer
{
    /// <summary>
    /// Reason why a connection was disconnected or rejected
    /// </summary>
    public enum DisconnectReason
    {
        /// <summary>
        /// No reason given
        /// </summary>
        None = 0,

        /// <summary>
        /// No message Received in timeout window
        /// </summary>
        Timeout = 1,

        /// <summary>
        /// Disconnect called by higher level (remote side)
        /// </summary>
        RequestedByRemotePeer = 2,

        /// <summary>
        /// Disconnect called by higher level (local side)
        /// </summary>
        RequestedByLocalPeer = 3,

        /// <summary>
        /// Received packet was not allowed by config
        /// </summary>
        InvalidPacket = 4,

        /// <summary>
        /// Send buffer was full and could not accept more data
        /// </summary>
        SendBufferFull = 5,

        /// <summary>
        /// Server is at max connections and will not accept a new connection
        /// </summary>
        ServerFull = 10,

        /// <summary>
        /// Server did not reply to connection request
        /// </summary>
        ConnectingTimeout = 11,

        /// <summary>
        /// Disconnect called locally before server replies with connected
        /// </summary>
        ConnectingCancel = 12,

        /// <summary>
        /// Key given with first message did not match the value on the server
        /// </summary>
        KeyInvalid = 13,

        /// <summary>
        /// Incoming connection packet was invalid
        /// </summary>
        InvalidUnconnectedPacket = 14,
    }
}
