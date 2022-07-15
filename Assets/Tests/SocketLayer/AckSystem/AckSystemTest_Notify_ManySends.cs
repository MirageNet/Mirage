using System.Collections.Generic;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests.AckSystemTests
{
    /// <summary>
    /// Send is done in setup, and then tests just valid that the sent data is correct
    /// </summary>
    [Category("SocketLayer")]
    public class AckSystemTest_Notify_ManySends : AckSystemTestBase
    {
        private const int messageCount = 5;
        private AckTestInstance instance;
        private ushort maxSequence;

        [SetUp]
        public void SetUp()
        {
            var config = new Config();
            maxSequence = (ushort)((1 << config.SequenceSize) - 1);

            instance = new AckTestInstance();
            instance.connection = new SubIRawConnection();
            instance.ackSystem = new AckSystem(instance.connection, new Config(), MAX_PACKET_SIZE, new Time(), bufferPool);

            // create and send n messages
            instance.messages = new List<byte[]>();
            for (var i = 0; i < messageCount; i++)
            {
                instance.messages.Add(createRandomData(i + 1));
                instance.ackSystem.SendNotify(instance.messages[i]);
            }


            // should have got 1 packet
            Assert.That(instance.connection.packets.Count, Is.EqualTo(messageCount));

            // should not have null data
            Assert.That(instance.connection.packets, Does.Not.Contain(null));
        }

        [Test]
        public void PacketsShouldBeNotify()
        {
            for (var i = 0; i < messageCount; i++)
            {
                var offset = 0;
                var packetType = ByteUtils.ReadByte(instance.packet(i), ref offset);
                Assert.That((PacketType)packetType, Is.EqualTo(PacketType.Notify));
            }
        }

        [Test]
        public void PacketSequenceShouldIncrement()
        {
            for (var i = 0; i < messageCount; i++)
            {
                var offset = 1;
                var sequance = ByteUtils.ReadUShort(instance.packet(i), ref offset);
                Assert.That(sequance, Is.EqualTo(i), "sequnce should start at 1 and increment for each message");
            }
        }

        [Test]
        public void PacketReceivedShouldBeMax()
        {
            for (var i = 0; i < messageCount; i++)
            {
                var offset = 3;
                var received = ByteUtils.ReadUShort(instance.packet(i), ref offset);
                Assert.That(received, Is.EqualTo(maxSequence), $"Received should stay max, index:{i}");
            }
        }
        [Test]
        public void PacketMaskShouldBe0()
        {
            for (var i = 0; i < messageCount; i++)
            {
                var offset = 5;
                var mask = ByteUtils.ReadUShort(instance.packet(i), ref offset);
                Assert.That(mask, Is.EqualTo(0), "Received should stay 0");
            }
        }

        [Test]
        public void PacketShouldContainMessage()
        {
            for (var i = 0; i < messageCount; i++)
            {
                AssertAreSameFromOffsets(instance.message(i), 0, instance.packet(i), AckSystem.NOTIFY_HEADER_SIZE, instance.message(i).Length);
            }
        }
    }
}
