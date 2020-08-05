using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Mirror.Udp
{
    internal class UdpConnection : IConnection
    {
        public UdpClient client;

        public UdpConnection(UdpClient client)
        {
            this.client = client;
        }

        public void Disconnect()
        {
            client.Client.Close();
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
            client.Client.SendTo(data.Array, client.Client.RemoteEndPoint);
            return Task.CompletedTask;
        }

        public void Stop()
        {
            client.Client.Close();
        }
    }
}
