// nanosockets breaks on some platforms (like iOS)
// so only include it for standalone and editor
// but not for mac because of code signing issue
// #if !(UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
// #define NANO_SOCKET_ALLOWED
// #endif

using System;
using System.Net;
using System.Net.Sockets;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Sockets.Udp
{
    public enum SocketLib { Automatic, Native, Managed };

    public sealed class UdpSocketFactory : SocketFactory, IHasAddress, IHasPort
    {
        public string Address = "localhost";
        public ushort Port = 7777;

        [Tooltip("Which socket implementation do you wish to use?\nThe default (automatic) will attempt to use NanoSockets on supported platforms and fallback to C# Sockets if unsupported.")]
        public SocketLib SocketLib = SocketLib.Automatic;

        [Header("NanoSocket-specific Options")]
        public int BufferSize = 256 * 1024;


        public override int MaxPacketSize => UdpMTU.MaxPacketSize;

        // Determines if we can use NanoSockets for socket-level IO. This will be true if either:
        // - We *want* to use native library explicitly.
        // - We have it set to Automatic selection and NanoSockets is supported.
        private bool useNanoSocket => SocketLib == SocketLib.Native || (SocketLib == SocketLib.Automatic && NanoSocket.Supported);

        /// <summary>
        /// did this instance call InitUDP.Init? if so then we need to call Deinit too
        /// </summary>
        [NonSerialized] private bool _udpNeedRelease = false;

        string IHasAddress.Address
        {
            get => Address;
            set => Address = value;
        }

        int IHasPort.Port
        {
            get => Port;
            set => Port = checked((ushort)value);
        }

        private void Awake()
        {
            if (useNanoSocket)
                InitNanosocket();
        }

        private void InitNanosocket()
        {
            if (NanoSocket.Supported)
            {
                InitUDP.Init();
                _udpNeedRelease = true;
            }
            else
            {
                Debug.LogWarning("NanoSocket support not available on this platform; falling back to Managed Sockets.");
                SocketLib = SocketLib.Managed;
            }
        }

        private void OnDestroy()
        {
            if (_udpNeedRelease)
            {
                InitUDP.Deinit();
                _udpNeedRelease = false;
            }
        }

        public override ISocket CreateClientSocket()
        {
            ThrowIfNotSupported();

            if (useNanoSocket)
            {
                return new NanoSocket(this);
            }
            else
            {
                return new UdpSocket();
            }
        }

        public override ISocket CreateServerSocket()
        {
            ThrowIfNotSupported();

            if (useNanoSocket)
            {
                return new NanoSocket(this);
            }
            else
            {
                return new UdpSocket();
            }
        }

        public override IEndPoint GetBindEndPoint()
        {
            if (useNanoSocket)
            {
                return new NanoEndPoint("::0", Port);
            }
            else
            {
                return new EndPointWrapper(new IPEndPoint(IPAddress.IPv6Any, Port));
            }
        }

        public override IEndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            var addressString = address ?? Address;
            var ipAddress = getAddress(addressString);

            var portIn = port ?? Port;

            if (useNanoSocket)
            {
                return new NanoEndPoint(addressString, portIn);
            }
            else
            {
                return new EndPointWrapper(new IPEndPoint(ipAddress, portIn));
            }
        }

        private IPAddress getAddress(string addressString)
        {
            if (IPAddress.TryParse(addressString, out var address))
                return address;

            var results = Dns.GetHostAddresses(addressString);
            if (results.Length == 0)
            {
                throw new SocketException((int)SocketError.HostNotFound);
            }
            else
            {
                return results[0];
            }
        }

        /// <summary>
        /// Explicitly throws an exception if a platform is not supported.
        /// Currently only fires on WebGL.
        /// </summary>
        /// <exception cref="NotSupportedException">Tells you why it's not supported.</exception>
        private void ThrowIfNotSupported()
        {
            if (IsWebGL)
            {
                throw new NotSupportedException("The WebGL platform does not support UDP Sockets. Please use WebSockets instead.");
            }
        }

        /// <summary>
        /// Is this platform a WebGL-based one?
        /// </summary>
        private static bool IsWebGL => Application.platform == RuntimePlatform.WebGLPlayer;
    }

    public class EndPointWrapper : IEndPoint
    {
        public EndPoint inner;

        public EndPointWrapper(EndPoint endPoint)
        {
            inner = endPoint;
        }

        public override bool Equals(object obj)
        {
            if (obj is EndPointWrapper other)
            {
                return inner.Equals(other.inner);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return inner.GetHashCode();
        }

        public override string ToString()
        {
            return inner.ToString();
        }

        IEndPoint IEndPoint.CreateCopy()
        {
            // copy the inner endpoint
            var copy = inner.Create(inner.Serialize());
            return new EndPointWrapper(copy);
        }
    }

    public class UdpMTU
    {
        /// <summary>
        /// IPv6 + UDP Header
        /// </summary>
        private const int HEADER_SIZE = 40 + 8;

        /// <summary>
        /// MTU is expected to be atleast this number
        /// </summary>
        private const int MIN_MTU = 1280;

        /// <summary>
        /// Max size of array that will be sent to or can be received from <see cref="ISocket"/>
        /// <para>This will also be the size of all buffers used by <see cref="Peer"/></para>
        /// <para>This is not max message size because this size includes packets header added by <see cref="Peer"/></para>
        /// </summary>
        // todo move these settings to socket
        public static int MaxPacketSize => MIN_MTU - HEADER_SIZE;
    }
}
