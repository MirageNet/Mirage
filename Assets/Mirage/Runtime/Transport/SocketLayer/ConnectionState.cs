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

        Disconnected = 9,
        Destroyed = 10,
    }
}
