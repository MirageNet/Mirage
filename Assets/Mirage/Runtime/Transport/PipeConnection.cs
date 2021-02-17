using System;
using System.IO;
using System.Net;
using System.Threading;
using Cysharp.Threading.Tasks;

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

        // buffer where we can queue up data
        readonly NetworkWriter writer = new NetworkWriter();

        public event MessageReceivedDelegate MessageReceived;
        public event Action Disconnected;

        public static (PipeConnection, PipeConnection) CreatePipe()
        {
            var c1 = new PipeConnection();
            var c2 = new PipeConnection();

            c1.connected = c2;
            c2.connected = c1;

            return (c1, c2);
        }

        public void Disconnect()
        {
            // disconnect both ends of the pipe
            connected.Disconnected?.Invoke();
            Disconnected?.Invoke();
        }

        // technically not an IPEndpoint,  will fix later
        public EndPoint GetEndPointAddress() => new IPEndPoint(IPAddress.Loopback, 0);

        // dispatch all the messages in this connection
        public void Poll()
        {
            var data = writer.ToArraySegment();

            if (data.Count == 0)
                return;

            using (PooledNetworkReader reader = NetworkReaderPool.GetReader(data))
            {
                while (reader.Position < reader.Length)
                {
                    int channel = reader.ReadPackedInt32();
                    ArraySegment<byte> packet = reader.ReadBytesAndSizeSegment();

                    MessageReceived(packet, channel);
                }
            }

            writer.SetLength(0);
        }

        public void Send(ArraySegment<byte> data, int channel = Channel.Reliable)
        {
            // add some data to the writer in the connected connection
            // and increase the message count
            connected.writer.WritePackedInt32(channel);
            connected.writer.WriteBytesAndSizeSegment(data);
        }
    }
}
