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
