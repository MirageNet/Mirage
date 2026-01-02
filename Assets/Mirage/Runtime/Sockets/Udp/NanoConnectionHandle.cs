// windows, linux or standalone c#, unless EXCLUDE_NANOSOCKETS is defined
#if !EXCLUDE_NANOSOCKETS && (UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || NETCOREAPP || NET_5_0_OR_GREATER)
using System;
using Mirage.SocketLayer;
using NanoSockets;

namespace Mirage.Sockets.Udp
{
    public sealed class NanoConnectionHandle : IConnectionHandle, IBindEndPoint, IConnectEndPoint, IEquatable<NanoConnectionHandle>
    {
        public Address address = new Address();

        public NanoConnectionHandle(string host, ushort port)
        {
            address.port = port;
            UDP.SetHostName(ref address, host);
        }

        public NanoConnectionHandle(Address address)
        {
            this.address = address;
        }

        bool IConnectionHandle.IsStateful => false;
        bool IConnectionHandle.SupportsGracefulDisconnect => false;
        void IConnectionHandle.Disconnect(string gracefulDisconnectReason) { /* not supported */ }
        ISocketLayerConnection IConnectionHandle.SocketLayerConnection { get; set; }

        public IConnectionHandle CreateCopy()
        {
            return new NanoConnectionHandle(address);
        }

        public bool Equals(NanoConnectionHandle other)
        {
            return address.Equals(other.address);
        }

        public override bool Equals(object obj)
        {
            if (obj is NanoConnectionHandle endPoint)
            {
                return address.Equals(endPoint.address);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return address.GetHashCode();
        }

        public override string ToString()
        {
            return address.ToString();
        }
    }
}
#endif
