using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Mirror.Udp
{
    internal class UdpConnection : IConnection
    {
        private UdpClient client;

        public UdpConnection(UdpClient client)
        {
            this.client = client;
        }
        public void Disconnect()
        {
            client.Close();
        }

        public EndPoint GetEndPointAddress()
        {
            return client.Client.RemoteEndPoint;
        }

        public Task<bool> ReceiveAsync(MemoryStream buffer)
        {
            throw new NotImplementedException();
        }

        public Task SendAsync(ArraySegment<byte> data)
        {
            client.Send(data.Array, data.Array.Length);
            return Task.CompletedTask;
        }
    }
}
