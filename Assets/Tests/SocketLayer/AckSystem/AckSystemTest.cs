using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests.AckSystemTests
{
    internal static class AckSystemTextExtensions
    {
        public static INotifyToken SendNotify(this AckSystem ackSystem, byte[] array)
        {
            return ackSystem.SendNotify(array, 0, array.Length);
        }
        public static void SendReliable(this AckSystem ackSystem, byte[] array)
        {
            ackSystem.SendReliable(array, 0, array.Length);
        }
    }
    /// <summary>
    /// helper methods for testing AckSystem
    /// </summary>
    public class AckSystemTestBase
    {
        public const int MAX_PACKET_SIZE = 1300;
        protected readonly Random rand = new Random();
        protected Pool<ByteBuffer> bufferPool = new Pool<ByteBuffer>(ByteBuffer.CreateNew, MAX_PACKET_SIZE, 100, 1000);
        protected Pool<AckSystem.ReliablePacket> reliablePool;
        protected Pool<RingBuffer<AckSystem.AckablePacket>> ackablePacketPool;
        protected Pool<RingBuffer<AckSystem.ReliableReceived>> reliableReceivePool;

        private readonly List<RingBuffer<AckSystem.AckablePacket>> _takenAckables = new List<RingBuffer<AckSystem.AckablePacket>>();
        private readonly List<RingBuffer<AckSystem.ReliableReceived>> _takenReceives = new List<RingBuffer<AckSystem.ReliableReceived>>();

        public AckSystemTestBase()
        {
            var config = new Config();
            reliablePool = new Pool<AckSystem.ReliablePacket>(AckSystem.ReliablePacket.CreateNew, 0, config.MaxReliablePacketsInSendBufferPerConnection);
            ackablePacketPool = new Pool<RingBuffer<AckSystem.AckablePacket>>((p) => new RingBuffer<AckSystem.AckablePacket>(config.SequenceSize, null), 0, 10);
            reliableReceivePool = new Pool<RingBuffer<AckSystem.ReliableReceived>>((p) => new RingBuffer<AckSystem.ReliableReceived>(config.SequenceSize, null), 0, 10);
        }

        [TearDown]
        public void BaseTearDown()
        {
            foreach (var item in _takenAckables)
                ackablePacketPool.Put(item);
            _takenAckables.Clear();

            foreach (var item in _takenReceives)
                reliableReceivePool.Put(item);
            _takenReceives.Clear();
        }

        protected AckSystem CreateAckSystem(IRawConnection connection, Config config, ITime time = null, Action onInvalidPacket = null)
        {
            var defaultSequenceSize = new Config().SequenceSize;
            RingBuffer<AckSystem.AckablePacket> ackable;
            RingBuffer<AckSystem.ReliableReceived> receive;

            if (config.SequenceSize == defaultSequenceSize)
            {
                ackable = ackablePacketPool.Take();
                ackable.Reset();
                _takenAckables.Add(ackable);

                receive = reliableReceivePool.Take();
                receive.Reset();
                _takenReceives.Add(receive);
            }
            else
            {
                ackable = new RingBuffer<AckSystem.AckablePacket>(config.SequenceSize, null);
                receive = new RingBuffer<AckSystem.ReliableReceived>(config.SequenceSize, null);
            }

            return new AckSystem(connection, config, MAX_PACKET_SIZE, time ?? new Mirage.SocketLayer.Time(), bufferPool,
                reliablePool, ackable, receive, onInvalidPacket);
        }

        protected byte[] createRandomData(int id)
        {
            // random size messages
            var buffer = new byte[rand.Next(2, 12)];
            // fill array with random
            rand.NextBytes(buffer);

            // first bytes can be ID
            buffer[0] = (byte)id;
            return buffer;
        }

        /// <summary>
        /// more effecient that CollectionAssert
        /// </summary>
        protected static void AssertAreSameFromOffsets(byte[] expected, int expectedOffset, byte[] actual, int actualOffset, int length)
        {
            for (var i = 0; i < length; i++)
            {
                if (expected[i + expectedOffset] != actual[i + actualOffset])
                {
                    var e = expected[i + expectedOffset];
                    var a = actual[i + actualOffset];
                    Assert.Fail($"Arrays are not the same\n  expected {e}\n  actual {a}");
                }
            }
        }

        protected static void AssertAreSameFromOffsets(byte[] expected, int expectedOffset, int length, ArraySegment<byte> segment)
        {
            if (segment == default)
            {
                Assert.Fail($"Segment should not be default");
            }
            if (segment.Count != length)
            {
                Assert.Fail($"Segment did not have same length as expected\n  expected {length}\n  actual {segment.Count}");
            }

            AssertAreSameFromOffsets(expected, expectedOffset, segment.Array, segment.Offset, length);
        }
    }

    // NSubstitute doesn't work for this type because interface is internal
    internal class SubIRawConnection : IRawConnection
    {
        public List<byte[]> packets = new List<byte[]>();

        public void SendRaw(byte[] packet, int length)
        {
            var clone = new byte[length];
            Buffer.BlockCopy(packet, 0, clone, 0, length);
            packets.Add(clone);
        }
    }

    internal class AckTestInstance
    {
        public SubIRawConnection connection;
        public AckSystem ackSystem;

        /// <summary>
        /// Bytes given to ack system
        /// </summary>
        public List<byte[]> messages;

        /// <summary>Sent messages</summary>
        public byte[] message(int i) => messages[i];
        /// <summary>received packet</summary>
        public byte[] packet(int i) => connection.packets[i];
    }
}
