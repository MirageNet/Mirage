using Mirage.SocketLayer;
using NanoSockets;

namespace Mirage.Sockets.Udp {
    public class NanoSocket : ISocket
    {
        Socket socket;
        NanoEndPoint receiveEndPoint;
        readonly int bufferSize;

        public NanoSocket(UdpSocketFactory factory) {
            bufferSize = factory.BufferSize;
        }

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
            socket = UDP.Create(bufferSize, bufferSize);
            UDP.SetDontFragment(socket);
            UDP.SetNonBlocking(socket);
        }
    }
}