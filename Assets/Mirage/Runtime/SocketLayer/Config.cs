namespace Mirage.SocketLayer
{
    // todo add validation for this config
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
        public float KeepAliveInterval = 2;

        /// <summary>
        /// how long without a message before disconnecting connection
        /// </summary>
        public float TimeoutDuration = 10;
        #endregion


        #region shared
        /// <summary>
        /// Key sent with connection message (defaults to Major version of assmebly)
        /// <para>Used to validate that server and client are same application/version</para>
        /// <para>NOTE: key will be ASCII encoded</para>
        /// </summary>
        public string key = null;

        /// <summary>
        /// How long after disconnect before connection is fully removed from Peer
        /// </summary>
        public float DisconnectDuration = 1;

        /// <summary>
        /// How many buffers to create at start
        /// </summary>
        public int BufferPoolStartSize = 100;

        /// <summary>
        /// max number of buffers allowed to be stored in pool
        /// <para>buffers over this limit will be left for GC</para>
        /// </summary>
        public int BufferPoolMaxSize = 5000;

        /// <summary>
        /// how long after last send to send ack without a message
        /// </summary>
        public float TimeBeforeEmptyAck = 0.11f;

        /// <summary>
        /// How many receives before sending an empty ack
        /// <para>this is so that acks are still sent even if receives many message before replying</para>
        /// </summary>
        public int ReceivesBeforeEmptyAck = 8;

        /// <summary>
        /// How many empty acks to send via <see cref="TimeBeforeEmptyAck"/>
        /// <para>Send enough acks that there is a high chances that 1 of them reaches other size</para>
        /// <para>Empty Ack count resets after receives new message</para>
        /// </summary>
        public int EmptyAckLimit = 8;

        /// <summary>
        /// How many packets can exist it ring buffers for Ack and Reliable system
        /// <para>This value wont count null packets so can be set lower than <see cref="SequenceSize"/>'s value to limit actual number of packets waiting to be acked</para>
        /// <para>Example: (max=2000) * (MTU=1200) * (connections=100) => 240MB</para>
        /// </summary>
        public int MaxReliablePacketsInSendBufferPerConnection = 2000;

        /// <summary>
        /// Bit size of sequence used for AckSystem
        /// <para>this value also determines the size of ring buffers for Ack and Reliable system</para>
        /// <para>Max of 16</para>
        /// </summary>
        public int SequenceSize = 12;

        /// <summary>
        /// How many fragments large reliable message can be split into
        /// <para>if set to 0 then messages over <see cref="SocketFactory.MaxPacketSize"/> will not be allowed to be sent</para>
        /// <para>max value is 255</para>
        /// </summary>
        public int MaxReliableFragments = 5;
        #endregion
    }
}
