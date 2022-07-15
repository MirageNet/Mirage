#if UNITY_STANDALONE || UNITY_EDITOR
using System;
using Mirage.SocketLayer;
using NanoSockets;

namespace Mirage.Sockets.Udp
{
    // todo Create an Exception in mirage that can be re-used by multiple sockets (makes it easier for user to catch)
    public class NanoSocketException : Exception
    {
        public NanoSocketException(string message) : base(message) { }
    }
    public sealed class NanoSocket : ISocket, IDisposable
    {
        private Socket socket;
        private NanoEndPoint receiveEndPoint;
        private readonly int bufferSize;
        private bool needsDisposing;

        public NanoSocket(UdpSocketFactory factory)
        {
            bufferSize = factory.BufferSize;
        }
        ~NanoSocket()
        {
            Dispose();
        }

        private void InitSocket()
        {
            socket = UDP.Create(bufferSize, bufferSize);
            UDP.SetDontFragment(socket);
            UDP.SetNonBlocking(socket);
            needsDisposing = true;
        }

        public void Bind(IEndPoint endPoint)
        {
            receiveEndPoint = (NanoEndPoint)endPoint;

            InitSocket();
            var result = UDP.Bind(socket, ref receiveEndPoint.address);
            if (result != 0)
            {
                throw new NanoSocketException("Socket Bind failed: address or port might already be in use");
            }
        }

        public void Dispose()
        {
            if (!needsDisposing) return;
            UDP.Destroy(ref socket);
            needsDisposing = false;
        }

        public void Close()
        {
            Dispose();
        }

        public void Connect(IEndPoint endPoint)
        {
            receiveEndPoint = (NanoEndPoint)endPoint;

            InitSocket();
            var result = UDP.Connect(socket, ref receiveEndPoint.address);
            if (result != 0)
            {
                throw new NanoSocketException("Socket Connect failed");
            }
        }

        public bool Poll()
        {
            return UDP.Poll(socket, 0) > 0;
        }

        public int Receive(byte[] buffer, out IEndPoint endPoint)
        {
            var count = UDP.Receive(socket, ref receiveEndPoint.address, buffer, buffer.Length);
            endPoint = receiveEndPoint;

            return count;
        }

        public void Send(IEndPoint endPoint, byte[] packet, int length)
        {
            var nanoEndPoint = (NanoEndPoint)endPoint;
            UDP.Send(socket, ref nanoEndPoint.address, packet, length);
        }
    }
}
#endif
