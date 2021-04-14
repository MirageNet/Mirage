namespace Mirage.SocketLayer
{
    internal enum PacketType
    {
        /// <summary>
        /// see <see cref="Commands"/>
        /// </summary>
        Command = 1,

        Unreliable = 2,
        Notify = 3,

        /// <summary>
        /// Used to keep connection alive.
        /// <para>Similar to ping/pong</para>
        /// </summary>
        KeepAlive = 10,
    }
}
