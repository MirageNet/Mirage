using System;
using System.Net;
using System.Net.Sockets;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Sockets.Udp
{
    public sealed class UdpSocketFactory : SocketFactory
    {
        [SerializeField] string address = "localhost";
        [SerializeField] int port = 7777;

        public override ISocket CreateClientSocket()
        {
            ThrowIfNotSupported();

            return new UdpSocket();
        }

        public override ISocket CreateServerSocket()
        {
            ThrowIfNotSupported();

            return new UdpSocket();
        }

        public override EndPoint GetBindEndPoint()
        {
            return new IPEndPoint(IPAddress.IPv6Any, port);
        }

        public override EndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            string addressString = address ?? this.address;
            IPAddress ipAddress = getAddress(addressString);

            ushort portIn = port ?? (ushort)this.port;

            return new IPEndPoint(ipAddress, portIn);
        }

        private IPAddress getAddress(string addressString)
        {
            if (IPAddress.TryParse(addressString, out IPAddress address))
                return address;

            IPAddress[] results = Dns.GetHostAddresses(addressString);
            if (results.Length == 0)
            {
                throw new SocketException((int)SocketError.HostNotFound);
            }
            else
            {
                return results[0];
            }
        }

        void ThrowIfNotSupported()
        {
            if (IsWebgl)
            {
                throw new NotSupportedException("Udp Socket can not be created in Webgl builds, Use WebSocket instead");
            }
        }

        private static bool IsWebgl => Application.platform == RuntimePlatform.WebGLPlayer;
    }

    public class UdpSocket : ISocket
    {
        Socket socket;
        IPEndPoint AnyEndpoint;

        public UdpSocket()
        {
        }

        public void Bind(EndPoint endPoint)
        {
            AnyEndpoint = endPoint as IPEndPoint;

            socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
            {
                DualMode = true,
                Blocking = false,
            };

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            socket.Bind(endPoint);
            // todo check socket options
            /*
            // options from https://github.com/phodoval/Mirage/blob/SimplifyTransports/Assets/Mirage/Runtime/Transport/Udp/UdpSocket.cs#L27
            socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.ReuseAddress, true);

            const uint IOC_IN = 0x80000000;
            const uint IOC_VENDOR = 0x18000000;
            socket.IOControl(unchecked((int)(IOC_IN | IOC_VENDOR | 12)), new[] { Convert.ToByte(false) }, null);
             */


            // maybe fix for "Connection reset by peer" https://stackoverflow.com/a/10337850/8479976
            // socket options https://flylib.com/books/en/1.398.1.49/1/
        }

        public void Connect(EndPoint endPoint)
        {
            AnyEndpoint = endPoint as IPEndPoint;

            socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
            {
                Blocking = false,
            };
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // todo check if connect should be called for udp or if it should be something else
            socket.Connect(endPoint);
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

        public int Receive(byte[] buffer, out EndPoint endPoint)
        {
            endPoint = AnyEndpoint;
            int c = socket.ReceiveFrom(buffer, ref endPoint);
            return c;
        }

        public void Send(EndPoint endPoint, byte[] packet, int length)
        {
            // todo check disconnected
            // todo what SocketFlags??
            socket.SendTo(packet, length, SocketFlags.None, endPoint);
        }
    }
}
