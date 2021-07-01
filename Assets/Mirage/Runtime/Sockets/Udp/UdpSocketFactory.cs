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

        public override IEndPoint GetBindEndPoint()
        {
            return new EndPointWrapper(new IPEndPoint(IPAddress.IPv6Any, port));
        }

        public override IEndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            string addressString = address ?? this.address;
            IPAddress ipAddress = getAddress(addressString);

            ushort portIn = port ?? (ushort)this.port;

            return new EndPointWrapper(new IPEndPoint(ipAddress, portIn));
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

    public struct EndPointWrapper : IEndPoint
    {
        public EndPoint inner;

        public EndPointWrapper(EndPoint endPoint)
        {
            inner = endPoint;
        }

        public override bool Equals(object obj)
        {
            if (obj is EndPointWrapper other)
            {
                return inner.Equals(other.inner);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return inner.GetHashCode();
        }

        public override string ToString()
        {
            return inner.ToString();
        }
    }
    public class UdpSocket : ISocket
    {
        Socket socket;
        IEndPoint AnyEndpoint;

        static EndPoint ToEndPoint(IEndPoint endPoint) => ((EndPointWrapper)endPoint).inner;

        public void Bind(IEndPoint endPoint)
        {
            AnyEndpoint = endPoint;

            socket = CreateSocket(endPoint);
            socket.DualMode = true;

            socket.Bind(ToEndPoint(endPoint));
        }

        static Socket CreateSocket(IEndPoint endPoint)
        {
            var ipEndPoint = ToEndPoint(endPoint) as IPEndPoint;
            var socket = new Socket(ipEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
            {
                Blocking = false,
            };

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            TrySetIOControl(socket);

            return socket;
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
                byte[] _false = new byte[] { 0, 0, 0, 0 };

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
            AnyEndpoint = endPoint;

            socket = CreateSocket(endPoint);

            socket.Connect(ToEndPoint(endPoint));
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
            EndPoint netEndPoint = ToEndPoint(AnyEndpoint);

            int c = socket.ReceiveFrom(buffer, ref netEndPoint);

            endPoint = new EndPointWrapper(netEndPoint as IPEndPoint);
            return c;
        }

        public void Send(IEndPoint endPoint, byte[] packet, int length)
        {
            // todo check disconnected
            // todo what SocketFlags??

            EndPoint netEndPoint = ToEndPoint(endPoint);
            socket.SendTo(packet, length, SocketFlags.None, netEndPoint);
        }
    }
}
