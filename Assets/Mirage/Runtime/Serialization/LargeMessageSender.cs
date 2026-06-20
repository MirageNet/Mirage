using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Mirage.SocketLayer;

namespace Mirage.Serialization
{
    public class LargeMessageSender
    {
        private readonly int _maxPacketSize;
        private readonly IMessageReceiver _messageHandler;
        private uint _largeMessageId;

        public LargeMessageSender(int maxPacketSize, IMessageReceiver messageHandler)
        {
            _maxPacketSize = maxPacketSize;
            _messageHandler = messageHandler;
        }

        public async UniTask SendLargeMessage(IConnection connection, byte[] data, int offset, int length)
        {
            var notAcked = new List<uint>();
            SendInternal(connection, notAcked, data, offset, length);

            // todo wait for NACK message,
            //      if empty that means all parts have been acked
            //      other peer should be sending NACK message short delay after receiving first packet, and repeat
            await UniTask.Yield();
        }

        private void SendInternal(IConnection connection, List<uint> notAcked, byte[] data, int offset, int length)
        {
            // send type, Id (if this message), current fragment, max fragment. 
            //const int header = 1 + 4 + 4 + 4;
            var sizePerFragment = 1000;//_maxPacketSize - header;
            var fragments = length / sizePerFragment;
            var extra = length % sizePerFragment;
            _largeMessageId++;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                {
                    // send meta data
                    var meta = new LargeMessageFragmentMeta
                    {
                        Id = _largeMessageId,
                        Fragments = checked((uint)fragments),
                        Extra = checked((uint)extra)
                    };
                    MessagePacker.Pack(meta, writer);
                    connection.SendReliable(writer.ToArraySegment());
                    writer.Reset();
                }

                // send real data
                for (var i = 0; i < fragments; i++)
                {
                    var msg = new LargeMessageFragment
                    {
                        Id = _largeMessageId,
                        Fragment = checked((uint)i)
                    };
                    notAcked.Add(checked((uint)i));
                    // write head
                    MessagePacker.Pack(msg, writer);
                    // then write bytes
                    writer.WriteBytes(data, offset + (i * sizePerFragment), sizePerFragment);
                    connection.SendReliable(writer.ToArraySegment());
                    writer.Reset();
                }

                if (extra != 0)
                {
                    var msg = new LargeMessageFragment
                    {
                        Id = _largeMessageId,
                        Fragment = checked((uint)fragments)
                    };
                    notAcked.Add(checked((uint)fragments));
                    // write head
                    MessagePacker.Pack(msg, writer);
                    // then write bytes
                    writer.WriteBytes(data, offset + (fragments * sizePerFragment), extra);
                    connection.SendReliable(writer.ToArraySegment());
                    writer.Reset();
                }
            }
        }

        public struct LargeMessageFragmentMeta
        {
            public uint Id;
            public uint Fragments;
            public uint Extra;
        }
        public struct LargeMessageFragment
        {
            public uint Id;
            public uint Fragment;
            public NetworkReader Reader;
        }
        public struct LargeMessageFragmentNotAck
        {
            public uint Id;
            public List<uint> Fragment;
        }
    }

    public static class E
    {
        public static void WriteNetworkReader(this NetworkWriter writer, NetworkReader reader)
        {
            // nothing
        }

        public static NetworkReader ReadNetworkReader(this NetworkReader reader)
        {
            return reader;
        }
    }
}
