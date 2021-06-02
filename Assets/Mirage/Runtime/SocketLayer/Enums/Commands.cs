namespace Mirage.SocketLayer
{
    /// <summary>
    /// Small message used to control a connection
    /// <para>
    ///     <see cref="PacketType"/> and Commands uses their own byte/enum to split up the flow and add struture to the code.
    /// </para>
    /// </summary>
    public enum Commands
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
}
