namespace Mirage.SocketLayer
{
    /// <summary>
    /// Reason for reject sent from server
    /// </summary>
    public enum RejectReason
    {
        /// <summary>
        /// No reason given
        /// </summary>
        None = 0,

        /// <summary>
        /// Server is at max connections and will not accept a new connection until one disconnects
        /// </summary>
        ServerFull = 1,

        /// <summary>
        /// Server did not reply to connection request 
        /// </summary>
        Timeout = 2,

        /// <summary>
        /// Closed called locally before connect
        /// </summary>
        ClosedByPeer = 3,
    }
}
