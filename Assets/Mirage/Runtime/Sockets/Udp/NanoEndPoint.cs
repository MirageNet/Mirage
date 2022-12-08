using System;
using Mirage.SocketLayer;
using NanoSockets;

namespace Mirage.Sockets.Udp
{
    public sealed class NanoEndPoint : IEndPoint, IEquatable<NanoEndPoint>
    {
        public Address address = new Address();

        public NanoEndPoint(string host, ushort port)
        {
            address.port = port;
            UDP.SetHostName(ref address, host);
        }

        public NanoEndPoint(Address address)
        {
            this.address = address;
        }

        public IEndPoint CreateCopy()
        {
            return new NanoEndPoint(address);
        }

        public bool Equals(NanoEndPoint other)
        {
            return address.Equals(other.address);
        }

        public override bool Equals(object obj)
        {
            if (obj is NanoEndPoint endPoint)
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
