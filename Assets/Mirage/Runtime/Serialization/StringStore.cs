using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using Mirage.Logging;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Serialization
{
    public class StringStore
    {
        /// <summary>Fast lookup to get index from an existing string</summary>
        public Dictionary<string, int> WriteLookup = new Dictionary<string, int>();
        public List<string> Strings = new List<string>();

        public int GetKey(string value)
        {
            if (WriteLookup.TryGetValue(value, out var index))
            {
                return index;
            }
            else
            {
                index = Strings.Count;
                Strings.Add(value);
                WriteLookup.Add(value, index);
                return index;
            }
        }

        public void WriteString(NetworkWriter writer, string value)
        {
            if (value == null)
            {
                writer.WritePackedUInt32(0);
            }
            else
            {
                var key = GetKey(value);
                writer.WritePackedUInt32(checked((uint)(key + 1)));
            }
        }

        public string ReadString(NetworkReader reader)
        {
            var key = reader.ReadPackedUInt32();
            if (key == 0)
                return null;

            var index = checked((int)(key - 1));
            return Strings[index];
        }
    }

    /// <summary>Default write/read methods for <see cref="StringStore"/>, using <see cref="StringExtensions.defaultEncoding"/></summary>
    public static class StringStoreExtensions
    {
        public static void WriteStringStore(this NetworkWriter writer, StringStore store)
        {
            var count = (uint)store.Strings.Count;
            writer.WritePackedUInt32(count);
            for (var i = 0; i < count; i++)
                // use defaultEncoding, so we use the real write method and not the one that uses StringStore
                writer.WriteString(store.Strings[i], StringExtensions.defaultEncoding);
        }
        /// <summary>Default read method for <see cref="StringStore"/>, using <see cref="StringExtensions.defaultEncoding"/></summary>
        public static StringStore ReadStringStore(this NetworkReader reader)
        {
            var store = new StringStore();
            var list = store.Strings;
            var count = reader.ReadPackedUInt32();
            for (var i = 0; i < count; i++)
                list.Add(reader.ReadString(StringExtensions.defaultEncoding));
            return store;
        }
    }
}

namespace Mirage.Serialization.BrotliCompression
{
    [NetworkMessage]
    public struct StringStoreLengthsMessage
    {
        public ushort stringCount;
        public ArraySegment<byte> payload;

        public int[] GetLengths()
        {
            var lengths = new int[stringCount];
            using (var reader = NetworkReaderPool.GetReader(payload, null))
            {
                for (var i = 0; i < stringCount; i++)
                    lengths[i] = (int)reader.ReadPackedUInt32();
            }
            return lengths;
        }
    }

    [NetworkMessage]
    public struct StringStoreStringsMessage
    {
        public ArraySegment<byte> payload;
    }

    /// <summary>Advanced write/read methods that use <see cref="BrotliEncoder>"/> to compress strings inside <see cref="StringStore"/></summary>
    public class StringStoreBrotliEncoder
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(StringStoreBrotliEncoder));

        private readonly NetworkWriter _lengthsWriter;
        private readonly NetworkWriter _stringsWriter;

        public (int lengthsByteCount, int stringsByteCount) GetByteLength()
        {
            return (_lengthsWriter.ByteLength, _stringsWriter.ByteLength);
        }

        private StringStoreBrotliEncoder(NetworkWriter lengthsWriter, NetworkWriter stringsWriter)
        {
            _lengthsWriter = lengthsWriter;
            _stringsWriter = stringsWriter;
        }

        public void Send(INetworkPlayer player)
        {
            if (_stringsWriter == null)
                throw new InvalidOperationException("Encode(StringStore) should be called before send");

            try
            {
                var lengthSegment = _lengthsWriter.ToArraySegment();
                var stringSegment = _stringsWriter.ToArraySegment();
                if (logger.LogEnabled()) logger.Log($"Sending StringStoreBrotliEncoder Lengths:{lengthSegment.Count}bytes, Strings:{stringSegment.Count}Bytes");

                NetworkDiagnostics.OnSend(new StringStoreLengthsMessage(), lengthSegment.Count, 1);
                NetworkDiagnostics.OnSend(new StringStoreStringsMessage(), stringSegment.Count, 1);

                player.Send(lengthSegment, Channel.Reliable);
                player.Send(stringSegment, Channel.Reliable);
            }
            catch (BufferFullException e)
            {
                Debug.LogError($"Failed to send mission to {player} because their buffer was full: {e}");
                player.Disconnect();
            }
        }
        public void Send(List<INetworkPlayer> players)
        {
            foreach (var player in players)
                Send(player);
        }

        /// <summary>
        /// Encodes the StringStore so it is ready to send
        /// </summary>
        /// <param name="stringStore"></param>
        /// <param name="lazyEncode"></param>
        public static unsafe StringStoreBrotliEncoder Encode(StringStore stringStore, int? _maxMessageSize = null)
        {
            var maxMessageSize = _maxMessageSize ?? (255 * 1100);
            var (stringByteLength, stringWriter) = CompressStringStoreStringsMessage(stringStore, maxMessageSize);
            var lengthWriter = CompressStringStoreLengthsMessage(stringByteLength);
            return new StringStoreBrotliEncoder(lengthWriter, stringWriter);
        }

        private static unsafe NetworkWriter CompressStringStoreLengthsMessage(int[] stringByteLength)
        {
            // assume most lengths will be under 200, but add a bit extra incase so we dont resize if 1 is over
            var writer = new NetworkWriter(Mathf.Max(1000, (int)(stringByteLength.Length * 1.2f)));
            // first write meta data
            writer.WriteUInt16((ushort)MessagePacker.GetId<StringStoreLengthsMessage>());
            writer.WriteUInt16((ushort)stringByteLength.Length);
            writer.WriteUInt16(0); // payload size placeholder
            for (var i = 0; i < stringByteLength.Length; i++)
                writer.WritePackedUInt32((uint)stringByteLength[i]); // use uint to avoid zigzag encoding

            var payloadLength = writer.ByteLength - 6;
            writer.WriteAtBytePosition((ulong)payloadLength, bits: 16, bytePosition: 4);
            return writer;
        }
        private static unsafe (int[] stringByteLength, NetworkWriter stringWriter) CompressStringStoreStringsMessage(StringStore stringStore, int maxMessageSize)
        {
            var rawPtr = IntPtr.Zero;
            var outPtr = IntPtr.Zero;

            try
            {
                var totalBytes = 0;
                // note: no strings in StringStore will be null, null is written as special index
                foreach (var str in stringStore.Strings)
                    totalBytes += Encoding.UTF8.GetByteCount(str);

                // allocate buffer to merge strings and pass to BrotliEncoder
                rawPtr = Marshal.AllocHGlobal(totalBytes);
                var rawSpan = new Span<byte>(rawPtr.ToPointer(), totalBytes);

                // encode all strings to rawSpan
                var offset = 0;
                var stringByteLength = new int[stringStore.Strings.Count];
                for (var i = 0; i < stringStore.Strings.Count; i++)
                {
                    var str = stringStore.Strings[i];
                    var byteLength = Encoding.UTF8.GetBytes(str, rawSpan[offset..]);
                    stringByteLength[i] = byteLength;
                    offset += byteLength;
                }


                // reasonable max size, 255 fragment * 1100 MTU packets
                var outByteSize = Mathf.Clamp(totalBytes + 6, 1000, maxMessageSize);
                outPtr = Marshal.AllocHGlobal(outByteSize);
                var outSpan = new Span<byte>(outPtr.ToPointer(), outByteSize);

                // compress
                // quality 11 -> max compression
                // window 20 -> 1mb lookback
                if (BrotliEncoder.TryCompress(rawSpan[..offset], outSpan, out var bytesWritten, quality: 11, window: 20))
                {
                    if (logger.LogEnabled()) logger.Log($"BrotliEncoder bytes {offset} -> {bytesWritten}");

                    var writer = new NetworkWriter(6 + bytesWritten, allowResize: false);
                    writer.WriteUInt16((ushort)MessagePacker.GetId<StringStoreStringsMessage>());
                    writer.WriteUInt32((uint)bytesWritten);
                    writer.WriteSpanRaw(outSpan[..bytesWritten]);
                    return (stringByteLength, writer);
                }
                else
                {
                    throw new Exception("Brotli compression failed: Data exceeds buffer.");
                }
            }
            finally
            {
                if (rawPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(rawPtr);
                if (outPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(outPtr);
            }
        }
    }

    /// <summary>
    /// Used to receive the next StringStore sent.
    /// <para>
    /// Will unregister message handlers after receiving StringStore
    /// </para>
    /// </summary>
    public class StringStoreBrotliDecoder
    {
        private int[] _lengthsMessage;
        private readonly IMessageReceiver _receiver;

        public StringStore StringStore { get; private set; }
        public event Action OnReceived;

        public StringStoreBrotliDecoder(IMessageReceiver receiver)
        {
            _receiver = receiver;
            receiver.RegisterHandler<StringStoreLengthsMessage>(HandleStringStoreLengthsMessage);
        }

        public void HandleStringStoreLengthsMessage(StringStoreLengthsMessage message)
        {
            _lengthsMessage = message.GetLengths();

            _receiver.UnregisterHandler<StringStoreLengthsMessage>();
            _receiver.RegisterHandler<StringStoreStringsMessage>(HandleStringStoreStringsMessage);
        }

        public void HandleStringStoreStringsMessage(StringStoreStringsMessage message)
        {
            StringStore = DecompressStringStore(_lengthsMessage, message);
            _receiver.UnregisterHandler<StringStoreStringsMessage>();
            OnReceived?.Invoke();
        }

        private static unsafe StringStore DecompressStringStore(int[] stringByteLength, StringStoreStringsMessage message)
        {
            var uncompressedByteCount = 0;
            for (var i = 0; i < stringByteLength.Length; i++)
                uncompressedByteCount += stringByteLength[i];

            var rawPtr = Marshal.AllocHGlobal(uncompressedByteCount);
            try
            {
                var decompressedSpan = new Span<byte>(rawPtr.ToPointer(), uncompressedByteCount);
                ReadOnlySpan<byte> compressedSpan = message.payload;

                // Brotli will tell us exactly how many bytes it wrote
                if (!BrotliDecoder.TryDecompress(compressedSpan, decompressedSpan, out var totalBytesWritten))
                {
                    throw new Exception("Brotli decompression failed.");
                }
                Debug.Assert(totalBytesWritten == uncompressedByteCount, "BrotliDecoder wrote a different number of bytes than server sent");

                var stringStore = new StringStore();
                stringStore.Strings = new List<string>(stringByteLength.Length);

                var currentOffset = 0;
                for (var i = 0; i < stringByteLength.Length; i++)
                {
                    var byteLen = stringByteLength[i];
                    if (byteLen == 0)
                    {
                        stringStore.Strings.Add(string.Empty);
                        continue;
                    }

                    var s = Encoding.UTF8.GetString(decompressedSpan.Slice(currentOffset, byteLen));
                    stringStore.Strings.Add(s);
                    currentOffset += byteLen;
                }

                return stringStore;
            }
            finally
            {
                Marshal.FreeHGlobal(rawPtr);
            }
        }

    }

    public static class StringStoreBrotliEncoderExtensions
    {
        public static void WriteStringStoreLengthsMessage(this NetworkWriter writer, StringStoreLengthsMessage part)
        {
            throw new NotSupportedException("StringStoreLengthsMessage should not be written automatically");
        }
        public static StringStoreLengthsMessage ReadStringStoreLengthsMessage(this NetworkReader reader)
        {
            var count = reader.ReadUInt16();
            var segmentLength = reader.ReadUInt16();
            var segment = reader.ReadBytesSegment(segmentLength);
            return new StringStoreLengthsMessage
            {
                stringCount = count,
                payload = segment,
            };
        }
        public static void WriteStringStoreStringsMessage(this NetworkWriter writer, StringStoreStringsMessage part)
        {
            throw new NotSupportedException("StringStoreStringsMessage should not be written automatically");
        }
        public static StringStoreStringsMessage ReadStringStoreStringsMessage(this NetworkReader reader)
        {
            var segmentLength = reader.ReadUInt32();
            var segment = reader.ReadBytesSegment((int)segmentLength);
            return new StringStoreStringsMessage
            {
                payload = segment,
            };
        }
    }
}
