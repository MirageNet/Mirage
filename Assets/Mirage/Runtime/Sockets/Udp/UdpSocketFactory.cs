// nanosockets breaks on some platforms (like iOS)
// so only include it for standalone and editor
// but not for mac because of code signing issue
#if (UNITY_STANDALONE || UNITY_EDITOR) && !(UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
#define NANO_SOCKET_ALLOWED
#endif

using Mirage.SocketLayer;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
#if NANO_SOCKET_ALLOWED
using NanoSockets;
#endif

namespace Mirage.Sockets.Udp
{
    public enum SocketLib { Automatic, Native, Managed };

    public sealed class UdpSocketFactory : SocketFactory, IHasAddress, IHasPort
    {
        public string Address = "localhost";
        public ushort Port = 7777;

        [Tooltip("Allows you to set which Socket implementation you want to use.\nAutomatic will use native (NanoSockets) on supported platforms (Windows, Mac & Linux).")]
        public SocketLib SocketLib;

        [Header("NanoSocket options")]
        public int BufferSize = 256 * 1024;

        public override int MaxPacketSize => UdpMTU.MaxPacketSize;

        private bool useNanoSocket => SocketLib == SocketLib.Native || (SocketLib == SocketLib.Automatic && IsDesktop);

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

        private static int initCount;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ClearCounter()
        {
            initCount = 0;
        }

        private void Awake()
        {
            if (!useNanoSocket) return;

            // NanoSocket is only available on Windows, Mac and Linux
            // However on newer versions of Mac it causes the standalone builds
            // to be unable to load the NanoSocket native library. So we just use
            // C# Managed sockets instead.

            // give different warning for OSX
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            Debug.LogWarning("NanoSocket support on MacOS is tempermental due to codesigning issues.\nTo ensure functionality, C# sockets will be used instead. This message is harmless (don't panic!).");
            SocketLib = SocketLib.Managed;
            return;

            // "Standalone" here is referring to Win64 or Linux64, but not mac, because that should be covered by case above
#elif NANO_SOCKET_ALLOWED
            // Attempt initialization of NanoSockets native library. If this fails, go back to native.
            InitializeNanoSockets();
#else
            Debug.LogWarning("NanoSocket does not support this platform (non-desktop platform detected). Switching to C# Managed sockets.");
            this.SocketLib = SocketLib.Managed;
#endif
        }

#if NANO_SOCKET_ALLOWED
        // Initializes the NanoSockets native library. If it fails, it resorts to C# Managed Sockets.
        private void InitializeNanoSockets()
        {
            try
            {
                if (initCount == 0) UDP.Initialize();

                initCount++;
            }
            catch (DllNotFoundException)
            {
                Debug.LogWarning("NanoSocket DLL not found or failed to load. Switching to C# Managed Sockets.");
                SocketLib = SocketLib.Managed;
            }
        }
#endif

        private void OnDestroy()
        {
            if (!useNanoSocket) return;

#if NANO_SOCKET_ALLOWED
            initCount--;

            if (initCount == 0)
            {
                UDP.Deinitialize();
            }
#endif
        }

        public override ISocket CreateClientSocket()
        {
            ThrowIfNotSupported();

#if NANO_SOCKET_ALLOWED
            if (useNanoSocket) return new NanoSocket(this);
#endif

            return new UdpSocket();
        }

        public override ISocket CreateServerSocket()
        {
            ThrowIfNotSupported();

#if NANO_SOCKET_ALLOWED
            if (useNanoSocket) return new NanoSocket(this);
#endif

            return new UdpSocket();
        }

        public override IEndPoint GetBindEndPoint()
        {
#if NANO_SOCKET_ALLOWED
            if (useNanoSocket) return new NanoEndPoint("::0", Port);
#endif

            return new EndPointWrapper(new IPEndPoint(IPAddress.IPv6Any, Port));
        }

        public override IEndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            var addressString = address ?? Address;
            var ipAddress = getAddress(addressString);

            var portIn = port ?? Port;

#if NANO_SOCKET_ALLOWED
            if (useNanoSocket) return new NanoEndPoint(addressString, portIn);
#endif

            return new EndPointWrapper(new IPEndPoint(ipAddress, portIn));
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

        private void ThrowIfNotSupported()
        {
            if (IsWebgl)
            {
                throw new NotSupportedException("The WebGL platform does not support UDP Sockets. Please use WebSockets instead.");
            }
        }

        private static bool IsWebgl => Application.platform == RuntimePlatform.WebGLPlayer;
        private static bool IsDesktop =>
            Application.platform == RuntimePlatform.LinuxPlayer
            || Application.platform == RuntimePlatform.OSXPlayer
            || Application.platform == RuntimePlatform.WindowsPlayer
            || Application.isEditor;
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
