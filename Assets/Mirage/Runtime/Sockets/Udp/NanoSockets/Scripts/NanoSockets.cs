/*
 *  Lightweight UDP sockets abstraction for rapid implementation of message-oriented protocols
 *  Copyright (c) 2019 Stanislav Denisov
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace NanoSockets
{
    public enum Status
    {
        OK = 0,
        Error = -1
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct Socket
    {
        [FieldOffset(0)]
        private long handle;

        public bool IsCreated
        {
            get
            {
                return handle > 0;
            }
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 18)]
    public struct Address : IEquatable<Address>
    {
        [FieldOffset(0)]
        private ulong address0;
        [FieldOffset(8)]
        private ulong address1;
        [FieldOffset(16)]
        public ushort port;
        [FieldOffset(16)]
        public ushort Port;

        public bool Equals(Address other)
        {
            return address0 == other.address0 && address1 == other.address1 && port == other.port;
        }

        public override bool Equals(object obj)
        {
            if (obj is Address)
                return Equals((Address)obj);

            return false;
        }

        public override int GetHashCode()
        {
            var hash = 17;

            hash = hash * 31 + address0.GetHashCode();
            hash = hash * 31 + address1.GetHashCode();
            hash = hash * 31 + port.GetHashCode();

            return hash;
        }

        public override string ToString()
        {
            // FIX: Unity IL2CPP SIGABRT in 2021.3.15 on Linux builds
            // Problem: On Linux IL2CPP builds, it seems something with
            // IL2CPP and StringBuilder causes SIGABRT to be emitted due to
            // a bad free of a pointer: "free(): invalid pointer". Unity will then
            // commit suicide.
            // Solution: This successfully works around it, unfortunately at
            // a cost of 64 * 2 bytes of memory every time ToString() is called.
            // Improvements welcome.

            // Allocate 64 bytes of memory temporarily.
            var ipPtr = Marshal.AllocHGlobal(64);
            var ipAddress = string.Empty;

            // Check if the GetIP call was possible
            if (UDP.GetIP(ref this, ipPtr, 64) == 0)
            {
                // NanoSockets returned OK. Copy that to an ANSI string.
                // Pretty sure we won't have UTF-8 in our IP addresses...
                ipAddress = Marshal.PtrToStringAnsi(ipPtr, 64);

                // Free the memory, otherwise memory leak = bad!
                Marshal.FreeHGlobal(ipPtr);

                return string.Format("{0}:{1}", ipAddress, port);
            }
            else
            {
                // Free the allocated memory, even though we didn't use it.
                Marshal.FreeHGlobal(ipPtr);
                // Return a UNKNOWN string just to make the function happy.
                return "UNKNOWN";
            }

            // Original code is as follows.
            /*
            var ip = new StringBuilder(64);
            NanoSockets.UDP.GetIP(ref this, ip, 64);
            return string.Format("IP:{0} Port:{1}", ip, this.port);
            */
        }

        public static Address CreateFromIpPort(string ip, ushort port)
        {
            var address = default(Address);

            NanoSockets.UDP.SetIP(ref address, ip);
            address.port = port;

            return address;
        }
    }

    [SuppressUnmanagedCodeSecurity]
    public static class UDP
    {
#if __IOS__ || UNITY_IOS && !UNITY_EDITOR
			private const string nativeLibrary = "__Internal";
#else
        private const string nativeLibrary = "nanosockets";
#endif

        public const int hostNameSize = 1025;

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status Initialize();

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_deinitialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Deinitialize();

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_create", CallingConvention = CallingConvention.Cdecl)]
        public static extern Socket Create(int sendBufferSize, int receiveBufferSize);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Destroy(ref Socket socket);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_bind", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Bind(Socket socket, IntPtr address);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_bind", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Bind(Socket socket, ref Address address);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_connect", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Connect(Socket socket, ref Address address);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_set_option", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status SetOption(Socket socket, int level, int optionName, ref int optionValue, int optionLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_get_option", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetOption(Socket socket, int level, int optionName, ref int optionValue, ref int optionLength);

        public static Status SetNonBlocking(Socket socket, bool shouldBlock = false)
                => SetNonBlocking(socket, (byte)(shouldBlock ? 1 : 0));

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_set_nonblocking", CallingConvention = CallingConvention.Cdecl)]
        private static extern Status SetNonBlocking(Socket socket, byte state);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_set_dontfragment", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status SetDontFragment(Socket socket);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_poll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Poll(Socket socket, long timeout);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(Socket socket, IntPtr address, IntPtr buffer, int bufferLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(Socket socket, IntPtr address, byte[] buffer, int bufferLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(Socket socket, ref Address address, IntPtr buffer, int bufferLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(Socket socket, ref Address address, byte[] buffer, int bufferLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_send_offset", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(Socket socket, IntPtr address, byte[] buffer, int offset, int bufferLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_send_offset", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(Socket socket, ref Address address, byte[] buffer, int offset, int bufferLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(Socket socket, IntPtr address, IntPtr buffer, int bufferLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(Socket socket, IntPtr address, byte[] buffer, int bufferLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(Socket socket, ref Address address, IntPtr buffer, int bufferLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(Socket socket, ref Address address, byte[] buffer, int bufferLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_receive_offset", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(Socket socket, IntPtr address, byte[] buffer, int offset, int bufferLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_receive_offset", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(Socket socket, ref Address address, byte[] buffer, int offset, int bufferLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_address_get", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetAddress(Socket socket, ref Address address);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_address_is_equal", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status IsEqual(ref Address left, ref Address right);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_address_set_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status SetIP(ref Address address, IntPtr ip);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_address_set_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status SetIP(ref Address address, string ip);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_address_get_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetIP(ref Address address, IntPtr ip, int ipLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_address_get_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetIP(ref Address address, StringBuilder ip, int ipLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_address_set_hostname", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status SetHostName(ref Address address, IntPtr name);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_address_set_hostname", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status SetHostName(ref Address address, string name);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_address_get_hostname", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetHostName(ref Address address, IntPtr name, int nameLength);

        [DllImport(nativeLibrary, EntryPoint = "nanosockets_address_get_hostname", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetHostName(ref Address address, StringBuilder name, int nameLength);

#if NANOSOCKETS_UNSAFE_API
			public static unsafe class Unsafe {
				[DllImport(nativeLibrary, EntryPoint = "nanosockets_receive", CallingConvention = CallingConvention.Cdecl)]
				public static extern int Receive(Socket socket, Address* address, byte* buffer, int bufferLength);

				[DllImport(nativeLibrary, EntryPoint = "nanosockets_send", CallingConvention = CallingConvention.Cdecl)]
				public static extern int Send(Socket socket, Address* address, byte* buffer, int bufferLength);
			}
#endif
    }
}
