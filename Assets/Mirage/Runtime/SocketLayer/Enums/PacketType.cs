namespace Mirage.SocketLayer
{
    public enum PacketType
    {
        /// <summary>
        /// see <see cref="Commands"/>
        /// </summary>
        Command = 1,

        /// <summary>
        /// data packet sent with no guarantee for order or reliablity
        /// <para>used for data that is fire and forget</para>
        /// </summary>
        Unreliable = 2,

        /// <summary>
        /// data packet sent with ack header so sender knows if packet gets delivered or lost
        /// </summary>
        Notify = 3,

        /// <summary>
        /// packet with just acks
        /// </summary>
        Reliable = 4,

        /// <summary>
        /// packet with just acks
        /// </summary>
        ReliableFragment = 6,

        /// <summary>
        /// packet with just acks
        /// </summary>
        Ack = 5,

        /// <summary>
        /// Used to keep connection alive.
        /// <para>Similar to ping/pong</para>
        /// </summary>
        KeepAlive = 10,
    }
}
