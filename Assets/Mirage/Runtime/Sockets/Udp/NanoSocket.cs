// windows, linux or standalone c#, unless EXCLUDE_NANOSOCKETS is defined
#if !EXCLUDE_NANOSOCKETS && (UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || NETCOREAPP || NET_5_0_OR_GREATER)
using System;
using Mirage.SocketLayer;
using NanoSockets;

namespace Mirage.Sockets.Udp
{
    public sealed class NanoSocket : ISocket, IDisposable
    {
        public static bool Supported => true;

        private Socket socket;
        private NanoConnectionHandle receiveEndPoint;
        private readonly int bufferSize;
        private bool needsDisposing;

        public NanoSocket(int bufferSize)
        {
            this.bufferSize = bufferSize;
        }
        ~NanoSocket()
        {
            Dispose();
        }

        private void CreateSocket()
        {
            socket = UDP.Create(bufferSize, bufferSize);
            UDP.SetDontFragment(socket);
            UDP.SetNonBlocking(socket);
            needsDisposing = true;
        }

        public void Bind(IBindEndPoint endPoint)
        {
            receiveEndPoint = (NanoConnectionHandle)endPoint;

            CreateSocket();
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

        public IConnectionHandle Connect(IConnectEndPoint endPoint)
        {
            receiveEndPoint = (NanoConnectionHandle)endPoint;

            CreateSocket();
            var result = UDP.Connect(socket, ref receiveEndPoint.address);
            if (result != 0)
            {
                throw new NanoSocketException("Socket Connect failed");
            }

            return receiveEndPoint;
        }

        public bool Poll()
        {
            return UDP.Poll(socket, 0) > 0;
        }

        public unsafe int Receive(Span<byte> outBuffer, out IConnectionHandle handle)
        {
            fixed (byte* ptr = outBuffer)
            {
                var count = UDP.Receive(socket, ref receiveEndPoint.address, new IntPtr(ptr), outBuffer.Length);
                handle = receiveEndPoint;
                return count;
            }
        }

        public unsafe void Send(IConnectionHandle handle, ReadOnlySpan<byte> span)
        {
            var nanoEndPoint = (NanoConnectionHandle)handle;
            fixed (byte* ptr = span)
            {
                UDP.Send(socket, ref nanoEndPoint.address, new IntPtr(ptr), span.Length);
            }
        }

        void ISocket.Tick() { }
        void ISocket.SetTickEvents(int maxPacketSize, OnData onData, OnDisconnect onDisconnect) { }
    }
}
#endif
