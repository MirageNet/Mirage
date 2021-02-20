using System;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;

namespace Mirage.KCP
{
    public class KcpServerConnection : KcpConnection
    {
        public KcpServerConnection(Socket socket, EndPoint remoteEndpoint, KcpDelayMode delayMode, int sendWindowSize, int receiveWindowSize) : base(socket, remoteEndpoint, delayMode, sendWindowSize, receiveWindowSize)
        {
        }

        internal void Handshake()
        {
            // send a greeting and see if the server replies
            Send(Hello);
        }  
    }
}
