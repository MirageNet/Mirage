using System;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;

namespace Mirror.KCP
{
    public class KcpServerConnection : KcpConnection
    {
        internal event Action<int> DataSent;

        public KcpServerConnection(Socket socket, EndPoint remoteEndpoint, KcpDelayMode delayMode, int sendWindowSize, int receiveWindowSize) : base(delayMode, sendWindowSize, receiveWindowSize)
        {
            this.socket = socket;
            this.remoteEndpoint = remoteEndpoint;
            SetupKcp();
        }

        internal UniTask HandshakeAsync()
        {
            // send a greeting and see if the server replies
            Send(Hello);

            return WaitForHello();
        }

        protected override void RawSend(byte[] data, int length)
        {
            DataSent?.Invoke(length);
            socket.SendTo(data, 0, length, SocketFlags.None, remoteEndpoint);
        }
    }
}
