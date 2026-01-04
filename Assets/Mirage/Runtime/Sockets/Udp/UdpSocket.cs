using System;
using System.Net;
using System.Net.Sockets;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Sockets.Udp
{
    public class UdpSocket : ISocket
    {
        private Socket socket;
        private UdpConnectionHandle Endpoint;

        private byte[] _internalReceiveBuffer;
        private byte[] _internalSendBuffer;
        private OnData _onData;

        public void Bind(IBindEndPoint endPoint)
        {
            Endpoint = (UdpConnectionHandle)endPoint;

            socket = CreateSocket(Endpoint.inner);
            socket.DualMode = true;
            socket.Bind(Endpoint.inner);
        }

        private static Socket CreateSocket(EndPoint endPoint)
        {
            var ipEndPoint = (IPEndPoint)endPoint;
            var newSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
            {
                Blocking = false,
            };

            newSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            TrySetIOControl(newSocket);

            return newSocket;
        }

        private static void TrySetIOControl(Socket socket)
        {
            try
            {
                if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor)
                {
                    // IOControl only seems to work on windows
                    // gives "SocketException: The descriptor is not a socket" when running on github action on Linux
                    // see https://github.com/mono/mono/blob/f74eed4b09790a0929889ad7fc2cf96c9b6e3757/mcs/class/System/System.Net.Sockets/Socket.cs#L2763-L2765
                    return;
                }

                // stops "SocketException: Connection reset by peer"
                // this error seems to be caused by a failed send, resulting in the next polling being true, even those endpoint is closed
                // see https://stackoverflow.com/a/15232187/8479976

                // this IOControl sets the reporting of "unrealable" to false, stoping SocketException after a connection closes without sending disconnect message
                const uint IOC_IN = 0x80000000;
                const uint IOC_VENDOR = 0x18000000;
                const uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                var _false = new byte[] { 0, 0, 0, 0 };

                socket.IOControl(unchecked((int)SIO_UDP_CONNRESET), _false, null);
            }
            catch (Exception e)
            {
                Debug.LogError("Exception setting IOControl");
                Debug.LogException(e);
            }
        }

        public IConnectionHandle Connect(IConnectEndPoint endPoint)
        {
            Endpoint = (UdpConnectionHandle)endPoint;
            socket = CreateSocket(Endpoint.inner);
            return Endpoint;
        }

        public void Close()
        {
            socket.Close();
            socket.Dispose();
            socket = null;
        }

        public bool Poll() // called from Peer before each Receive
        {
            return false;
        }

        public int Receive(Span<byte> outBuffer, out IConnectionHandle handle)
        {
            throw new NotSupportedException("Use Tick() instead");
        }

        public void Send(IConnectionHandle handle, ReadOnlySpan<byte> span)
        {
            var netEndPoint = ((UdpConnectionHandle)handle).inner;

            // .netstandard2.1 socket does not support span, so we have to copy buffer to an array before sending
            span.CopyTo(_internalSendBuffer);
            socket.SendTo(_internalSendBuffer, 0, span.Length, SocketFlags.None, netEndPoint);
        }

        void ISocket.Tick()
        {
            while (socket != null && socket.Poll(0, SelectMode.SelectRead))
            {
                try
                {
                    var length = socket.ReceiveFrom(_internalReceiveBuffer, ref Endpoint.inner);
                    if (length > 0) // ignore zero byte receives
                    {
                        var span = _internalReceiveBuffer.AsSpan(0, length);
                        _onData.Invoke(Endpoint, span);
                    }
                }
                catch (SocketException e)
                {
                    // Usually occurs if a remote port is closed and SIO_UDP_CONNRESET didn't catch it
                    Debug.LogWarning($"UDP Receive Error: {e.Message}");
                }
            }
        }
        void ISocket.Flush() { }
        void ISocket.SetTickEvents(int maxPacketSize, OnData onData, OnDisconnect onDisconnect)
        {
            _internalReceiveBuffer = new byte[maxPacketSize];
            _internalSendBuffer = new byte[maxPacketSize];
            _onData = onData;
        }
    }
}
