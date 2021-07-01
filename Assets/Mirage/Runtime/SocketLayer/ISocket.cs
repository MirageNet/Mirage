namespace Mirage.SocketLayer
{
    /// <summary>
    /// Link between Mirage and the outside world
    /// </summary>
    public interface ISocket
    {
        /// <summary>
        /// Starts listens for data on an endpoint
        /// <para>Used by Server to allow clients to connect</para>
        /// </summary>
        /// <param name="endPoint">the endpoint to listen on</param>
        void Bind(IEndPoint endPoint);

        /// <summary>
        /// Sets up Socket ready to send data to endpoint as a client
        /// </summary>
        /// <param name="endPoint"></param>
        void Connect(IEndPoint endPoint);

        /// <summary>
        /// Closes the socket, stops receiving messages from other peers
        /// </summary>
        void Close();

        /// <summary>
        /// Checks if a packet is available 
        /// </summary>
        /// <returns>true if there is atleast 1 packet to read</returns>
        bool Poll();

        /// <summary>
        /// Gets next packet
        /// <para>Should be called after Poll</para>
        /// <para>
        ///     Implementation should check that incoming packet is within the size of <paramref name="buffer"/>,
        ///     and make sure not to return <paramref name="bytesReceived"/> above that size
        /// </para>
        /// </summary>
        /// <param name="buffer">buffer to write recevived packet into</param>
        /// <param name="endPoint">where packet came from</param>
        /// <returns>length of packet, should not be above <paramref name="buffer"/> length</returns>
        int Receive(byte[] buffer, out IEndPoint endPoint);

        /// <summary>
        /// Sends a packet to an endpoint
        /// <para>Implementation should use <paramref name="length"/> because <paramref name="packet"/> is a buffer than may contain data from previous packets</para>
        /// </summary>
        /// <param name="endPoint">where packet is being sent to</param>
        /// <param name="packet">buffer that contains the packet, starting at index 0</param>
        /// <param name="length">length of the packet</param>
        void Send(IEndPoint endPoint, byte[] packet, int length);
    }

    /// <summary>
    /// Object that can be used as an endpoint for <see cref="Peer"/> and <see cref="ISocket"/>
    /// <para>
    /// Implementation of this should override <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/> so that 2 instance wil be equal if they have the same address internally
    /// </para>
    /// <para>
    /// When a new connection is received by Peer a copy of this endpoint will be created and given to that connection.
    /// On future received the incoming endpoint will be compared to active connections inside a dictionary
    /// </para>
    /// </summary>
    public interface IEndPoint
    {
        /// <summary>
        /// Creates a new instance of <see cref="IEndPoint"/> with same connection data
        /// <para>this is called when a new connection is created by <see cref="Peer"/></para>
        /// </summary>
        /// <returns></returns>
        IEndPoint CreateCopy();
    }
}
