namespace Mirage.SocketLayer
{
    /// <summary>
    /// Reason why a connection was disconnected or rejected.
    /// <para>Ranges: 0-99 SocketLayer/Transport, 100-999 Mirage Core, 1000+ Game-defined</para>
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
        /// Disconnect requested by remote peer
        /// </summary>
        RequestedByRemotePeer = 2,

        /// <summary>
        /// Disconnect requested by local peer
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
        /// Server is at max connections
        /// </summary>
        ServerFull = 10,

        /// <summary>
        /// Server did not reply to connection request in time
        /// </summary>
        ConnectingTimeout = 11,

        /// <summary>
        /// Closed called locally before connect completed
        /// </summary>
        ConnectingCancel = 12,

        /// <summary>
        /// Key given with first message did not match the value on the server
        /// </summary>
        KeyInvalid = 13,

        /// <summary>
        /// Send if <see cref="Config.SendRejectIfUnconnectedPacketIsInvalid"/> is true
        /// </summary>
        InvalidUnconnectedPacket = 14,
    }
}
