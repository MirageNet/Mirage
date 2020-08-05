using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Mirror.Udp
{
    internal class UdpConnection : IConnection
    {
        public Socket socket;

        public UdpConnection(Socket socket)
        {
            this.socket = socket;
        }

        public void Disconnect()
        {
            socket.Close();
        }

        public EndPoint GetEndPointAddress()
        {
            return socket.RemoteEndPoint;
        }

        public async Task<bool> ReceiveAsync(MemoryStream buffer)
        {
            try
            {
                socket.Receive(buffer.ToArray());
                await Task.CompletedTask;

                if (buffer.Length > 0)
                {
                    return true;
                }
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }

        public Task SendAsync(ArraySegment<byte> data)
        {
            socket.SendTo(data.Array, socket.RemoteEndPoint);
            return Task.CompletedTask;
        }

        public void Stop()
        {
            socket.Close();
        }
    }
}
