using System;
using System.Net;

namespace Mirage
{

    /// <summary>
    /// A connection that is directly connected to another connection
    /// If you send data in one of them,  you receive it on the other one
    /// </summary>
    public class PipeConnection : IConnection
    {

        private PipeConnection connected;

        // should only be created by CreatePipe
        private PipeConnection()
        {

        }

        public event MessageReceivedDelegate MessageReceived;
        public event Action Disconnected;

        public static (IConnection, IConnection) CreatePipe()
        {
            var c1 = new PipeConnection();
            var c2 = new PipeConnection();

            c1.connected = c2;
            c2.connected = c1;

            return (c1, c2);
        }

        public void Disconnect()
        {
            Disconnected?.Invoke();
            connected.Disconnected?.Invoke();
        }

        // technically not an IPEndpoint,  will fix later
        public EndPoint GetEndPointAddress() => new IPEndPoint(IPAddress.Loopback, 0);

        public void Send(ArraySegment<byte> data, int channel = Channel.Reliable)
        {
            connected.MessageReceived?.Invoke(data, channel);
        }
    }
}
