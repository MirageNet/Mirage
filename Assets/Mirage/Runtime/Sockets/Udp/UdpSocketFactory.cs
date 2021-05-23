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
            return new IPEndPoint(IPAddress.Any, port);
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
            foreach (IPAddress result in results)
            {
                if (result.AddressFamily == AddressFamily.InterNetwork)
                {
                    return result;
                }
            }

            throw new FormatException("Could not parse address");
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
        readonly Socket socket;

        public UdpSocket()
        {
            // todo do we need to use AddressFamily from endpoint?
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Blocking = false;
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        }

        public void Bind(EndPoint endPoint)
        {
            socket.Bind(endPoint);
            // todo check socket options
            /*
            // options from https://github.com/phodoval/Mirage/blob/SimplifyTransports/Assets/Mirage/Runtime/Transport/Udp/UdpSocket.cs#L27
            socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.ReuseAddress, true);

            const uint IOC_IN = 0x80000000;
            const uint IOC_VENDOR = 0x18000000;
            socket.IOControl(unchecked((int)(IOC_IN | IOC_VENDOR | 12)), new[] { Convert.ToByte(false) }, null);
             */
        }

        public void Connect(EndPoint endPoint)
        {
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

        public int Receive(byte[] buffer, ref EndPoint endPoint)
        {
            // todo do we need to set if null
            endPoint = endPoint ?? new IPEndPoint(IPAddress.Any, 0);
            return socket.ReceiveFrom(buffer, ref endPoint);
        }

        public void Send(EndPoint endPoint, byte[] packet, int length)
        {
            // todo check disconnected
            // todo what SocketFlags??
            socket.SendTo(packet, length, SocketFlags.None, (IPEndPoint)endPoint);
        }
    }
}
