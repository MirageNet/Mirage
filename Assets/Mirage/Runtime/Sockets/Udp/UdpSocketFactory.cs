using System;
using System.Net;
using System.Net.Sockets;
using Mirage.SocketLayer;
using UnityEngine;
#if UNITY_STANDALONE || UNITY_EDITOR
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

        bool useNanoSocket => SocketLib == SocketLib.Native || (SocketLib == SocketLib.Automatic && IsDesktop);

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

        static int initCount;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ClearCounter()
        {
            initCount = 0;
        }

        void Awake()
        {
            if (!useNanoSocket) return;

			// NanoSocket is only available on Windows, Mac and Linux
			// However on newer versions of Mac it causes the standalone builds
			// to be unable to load the NanoSocket native library. So we just use
			// C# Managed sockets instead.
			
			// Are we a Unity Editor instance or standalone instance?
#if UNITY_STANDALONE || UNITY_EDITOR
			try
            {
                if (initCount == 0)
                {
                    UDP.Initialize();
                }

                initCount++;
            }
            catch (DllNotFoundException)
            {
                Debug.LogWarning("NanoSocket DLL not found or failed to load. Switching to C# Managed Sockets.");
                SocketLib = SocketLib.Managed;
                return;
            }
#else
            Debug.LogWarning("NanoSocket does not support this platform, switching to C# Managed sockets.");
            SocketLib = SocketLib.Managed;
#endif
        }

        void OnDestroy()
        {
            if (!useNanoSocket) return;

#if UNITY_STANDALONE || UNITY_EDITOR
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

#if UNITY_STANDALONE || UNITY_EDITOR
            if (useNanoSocket) return new NanoSocket(this);
#endif

            return new UdpSocket();
        }

        public override ISocket CreateServerSocket()
        {
            ThrowIfNotSupported();

#if UNITY_STANDALONE || UNITY_EDITOR
            if (useNanoSocket) return new NanoSocket(this);
#endif

            return new UdpSocket();
        }

        public override IEndPoint GetBindEndPoint()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (useNanoSocket) return new NanoEndPoint("::0", Port);
#endif

            return new EndPointWrapper(new IPEndPoint(IPAddress.IPv6Any, Port));
        }

        public override IEndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            string addressString = address ?? Address;
            IPAddress ipAddress = getAddress(addressString);

            ushort portIn = port ?? Port;

#if UNITY_STANDALONE || UNITY_EDITOR
            if (useNanoSocket) return new NanoEndPoint(addressString, portIn);
#endif

            return new EndPointWrapper(new IPEndPoint(ipAddress, portIn));
        }

        private IPAddress getAddress(string addressString)
        {
            if (IPAddress.TryParse(addressString, out IPAddress address))
                return address;

            IPAddress[] results = Dns.GetHostAddresses(addressString);
            if (results.Length == 0)
            {
                throw new SocketException((int)SocketError.HostNotFound);
            }
            else
            {
                return results[0];
            }
        }

        void ThrowIfNotSupported()
        {
            if (IsWebgl)
            {
                throw new NotSupportedException("Udp Socket can not be created in Webgl builds, Use WebSocket instead");
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
            EndPoint copy = inner.Create(inner.Serialize());
            return new EndPointWrapper(copy);
        }
    }

    public class UdpMTU
    {
        /// <summary>
        /// IPv6 + UDP Header
        /// </summary>
        const int HEADER_SIZE = 40 + 8;

        /// <summary>
        /// MTU is expected to be atleast this number
        /// </summary>
        const int MIN_MTU = 1280;

        /// <summary>
        /// Max size of array that will be sent to or can be received from <see cref="ISocket"/>
        /// <para>This will also be the size of all buffers used by <see cref="Peer"/></para>
        /// <para>This is not max message size because this size includes packets header added by <see cref="Peer"/></para>
        /// </summary>
        // todo move these settings to socket
        public static int MaxPacketSize => MIN_MTU - HEADER_SIZE;
    }
}
