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

        public NanoSocket(UdpSocketFactory factory) => throw new NotSupportedException();
        public void Bind(IEndPoint endPoint) => throw new NotSupportedException();
        public void Connect(IEndPoint endPoint) => throw new NotSupportedException();
        public void Close() => throw new NotSupportedException();
        public bool Poll() => throw new NotSupportedException();
        public int Receive(byte[] buffer, out IEndPoint endPoint) => throw new NotSupportedException();
        public void Send(IEndPoint endPoint, byte[] packet, int length) => throw new NotSupportedException();
    }
    public sealed class NanoEndPoint : IEndPoint
    {
        public NanoEndPoint(string host, ushort port) => throw new NotSupportedException();
        public IEndPoint CreateCopy() => throw new NotSupportedException();
    }
}
#endif
