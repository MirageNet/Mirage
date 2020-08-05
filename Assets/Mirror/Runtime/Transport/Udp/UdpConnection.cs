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

        public async Task<bool> ReceiveAsync(MemoryStream buffer)
        {
            try
            {
                while (true)
                {
                    var receivedResult = await client.ReceiveAsync();
                    buffer.Write(receivedResult.Buffer, 0, receivedResult.Buffer.Length);
                    return true;
                }
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
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
