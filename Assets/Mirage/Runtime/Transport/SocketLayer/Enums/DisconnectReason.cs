namespace Mirage.SocketLayer
{

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
