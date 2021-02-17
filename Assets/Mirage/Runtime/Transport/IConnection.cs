using System;
using System.Net;

namespace Mirage
{

    public static class Channel
    {
        // 2 well known channels
        // transports can implement other channels
        // to expose their features
        public const int Reliable = 0;
        public const int Unreliable = 1;
    }

    public delegate void MessageReceivedDelegate(ArraySegment<byte> data, int channel);

    public interface IConnection
    {
        void Send(ArraySegment<byte> data, int channel = Channel.Reliable);

        /// <summary>
        /// raised when we get a message
        /// </summary>
        /// <remarks>
        /// The event gets raised with the data that got received and the channel id
        /// do not keep the data array, it will be reused for other messages
        /// </remarks>
        event MessageReceivedDelegate MessageReceived;

        /// <summary>
        /// Raised when the connection disconnects
        /// </summary>
        event Action Disconnected;

        /// <summary>
        /// Disconnect this connection
        /// </summary>
        void Disconnect();

        /// <summary>
        /// the address of endpoint we are connected to
        /// Note this can be IPEndPoint or a custom implementation
        /// of EndPoint, which depends on the transport
        /// </summary>
        /// <returns></returns>
        EndPoint GetEndPointAddress();
    }
}
