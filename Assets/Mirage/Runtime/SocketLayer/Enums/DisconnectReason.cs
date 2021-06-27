namespace Mirage.SocketLayer
{
    /// <summary>
    /// Reason why a connection was disconnected
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
        /// Disconnect called by higher level
        /// </summary>
        RequestedByRemotePeer = 2,

        /// <summary>
        /// Disconnect called by higher level
        /// </summary>
        RequestedByLocalPeer = 3,

        /// <summary>
        /// Received packet was not allowed by config
        /// </summary>
        InvalidPacket = 4,
    }
}
