// windows or linux, unless EXCLUDE_NANOSOCKETS  is defined
#if EXCLUDE_NANOSOCKETS || !(UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || NETCOREAPP || NET_5_0_OR_GREATER)
// these classes are copies of the real classes but throw NotSupported
// this is needed so that NanoSocket scripts can be excluded from builds because this breaks on some targets
using System;
using Mirage.SocketLayer;

namespace Mirage.Sockets.Udp
{
    public static class InitUDP
    {
        public static void Init() => throw new NotSupportedException();
        public static void Deinit() => throw new NotSupportedException();
    }
    public sealed class NanoSocket : ISocket
    {
        public static bool Supported => false;

        public NanoSocket(int bufferSize) => throw new NotSupportedException();
        public void Bind(IBindEndPoint endPoint) => throw new NotSupportedException();
        public IConnectionHandle Connect(IConnectEndPoint endPoint) => throw new NotSupportedException();
        public void Close() => throw new NotSupportedException();
        public bool Poll() => throw new NotSupportedException();
        public int Receive(Span<byte> outBuffer, out IConnectionHandle handle) => throw new NotSupportedException();
        public void Send(IConnectionHandle handle, ReadOnlySpan<byte> span) => throw new NotSupportedException();
        void ISocket.Tick() => throw new NotSupportedException();
        void ISocket.Flush() => throw new NotSupportedException();
        void ISocket.SetTickEvents(int maxPacketSize, OnData onData, OnDisconnect onDisconnect) => throw new NotSupportedException();

    }
    public sealed class NanoConnectionHandle : IConnectionHandle, IBindEndPoint, IConnectEndPoint, IEquatable<NanoConnectionHandle>
    {
        public NanoConnectionHandle(string host, ushort port) => throw new NotSupportedException();

        bool IConnectionHandle.IsStateful => throw new NotSupportedException();
        bool IConnectionHandle.SupportsGracefulDisconnect => throw new NotSupportedException();
        void IConnectionHandle.Disconnect(string gracefulDisconnectReason) => throw new NotSupportedException();
        ISocketLayerConnection IConnectionHandle.SocketLayerConnection
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
        public IConnectionHandle CreateCopy() => throw new NotSupportedException();
        public bool Equals(NanoConnectionHandle other) => throw new NotSupportedException();
        public override bool Equals(object obj) => throw new NotSupportedException();
        public override int GetHashCode() => throw new NotSupportedException();
        public override string ToString() => throw new NotSupportedException();
    }
}
#endif
