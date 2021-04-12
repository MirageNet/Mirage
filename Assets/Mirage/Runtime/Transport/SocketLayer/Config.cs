namespace Mirage.SocketLayer
{
    public class Config
    {
        #region server 
        /// <summary>
        /// Max concurrent connections server will accept
        /// </summary>
        public int MaxConnections = 100;
        #endregion

        #region client
        /// <summary>
        /// How often connect attempt message will be re-sent if server does not reply
        /// </summary>
        public float ConnectAttemptInterval = 0.25f;

        /// <summary>
        /// How many times attempt to connect before giving up
        /// </summary>
        public int MaxConnectAttempts = 10;

        /// <summary>
        /// how long after previous send before sending keep alive message
        /// <para>Keep alive is to stop connection from timing out</para>
        /// <para>keep alive is sent over unreliable so this interval should be low enough so that <see cref="TimeoutDuration"/> does not timeout if some unreliable packets are missed </para>
        /// </summary>
        public float KeepAliveInterval = 1;

        /// <summary>
        /// how long without a message before disconnecting connection
        /// </summary>
        public float TimeoutDuration = 5;
        #endregion


        #region shared
        /// <summary>
        /// How long after disconnect before connection is fully removed from Peer
        /// </summary>
        public float DisconnectDuration = 2;

        /// <summary>
        /// Max size of a packet (excluding peer header)
        /// </summary>
        public int Mtu = 1280 - 20 - 8;
        /// <summary>
        /// How many buffers to create at start
        /// </summary>
        public int BufferPoolStartSize = 10;
        /// <summary>
        /// max number of buffers allowed to be stored in pool
        /// <para>buffers over this limit will be left for GC</para>
        /// </summary>
        public int BufferPoolMaxSize = 100;
        #endregion
    }
}
