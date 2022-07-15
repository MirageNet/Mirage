using System;
using System.Net;
using System.Net.Sockets;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Sockets.Udp
{
    public class UdpSocket : ISocket
    {
        private Socket socket;
        private EndPointWrapper Endpoint;

        public void Bind(IEndPoint endPoint)
        {
            Endpoint = (EndPointWrapper)endPoint;

            socket = CreateSocket(Endpoint.inner);
            socket.DualMode = true;
            socket.Bind(Endpoint.inner);
        }

        private static Socket CreateSocket(EndPoint endPoint)
        {
            var ipEndPoint = (IPEndPoint)endPoint;
            var newSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
            {
                Blocking = false,
            };

            newSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            TrySetIOControl(newSocket);

            return newSocket;
        }

        private static void TrySetIOControl(Socket socket)
        {
            try
            {
                if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor)
                {
                    // IOControl only seems to work on windows
                    // gives "SocketException: The descriptor is not a socket" when running on github action on Linux
                    // see https://github.com/mono/mono/blob/f74eed4b09790a0929889ad7fc2cf96c9b6e3757/mcs/class/System/System.Net.Sockets/Socket.cs#L2763-L2765
                    return;
                }

                // stops "SocketException: Connection reset by peer"
                // this error seems to be caused by a failed send, resulting in the next polling being true, even those endpoint is closed
                // see https://stackoverflow.com/a/15232187/8479976

                // this IOControl sets the reporting of "unrealable" to false, stoping SocketException after a connection closes without sending disconnect message
                const uint IOC_IN = 0x80000000;
                const uint IOC_VENDOR = 0x18000000;
                const uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                var _false = new byte[] { 0, 0, 0, 0 };

                socket.IOControl(unchecked((int)SIO_UDP_CONNRESET), _false, null);
            }
            catch (Exception e)
            {
                Debug.LogError("Exception setting IOControl");
                Debug.LogException(e);
            }
        }

        public void Connect(IEndPoint endPoint)
        {
            Endpoint = (EndPointWrapper)endPoint;

            socket = CreateSocket(Endpoint.inner);
        }

        public void Close()
        {
            socket.Close();
            socket.Dispose();
        }

        /// <summary>
        /// Is message avaliable
        /// </summary>
        /// <returns>true if data to read</returns>
        public bool Poll()
        {
            return socket.Poll(0, SelectMode.SelectRead);
        }

        public int Receive(byte[] buffer, out IEndPoint endPoint)
        {
            var c = socket.ReceiveFrom(buffer, ref Endpoint.inner);
            endPoint = Endpoint;
            return c;
        }

        public void Send(IEndPoint endPoint, byte[] packet, int length)
        {
            var netEndPoint = ((EndPointWrapper)endPoint).inner;
            socket.SendTo(packet, length, SocketFlags.None, netEndPoint);
        }
    }
}
