using System;
using System.Runtime.InteropServices;
using Mirage.Serialization;

namespace Mirage.RemoteCalls
{
    [NetworkMessage]
    public struct RpcMessage
    {
        public uint NetId;
        public int FunctionIndex;
        public ArraySegment<byte> Payload;
    }

    [NetworkMessage]
    public struct RpcWithReplyMessage
    {
        public uint NetId;
        public int FunctionIndex;

        /// <summary>
        /// Id sent with rpc so that server can reply with <see cref="RpcReply"/> and send the same Id
        /// </summary>
        public int ReplyId;

        public ArraySegment<byte> Payload;
    }

    [NetworkMessage]
    public struct RpcReply
    {
        public int ReplyId;
        /// <summary>If result is returned, or exception was thrown</summary>
        public bool Success;
        public ArraySegment<byte> Payload;
    }

    public interface IPayloadViewSource
    {
        int Version { get; }
    }


    /// <summary>
    /// View into payload array, should only be used while underlying data is in scope and not disposed
    /// </summary>
    public unsafe struct PayloadView
    {
        private readonly byte* _ptr;
        private readonly int _length;
        private readonly int _sourceVersion;
        private readonly IPayloadViewSource _source;

        public PayloadView(UnsafeView payload, IPayloadViewSource source)
        {
            _ptr = payload.ptr;
            _length = payload.length;
            _sourceVersion = source.Version;
            _source = source;
        }

        public Span<byte> Get()
        {
            if (_source.Version != _sourceVersion)
            {
                throw new AccessViolationException("Payload version did not match, old payload was recyled");
            }

            return new Span<byte>(_ptr, _length);
        }
    }


    /// <summary>
    /// should only be used with an already pinned buffer
    /// </summary>
    public unsafe ref struct UnsafeView
    {
        public readonly byte* ptr;
        public readonly int length;

        public UnsafeView(byte* ptr, int length)
        {
            this.ptr = ptr;
            this.length = length;
        }
    }

    public class Example
    {
        public unsafe void OnData(Span<byte> data)
        {
            fixed (byte* ptr = data)
            {
                var span = new UnsafeView(ptr, data.Length);
                using (var reader = NetworkReaderPool.GetReader())
                {
                    // give reader the new data
                    // previously it could be a manged array, but must now be our UnsafeView

                    // reset will also increment version
                    reader.Reset(span);

                    // ... decoding byte to network message (deeper in stack)
                    RpcReply rpcReply;
                    // ... normal reading of ints
                    rpcReply.Payload = ReadPayloadView(reader);  // will be changed to reader.ReadPayloadView later

                    // ... handle rpc (deeper in stack)
                    using (var reader2 = NetworkReaderPool.GetReader())
                    {
                        // use the already pinned PayloadView
                        reader2.Reset(rpcReply.Payload);

                        // ... decode payload
                        // ... run rpc
                    }
                }
                // increasing IPayloadViewSource version int here 
                // use UnsafeSpan in NetworkReader, byte* will be "safe"
            }
        }

        private static void SendReply<T>(INetworkPlayer senderPlayer, int replyId, bool success, T result)
        {
            var serverRpcReply = new RpcReply
            {
                ReplyId = replyId,
                Success = success,
            };
            if (success)
            {
                // if success, write payload and send
                // else just send it without payload (since there is no result)
                using (var writer = NetworkWriterPool.GetWriter())
                {
                    writer.Write(result);
                    serverRpcReply.Payload = ToPayloadView(writer); // will be changed to writer.ToPayloadView later
                    senderPlayer.Send(serverRpcReply);
                }
            }
            else
            {
                senderPlayer.Send(serverRpcReply);
            }
        }
        private static unsafe PayloadView ReadPayloadView(NetworkReader reader)
        {
            byte* ptr = reader.GetPinnedBuffer(); // internally pinned buffer used for fast serialization
            int length = ...;
            // ... same position and length code that we have now for ArraySegment

            var view = new UnsafeView(ptr, length);
            return new PayloadView(view, reader);
        }
        private static unsafe PayloadView ToPayloadView(NetworkWriter writer)
        {
            var ptr = writer.GetPinnedBuffer(); // internally pinned buffer used for fast serialization
            var length = writer.ByteLength; // number of bytes written
            var view = new UnsafeView(ptr, length);
            return new PayloadView(view, writer);
        }


        // old/Current NetworkReader
        public unsafe class NetworkReader_Old : IDisposable
        {
            private byte[] _managedBuffer;
            private GCHandle _handle; // pin for managed buffer
            private ulong* _longPtr;  // ptr to managed buffer, cast as ulong* for big packing 
            private bool _needsDisposing; // is buffer pinned?

            /// <summary>Current read position</summary>
            private int _bitPosition;

            /// <summary>Offset of given buffer</summary>
            private int _bitOffset;

            /// <summary>Length of given buffer</summary>
            private int _bitLength;

            // ... methods to handle reading data from pointer (uses bit packing)
        }


        // new NetworkReader
        public unsafe class NetworkReader
        {
            protected ulong* _longPtr;  // just a pointer, cast as ulong* for big packing 

            /// <summary>Current read position</summary>
            private int _bitPosition;

            /// <summary>Offset of given buffer</summary>
            private int _bitOffset;

            /// <summary>Length of given buffer</summary>
            protected int _bitLength;

            // ... methods to handle reading data from pointer (uses bit packing)
        }

        public unsafe class ManagedNetworkReader : NetworkReader, IDisposable
        {
            private byte[] _managedBuffer;
            private GCHandle _handle; // pin for managed buffer
            private bool _needsDisposing; // is buffer pinned?

            // will pass set base._longPtr when setting buffer
        }

        public unsafe class SpanNetworkReader : NetworkReader
        {
            // will pass set base._longPtr when setting buffer
            // must be given a pinned span?


            public unsafe void Reset(UnsafeView view)
            {
                _longPtr = (ulong*)view.ptr;
                _bitLength = view.length * 8;
            }
        }
    }
}
