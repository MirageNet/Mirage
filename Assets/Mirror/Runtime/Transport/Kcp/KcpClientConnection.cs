using System;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mirror.KCP
{
    public class KcpClientConnection : KcpConnection
    {

        /// <summary>
        /// Client connection,  does not share the UDP client with anyone
        /// so we can set up our own read loop
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public KcpClientConnection(Socket socket, EndPoint remoteEndpoint, KcpDelayMode delayMode, int sendWindowSize, int receiveWindowSize) : base(socket, remoteEndpoint, delayMode, sendWindowSize, receiveWindowSize) 
        {
        }

        internal async UniTask HandshakeAsync(int bits)
        {
            // in the very first message we must mine a hashcash token
            // and send that as a hello
            // the server won't accept connections otherwise
            string applicationName = Application.productName;

            HashCash token = await UniTask.RunOnThreadPool(() => HashCash.Mine(applicationName, bits));
            byte[] hello = new byte[1000];
            int length = HashCashEncoding.Encode(hello, 0, token);

            var data = new ArraySegment<byte>(hello, 0, length);
            // send a greeting and see if the server replies

            Send(data);

            await WaitForHello();

        }
    }
}
