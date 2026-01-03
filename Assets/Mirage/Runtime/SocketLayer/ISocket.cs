using System;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Delegate for handling incoming data from a connection.
    /// <para>Should only be invoked from within <see cref="ISocket.Tick"/>.</para>
    /// </summary>
    public delegate void OnData(IConnectionHandle handle, ReadOnlySpan<byte> data);

    /// <summary>
    /// Delegate for handling a disconnection from a connection.
    /// <para>Should only be invoked from within <see cref="ISocket.Tick"/>.</para>
    /// </summary>
    public delegate void OnDisconnect(IConnectionHandle handle, ReadOnlySpan<byte> data, string reason);


    /// <summary>
    /// Link between Mirage and the outside world
    /// </summary>
    public interface ISocket
    {
        /// <summary>
        /// Starts listens for data on an endPoint
        /// <para>Used by Server to allow clients to connect</para>
        /// </summary>
        /// <param name="endPoint">the endPoint to listen on</param>
        void Bind(IBindEndPoint endPoint);

        /// <summary>
        /// Sets up Socket ready to send data to endPoint as a client
        /// </summary>
        /// <param name="endPoint">the endPoint to connect to</param>
        /// <returns>returns the handle for the connection</returns>
        IConnectionHandle Connect(IConnectEndPoint endPoint);

        /// <summary>
        /// Closes the socket, stops receiving messages from other peers
        /// </summary>
        void Close();

        /// <summary>
        /// Set events that will be used by <see cref="Tick"/>. Will be called once when <see cref="Peer"/> is set up with <see cref="ISocket"/>.
        /// <para>The <paramref name="onData"/> and <paramref name="onDisconnect"/> events should only be invoked from within the <see cref="Tick"/> method.</para>
        /// </summary>
        void SetTickEvents(int maxPacketSize, OnData onData, OnDisconnect onDisconnect);

        /// <summary>
        /// Should invoke <see cref="OnData"/> and <see cref="OnDisconnect"/> events for any new data or disconnections.
        /// <para>This method is called by <see cref="Peer.UpdateReceive"/> once per frame.</para>
        /// </summary>
        void Tick();

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
        /// <param name="buffer">buffer to write received packet into</param>
        /// <param name="endPoint">where packet came from</param>
        /// <returns>length of packet, should not be above <paramref name="buffer"/> length</returns>
        int Receive(Span<byte> outBuffer, out IConnectionHandle handle);

        /// <summary>
        /// Sends a packet to an endPoint
        /// <para>Implementation should use <paramref name="length"/> because <paramref name="packet"/> is a buffer than may contain data from previous packets</para>
        /// </summary>
        /// <param name="endPoint">where packet is being sent to</param>
        /// <param name="packet">buffer that contains the packet, starting at index 0</param>
        /// <param name="length">length of the packet</param>
        void Send(IConnectionHandle handle, ReadOnlySpan<byte> packet);
    }

    public interface IBindEndPoint { }
    public interface IConnectEndPoint { }

    /// <summary>
    /// Object that can be used as an endPoint or handle for <see cref="Peer"/> and <see cref="ISocket"/>
    /// <para>
    /// Implementation of this should override <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/> so that 2 instance wil be equal if they have the same address internally
    /// </para>
    /// <para>
    /// When a new connection is received by Peer a copy of this endPoint will be created and given to that connection.
    /// On future received the incoming endPoint will be compared to active connections inside a dictionary
    /// </para>
    /// </summary>
    public interface IConnectionHandle
    {
        bool IsStateful { get; }

        /// <summary>
        /// Used by stateful connections, stores a direct reference to avoid lookup
        /// </summary>
        ISocketLayerConnection SocketLayerConnection { get; set; }

        /// <summary>
        /// Can gracefulDisconnectReason be used with <see cref="Disconnect(string)"/> or should <see cref="Peer"/> handle Linger and disconnect itself
        /// </summary>
        /// <param name="reason"></param>
        bool SupportsGracefulDisconnect { get; }

        /// <summary>
        /// disconnect for stateful connections. Should be made safe to call multiple times
        /// </summary>
        /// <param name="reason"></param>
        void Disconnect(string gracefulDisconnectReason);


        /// <summary>
        /// Used by stateless connection, copy will be stored and used later to sending messages
        /// <para>Creates a new instance of <see cref="IConnectionHandle"/> with same connection data.</para>
        /// <para>this is called when a new connection is created by <see cref="Peer"/></para>
        /// </summary>
        /// <returns></returns>
        IConnectionHandle CreateCopy();
        int GetHashCode();
        bool Equals(object obj);
    }

    /// <summary>
    /// Used by <see cref="Peer"/> to get get the <see cref="Connection"/> object directly to avoid lookup
    /// </summary>
    public interface ISocketLayerConnection { }
}
