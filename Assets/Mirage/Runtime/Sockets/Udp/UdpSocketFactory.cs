using System;
using System.Net;
using System.Net.Sockets;
using Mirage.SocketLayer;
using NanoSockets;
using UnityEngine;

namespace Mirage.Sockets.Udp
{
    public sealed class UdpSocketFactory : SocketFactory
    {
        [SerializeField] string address = "localhost";
        [SerializeField] ushort port = 7777;

        static int initCount;

        [RuntimeInitializeOnLoadMethod]
        static void ClearCounter() {
            initCount = 0;
        }

        void Awake() {
            if (!IsDesktop) return;

            if (initCount == 0)
            {
                UDP.Initialize();
            }

            initCount++;
        }

        void OnDestroy() {
            if (!IsDesktop) return;

            initCount--;

            if (initCount == 0)
            {
                UDP.Deinitialize();
            }
        }

        public override ISocket CreateClientSocket()
        {
            ThrowIfNotSupported();

            if (IsDesktop) return new NanoSocket();

            return new UdpSocket();
        }

        public override ISocket CreateServerSocket()
        {
            ThrowIfNotSupported();

            if (IsDesktop) return new NanoSocket();

            return new UdpSocket();
        }

        public override IEndPoint GetBindEndPoint()
        {
            if (IsDesktop) return new NanoEndPoint("::0", port);

            return new EndPointWrapper(new IPEndPoint(IPAddress.IPv6Any, port));
        }

        public override IEndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            string addressString = address ?? this.address;
            IPAddress ipAddress = getAddress(addressString);

            ushort portIn = port ?? this.port;

            if (IsDesktop) return new NanoEndPoint(addressString, portIn);

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
        private static bool IsDesktop =>
            Application.platform == RuntimePlatform.LinuxPlayer
            || Application.platform == RuntimePlatform.OSXPlayer
            || Application.platform == RuntimePlatform.WindowsPlayer
            || Application.isEditor;
    }

    public class EndPointWrapper : IEndPoint
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

        IEndPoint IEndPoint.CreateCopy()
        {
            // copy the inner endpoint
            EndPoint copy = inner.Create(inner.Serialize());
            return new EndPointWrapper(copy);
        }
    }
}
