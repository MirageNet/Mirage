namespace Mirage.SocketLayer
{
    public struct Config
    {
        // server
        /// <summary>
        /// Max concurrent connections server will accept
        /// </summary>
        public int MaxConnections;


        // client
        /// <summary>
        /// How often connect attempt message will be re-sent if server does not reply
        /// </summary>
        public float ConnectAttemptInterval;

        /// <summary>
        /// How many times attempt to connect before giving up
        /// </summary>
        public int MaxConnectAttempts;

        /// <summary>
        /// how long after previous send before sending keep alive message
        /// <para>Keep alive is to stop connection from timing out</para>
        /// <para>keep alive is sent over unreliable so this interval should be low enough so that <see cref="TimeoutDuration"/> does not timeout if some unreliable packets are missed </para>
        /// </summary>
        public float KeepAliveInterval;

        /// <summary>
        /// how long without a message before disconnecting connection
        /// </summary>
        public float TimeoutDuration;


        // shared
        /// <summary>
        /// How long after disconnect before connection is fully removed from Peer
        /// </summary>
        public float DisconnectDuration;
    }
}
