using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using JamesFrowen.SimpleWeb;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Sockets.Udp
{
    public sealed class WebSocketFactory : SocketFactory
    {
        [SerializeField] string address = "localhost";
        [SerializeField] int port = 7777;

        public override ISocket CreateClientSocket()
        {
            if (IsWebgl)
                return new WebGLWebSocket();
            else
                return new StandAloneWebSocket();
        }

        public override ISocket CreateServerSocket()
        {
            if (IsWebgl)
            {
                throw new NotSupportedException("Webgl can not be a server");
            }

            return new StandAloneWebSocket();
        }

        public override EndPoint GetBindEndPoint()
        {
            return new IPEndPoint(IPAddress.IPv6Any, port);
        }

        public override EndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            string addressString = address ?? this.address;
            IPAddress ipAddress = getAddress(addressString);

            ushort portIn = port ?? (ushort)this.port;

            return new IPEndPoint(ipAddress, portIn);
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

        private static bool IsWebgl => Application.platform == RuntimePlatform.WebGLPlayer;
    }

    public class StandAloneWebSocket : ISocket
    {
        // todo connect handshake
        // todo message encode/decode

        Socket socket;
        IPEndPoint AnyEndpoint;
        // todo set size of this buffer
        byte[] buffer = new byte[1300];
        WebSocketEncoding webSocketEncoding;


        public void Bind(EndPoint endPoint)
        {
            webSocketEncoding = new WebSocketEncoding(false);

            AnyEndpoint = endPoint as IPEndPoint;

            socket = CreateSocket(endPoint);
            socket.DualMode = true;

            socket.Bind(endPoint);
        }

        static Socket CreateSocket(EndPoint endPoint)
        {
            // todo check this config
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                Blocking = false,
            };

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            TrySetIOControl(socket);

            return socket;
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
                byte[] _false = new byte[] { 0, 0, 0, 0 };

                socket.IOControl(unchecked((int)SIO_UDP_CONNRESET), _false, null);
            }
            catch (Exception e)
            {
                Debug.LogError("Exception setting IOControl");
                Debug.LogException(e);
            }
        }

        public void Connect(EndPoint endPoint)
        {
            webSocketEncoding = new WebSocketEncoding(true);
            AnyEndpoint = endPoint as IPEndPoint;

            socket = CreateSocket(endPoint);

            socket.Connect(endPoint);
        }

        public void Close()
        {
            socket.Close();
            socket.Dispose();
        }

        /// <summary>
        /// Is message avaliable
        /// </summary>
        /// <returns>true if data to read</returns>
        public bool Poll()
        {
            return socket.Poll(0, SelectMode.SelectRead);
        }



        public int Receive(byte[] buffer, out EndPoint endPoint)
        {
            endPoint = AnyEndpoint;
            int c = socket.ReceiveFrom(buffer, ref endPoint);
            return c;
        }

        public void Send(EndPoint endPoint, byte[] packet, int length)
        {
            int sendLength =
            socket.SendTo(packet, length, SocketFlags.None, endPoint);
        }

    }

    public class WebSocketEncoding
    {
        readonly bool setMask;
        readonly MaskHelper maskHelper;
        public WebSocketEncoding(bool setMask)
        {
            this.setMask = setMask;
            maskHelper = new MaskHelper();
        }

        public int Encode(byte[] src, int length, byte[] dst)
        {
            int offset = 0;
            offset = WriteHeader(dst, offset, length, setMask);

            if (setMask)
            {
                offset = maskHelper.WriteMask(dst, offset);
            }

            Buffer.BlockCopy(src, 0, dst, offset, length);
            offset += length;

            if (setMask)
            {
                int messageOffset = offset - length;
                MessageProcessor.ToggleMask(dst, messageOffset, length, dst, messageOffset - Constants.MaskSize);
            }

            return offset;
        }

        static int WriteHeader(byte[] buffer, int startOffset, int msgLength, bool setMask)
        {
            int sendLength = 0;
            const byte finished = 128;
            const byte byteOpCode = 2;

            buffer[startOffset + 0] = finished | byteOpCode;
            sendLength++;

            if (msgLength <= Constants.BytePayloadLength)
            {
                buffer[startOffset + 1] = (byte)msgLength;
                sendLength++;
            }
            else if (msgLength <= ushort.MaxValue)
            {
                buffer[startOffset + 1] = 126;
                buffer[startOffset + 2] = (byte)(msgLength >> 8);
                buffer[startOffset + 3] = (byte)msgLength;
                sendLength += 3;
            }
            else
            {
                throw new InvalidDataException($"Trying to send a message larger than {ushort.MaxValue} bytes");
            }

            if (setMask)
            {
                buffer[startOffset + 1] |= 0b1000_0000;
            }

            return sendLength + startOffset;
        }
    }

    public class WebGLWebSocket : ISocket
    {
        public void Bind(EndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Connect(EndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public bool Poll()
        {
            throw new NotImplementedException();
        }

        public int Receive(byte[] buffer, out EndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public void Send(EndPoint endPoint, byte[] packet, int length)
        {
            throw new NotImplementedException();
        }
    }
}


namespace JamesFrowen.SimpleWeb
{
    sealed class MaskHelper : IDisposable
    {
        readonly byte[] maskBuffer;
        readonly RNGCryptoServiceProvider random;

        public MaskHelper()
        {
            maskBuffer = new byte[4];
            random = new RNGCryptoServiceProvider();
        }
        public void Dispose()
        {
            random.Dispose();
        }

        public int WriteMask(byte[] buffer, int offset)
        {
            random.GetBytes(maskBuffer);
            Buffer.BlockCopy(maskBuffer, 0, buffer, offset, 4);

            return offset + 4;
        }
    }
    public static class MessageProcessor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static byte FirstLengthByte(byte[] buffer) => (byte)(buffer[1] & 0b0111_1111);

        public static bool NeedToReadShortLength(byte[] buffer)
        {
            byte lenByte = FirstLengthByte(buffer);

            return lenByte >= Constants.UshortPayloadLength;
        }

        public static int GetOpcode(byte[] buffer)
        {
            return buffer[0] & 0b0000_1111;
        }

        public static int GetPayloadLength(byte[] buffer)
        {
            byte lenByte = FirstLengthByte(buffer);
            return GetMessageLength(buffer, 0, lenByte);
        }

        public static void ValidateHeader(byte[] buffer, int maxLength, bool expectMask)
        {
            bool finished = (buffer[0] & 0b1000_0000) != 0; // has full message been sent
            bool hasMask = (buffer[1] & 0b1000_0000) != 0; // true from clients, false from server, "All messages from the client to the server have this bit set"

            int opcode = buffer[0] & 0b0000_1111; // expecting 1 - text message
            byte lenByte = FirstLengthByte(buffer);

            ThrowIfNotFinished(finished);
            ThrowIfMaskNotExpected(hasMask, expectMask);
            ThrowIfBadOpCode(opcode);

            int msglen = GetMessageLength(buffer, 0, lenByte);

            ThrowIfLengthZero(msglen);
            ThrowIfMsgLengthTooLong(msglen, maxLength);
        }

        public static void ToggleMask(byte[] src, int sourceOffset, int messageLength, byte[] maskBuffer, int maskOffset)
        {
            ToggleMask(src, sourceOffset, src, sourceOffset, messageLength, maskBuffer, maskOffset);
        }

        public static void ToggleMask(byte[] src, int srcOffset, byte[] dst, int dstOffset, int messageLength, byte[] maskBuffer, int maskOffset)
        {
            for (int i = 0; i < messageLength; i++)
            {
                byte maskByte = maskBuffer[maskOffset + i % Constants.MaskSize];
                dst[dstOffset + i] = (byte)(src[srcOffset + i] ^ maskByte);
            }
        }

        /// <exception cref="InvalidDataException"></exception>
        static int GetMessageLength(byte[] buffer, int offset, byte lenByte)
        {
            if (lenByte == Constants.UshortPayloadLength)
            {
                // header is 4 bytes long
                ushort value = 0;
                value |= (ushort)(buffer[offset + 2] << 8);
                value |= buffer[offset + 3];

                return value;
            }
            else if (lenByte == Constants.UlongPayloadLength)
            {
                throw new InvalidDataException("Max length is longer than allowed in a single message");
            }
            else // is less than 126
            {
                // header is 2 bytes long
                return lenByte;
            }
        }

        /// <exception cref="InvalidDataException"></exception>
        static void ThrowIfNotFinished(bool finished)
        {
            if (!finished)
            {
                throw new InvalidDataException("Full message should have been sent, if the full message wasn't sent it wasn't sent from this trasnport");
            }
        }

        /// <exception cref="InvalidDataException"></exception>
        static void ThrowIfMaskNotExpected(bool hasMask, bool expectMask)
        {
            if (hasMask != expectMask)
            {
                throw new InvalidDataException($"Message expected mask to be {expectMask} but was {hasMask}");
            }
        }

        /// <exception cref="InvalidDataException"></exception>
        static void ThrowIfBadOpCode(int opcode)
        {
            // 2 = binary
            // 8 = close
            if (opcode != 2 && opcode != 8)
            {
                throw new InvalidDataException("Expected opcode to be binary or close");
            }
        }

        /// <exception cref="InvalidDataException"></exception>
        static void ThrowIfLengthZero(int msglen)
        {
            if (msglen == 0)
            {
                throw new InvalidDataException("Message length was zero");
            }
        }

        /// <summary>
        /// need to check this so that data from previous buffer isnt used
        /// </summary>
        /// <exception cref="InvalidDataException"></exception>
        static void ThrowIfMsgLengthTooLong(int msglen, int maxLength)
        {
            if (msglen > maxLength)
            {
                throw new InvalidDataException("Message length is greater than max length");
            }
        }
    }
    /// <summary>
    /// Constant values that should never change
    /// <para>
    /// Some values are from https://tools.ietf.org/html/rfc6455
    /// </para>
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Header is at most 4 bytes
        /// <para>
        /// If message is less than 125 then header is 2 bytes, else header is 4 bytes
        /// </para>
        /// </summary>
        public const int HeaderSize = 4;

        /// <summary>
        /// Smallest size of header
        /// <para>
        /// If message is less than 125 then header is 2 bytes, else header is 4 bytes
        /// </para>
        /// </summary>
        public const int HeaderMinSize = 2;

        /// <summary>
        /// bytes for short length
        /// </summary>
        public const int ShortLength = 2;

        /// <summary>
        /// Message mask is always 4 bytes
        /// </summary>
        public const int MaskSize = 4;

        /// <summary>
        /// Max size of a message for length to be 1 byte long
        /// <para>
        /// payload length between 0-125
        /// </para>
        /// </summary>
        public const int BytePayloadLength = 125;

        /// <summary>
        /// if payload length is 126 when next 2 bytes will be the length
        /// </summary>
        public const int UshortPayloadLength = 126;

        /// <summary>
        /// if payload length is 127 when next 8 bytes will be the length
        /// </summary>
        public const int UlongPayloadLength = 127;


        /// <summary>
        /// Guid used for WebSocket Protocol
        /// </summary>
        public const string HandshakeGUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public static readonly int HandshakeGUIDLength = HandshakeGUID.Length;

        public static readonly byte[] HandshakeGUIDBytes = Encoding.ASCII.GetBytes(HandshakeGUID);

        /// <summary>
        /// Handshake messages will end with \r\n\r\n
        /// </summary>
        public static readonly byte[] endOfHandshake = new byte[4] { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };
    }
}
