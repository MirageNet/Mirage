namespace Mirage.SocketLayer
{
    public enum PacketType
    {
        /// <summary>
        /// see <see cref="Commands"/>
        /// </summary>
        Command = 1,

        /// <summary>
        /// data packet sent with no guarantee for order or reliability
        /// <para>used for data that is fire and forget</para>
        /// </summary>
        Unreliable = 2,

        /// <summary>
        /// data packet sent with ack header so sender knows if packet gets delivered or lost
        /// </summary>
        Notify = 3,

        /// <summary>
        /// data packet that are guarantee to be in order, and not lost.
        /// <para>contains ack header</para>
        /// <para>If a package is lost then other Reliable packets will be held until the lost packet is resent</para>
        /// </summary>
        Reliable = 4,

        /// <summary>
        /// part of a Reliable message. same as Reliable but only part of a message
        /// </summary>
        ReliableFragment = 6,

        /// <summary>
        /// packet with just ack header
        /// <para>only sent if no other packets with ack header were sent recently</para>
        /// </summary>
        Ack = 5,

        /// <summary>
        /// Used to keep connection alive.
        /// <para>Similar to ping/pong</para>
        /// </summary>
        KeepAlive = 10,
    }
}
