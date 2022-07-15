using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests.AckSystemTests
{
    /// <summary>
    /// Send is done in setup, and then tests just valid that the sent data is correct
    /// </summary>
    [Category("SocketLayer")]
    public class AckSystemTest_Notify_NoDroppedSends : AckSystemTestBase
    {
        private const int messageCount = 5;
        private ushort maxSequence;
        private AckTestInstance instance1;
        private AckTestInstance instance2;
        private List<ArraySegment<byte>> received1;
        private List<ArraySegment<byte>> received2;

        [SetUp]
        public void SetUp()
        {
            var config = new Config();
            maxSequence = (ushort)((1 << config.SequenceSize) - 1);

            instance1 = new AckTestInstance();
            instance1.connection = new SubIRawConnection();
            instance1.ackSystem = new AckSystem(instance1.connection, config, MAX_PACKET_SIZE, new Time(), bufferPool);
            received1 = new List<ArraySegment<byte>>();


            instance2 = new AckTestInstance();
            instance2.connection = new SubIRawConnection();
            instance2.ackSystem = new AckSystem(instance2.connection, config, MAX_PACKET_SIZE, new Time(), bufferPool);
            received2 = new List<ArraySegment<byte>>();

            // create and send n messages
            instance1.messages = new List<byte[]>();
            instance2.messages = new List<byte[]>();
            for (var i = 0; i < messageCount; i++)
            {
                instance1.messages.Add(createRandomData(i + 1));
                instance2.messages.Add(createRandomData(i + 1));

                // send inside loop so message sending alternates between 1 and 2

                // send to conn1
                instance1.ackSystem.SendNotify(instance1.messages[i]);
                // give to instance2 from conn1
                var segment2 = instance2.ackSystem.ReceiveNotify(instance1.connection.packets[i], instance1.connection.packets[i].Length);
                received2.Add(segment2);

                // send to conn2
                instance2.ackSystem.SendNotify(instance2.messages[i]);
                // give to instance1 from conn2
                var segment1 = instance1.ackSystem.ReceiveNotify(instance2.connection.packets[i], instance2.connection.packets[i].Length);
                received1.Add(segment1);
            }

            // should have got 1 packet
            Assert.That(instance1.connection.packets.Count, Is.EqualTo(messageCount));
            Assert.That(instance2.connection.packets.Count, Is.EqualTo(messageCount));

            // should not have null data
            Assert.That(instance1.connection.packets, Does.Not.Contain(null));
            Assert.That(instance2.connection.packets, Does.Not.Contain(null));
        }


        [Test]
        public void AllPacketsShouldBeNotify()
        {
            for (var i = 0; i < messageCount; i++)
            {
                var offset = 0;
                var packetType = ByteUtils.ReadByte(instance1.packet(i), ref offset);
                Assert.That((PacketType)packetType, Is.EqualTo(PacketType.Notify));
            }

            for (var i = 0; i < messageCount; i++)
            {
                var offset = 0;
                var packetType = ByteUtils.ReadByte(instance2.packet(i), ref offset);
                Assert.That((PacketType)packetType, Is.EqualTo(PacketType.Notify));
            }
        }

        [Test]
        public void SequenceShouldIncrementPerSystem()
        {
            for (var i = 0; i < messageCount; i++)
            {
                var offset = 1;
                var sequance = ByteUtils.ReadUShort(instance1.packet(i), ref offset);
                Assert.That(sequance, Is.EqualTo(i), "sequnce should start at 1 and increment for each message");
            }

            for (var i = 0; i < messageCount; i++)
            {
                var offset = 1;
                var sequance = ByteUtils.ReadUShort(instance2.packet(i), ref offset);
                Assert.That(sequance, Is.EqualTo(i), "sequnce should start at 1 and increment for each message");
            }
        }

        [Test]
        public void ReceivedShouldBeEqualToLatest()
        {
            for (var i = 0; i < messageCount; i++)
            {
                var offset = 3;
                var received = ByteUtils.ReadUShort(instance1.packet(i), ref offset);
                var expected = i - 1;
                if (expected == -1) expected = maxSequence;
                Assert.That(received, Is.EqualTo(expected), "Received should start at 0 and increment each time");
            }

            for (var i = 0; i < messageCount; i++)
            {
                var offset = 3;
                var received = ByteUtils.ReadUShort(instance2.packet(i), ref offset);
                Assert.That(received, Is.EqualTo(i), "Received should start at 1 (received first message before sending) and increment each time");
            }
        }
        [Test]
        public void MaskShouldBePreviousSequences()
        {
            var expectedMask = new uint[6] {
                0b0,
                0b1,
                0b11,
                0b111,
                0b1111,
                0b1_1111,
            };
            var mask = new uint[5];

            for (var i = 0; i < messageCount; i++)
            {
                var offset = 5;
                mask[i] = ByteUtils.ReadUInt(instance1.packet(i), ref offset);
            }
            // do 2nd loop so we can log all values to debug
            for (var i = 0; i < messageCount; i++)
            {
                Assert.That(mask[i], Is.EqualTo(expectedMask[i]), $"Received should contain previous receives\n  instance 1, index{i}\n{string.Join(",", mask.Select(x => x.ToString()))}");
            }


            // start at 1
            for (var i = 0; i < messageCount; i++)
            {
                var offset = 5;
                mask[i] = ByteUtils.ReadUInt(instance2.packet(i), ref offset);
            }
            // do 2nd loop so we can log all values to debug
            for (var i = 0; i < messageCount; i++)
            {
                Assert.That(mask[i], Is.EqualTo(expectedMask[i + 1]), $"Received should contain previous receives\n  instance 2, index{i}\n{string.Join(",", mask.Select(x => x.ToString()))}");
            }
        }

        [Test]
        public void PacketShouldContainMessage()
        {
            for (var i = 0; i < messageCount; i++)
            {
                AssertAreSameFromOffsets(instance1.message(i), 0, instance1.packet(i), AckSystem.NOTIFY_HEADER_SIZE, instance1.message(i).Length);
            }

            for (var i = 0; i < messageCount; i++)
            {
                AssertAreSameFromOffsets(instance2.message(i), 0, instance2.packet(i), AckSystem.NOTIFY_HEADER_SIZE, instance2.message(i).Length);
            }
        }

        [Test]
        public void AllSegmentsShouldHaveBeenReturned()
        {
            for (var i = 0; i < messageCount; i++)
            {
                AssertAreSameFromOffsets(instance1.message(i), 0, instance1.message(i).Length, received2[i]);
            }

            for (var i = 0; i < messageCount; i++)
            {
                AssertAreSameFromOffsets(instance2.message(i), 0, instance2.message(i).Length, received1[i]);
            }
        }
    }
}
