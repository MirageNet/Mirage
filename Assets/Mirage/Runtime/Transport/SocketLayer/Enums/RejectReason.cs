namespace Mirage.SocketLayer
{
    /// <summary>
    /// Reason for reject sent from server
    /// </summary>
    public enum RejectReason
    {
        None = 0,
        ServerFull = 1,
        Timeout = 2,
        /// <summary>
        /// Closed called locally before connect
        /// </summary>
        ClosedByPeer = 3,
    }
}
