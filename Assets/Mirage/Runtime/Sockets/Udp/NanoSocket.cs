using System;
using Mirage.SocketLayer;
using NanoSockets;

namespace Mirage.Sockets.Udp {
    public sealed class NanoEndPoint : IEndPoint, IEquatable<NanoEndPoint>
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
        NanoEndPoint receiveEndPoint;

        public void Bind(IEndPoint endPoint)
        {
            receiveEndPoint = (NanoEndPoint)endPoint;

            InitSocket();
            UDP.Bind(socket, ref receiveEndPoint.address);
        }

        public void Close()
        {
            UDP.Destroy(ref socket);
        }

        public void Connect(IEndPoint endPoint)
        {
            receiveEndPoint = (NanoEndPoint)endPoint;
            
            InitSocket();
            UDP.Connect(socket, ref receiveEndPoint.address);
        }

        public bool Poll()
        {
            return UDP.Poll(socket, 0) > 0;
        }

        public int Receive(byte[] buffer, out IEndPoint endPoint)
        {
            int count = UDP.Receive(socket, ref receiveEndPoint.address, buffer, buffer.Length);
            endPoint = receiveEndPoint;

            return count;
        }

        public void Send(IEndPoint endPoint, byte[] packet, int length)
        {
            var nanoEndPoint = (NanoEndPoint)endPoint;
            UDP.Send(socket, ref nanoEndPoint.address, packet, length);
        }

        void InitSocket()
        {
            socket = UDP.Create(256 * 1024, 256 * 1024);
            UDP.SetDontFragment(socket);
            UDP.SetNonBlocking(socket);
        }
    }
}