using System.Net;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Link between Mirage and the outside world...
    /// </summary>
    public interface ISocket
    {
        /// <summary>
        /// Starts listens for data on an endpoint
        /// <para>Used by Server to accpet data from clients</para>
        /// </summary>
        /// <param name="endPoint"></param>
        void Bind(EndPoint endPoint);

        void Close();

        /// <summary>
        /// Is message avaliable
        /// </summary>
        /// <returns>true if data to read</returns>
        bool Poll();

        /// <summary>
        /// Gets next Message
        /// <para>Should be called after Poll</para>
        /// <para>Implementation should check that incoming packet is within the size of <paramref name="data"/></para>
        /// </summary>
        /// <param name="data">Received data</param>
        void Receive(byte[] data, ref EndPoint endPoint, out int bytesReceived);

        /// <summary>
        /// Sends data to endpoint
        /// <para>Implementation should use <paramref name="length"/> if given as <paramref name="data"/> is a buffer that may contain values from previous packets</para>
        /// </summary>
        /// <param name="data"></param>
        void Send(EndPoint endPoint, byte[] data, int? length);
    }
}
