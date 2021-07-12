using System;
using Mirage.SocketLayer;
using NanoSockets;

namespace Mirage.Sockets.Udp {
    public class NanoEndPoint : IEndPoint, IEquatable<NanoEndPoint>
    {
        public Address address;

        public NanoEndPoint(string host, ushort port)
        {
            address = new Address();
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
            if (obj is NanoEndPoint endPoint) {
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

    public class NanoSocket : ISocket
    {
        Socket socket;
        NanoEndPoint tmpEndPoint;

        public void Bind(IEndPoint endPoint)
        {
            tmpEndPoint = (NanoEndPoint)endPoint;

            InitSocket();
            UDP.Bind(socket, ref tmpEndPoint.address);
        }

        public void Close()
        {
            UDP.Destroy(ref socket);
        }

        public void Connect(IEndPoint endPoint)
        {
            tmpEndPoint = (NanoEndPoint)endPoint;
            
            InitSocket();
            UDP.Connect(socket, ref tmpEndPoint.address);
        }

        public bool Poll()
        {
            return UDP.Poll(socket, 0) > 0;
        }

        public int Receive(byte[] buffer, out IEndPoint endPoint)
        {
            int count = UDP.Receive(socket, ref tmpEndPoint.address, buffer, buffer.Length);
            endPoint = tmpEndPoint;

            return count;
        }

        public void Send(IEndPoint endPoint, byte[] packet, int length)
        {
            tmpEndPoint = (NanoEndPoint)endPoint;
            UDP.Send(socket, ref tmpEndPoint.address, packet, length);
        }

        void InitSocket()
        {
            socket = UDP.Create(256 * 1024, 256 * 1024);
            UDP.SetDontFragment(socket);
            UDP.SetNonBlocking(socket);
        }
    }
}