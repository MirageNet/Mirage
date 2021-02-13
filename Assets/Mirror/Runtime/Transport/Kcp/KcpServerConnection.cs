using System;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;

namespace Mirror.KCP
{
    public class KcpServerConnection : KcpConnection
    {
        public KcpServerConnection(Socket socket, EndPoint remoteEndpoint, KcpDelayMode delayMode, int sendWindowSize, int receiveWindowSize) : base(socket, remoteEndpoint, delayMode, sendWindowSize, receiveWindowSize)
        {
        }

        internal UniTask HandshakeAsync()
        {
            // send a greeting and see if the server replies
            Send(Hello);

            return WaitForHello();
        }  
    }
}
