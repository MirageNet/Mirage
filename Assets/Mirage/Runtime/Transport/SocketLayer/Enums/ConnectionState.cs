namespace Mirage.SocketLayer
{
    public enum ConnectionState
    {
        /// <summary>
        /// Initial state
        /// </summary>
        Created = 1,
        /// <summary>
        /// Client is connecting to server
        /// </summary>
        Connecting = 2,
        /// <summary>
        /// Server as accepted connection
        /// </summary>
        Connected = 3,

        /// <summary>
        /// Server or client has disconnected the connection and is waiting to be cleaned up
        /// </summary>
        Disconnected = 9,
        /// <summary>
        /// Marked to be removed from the connection collection
        /// </summary>
        Removing = 10,
        /// <summary>
        /// Removed from collection and all state cleaned up
        /// </summary>
        Destroyed = 11,
    }
}
