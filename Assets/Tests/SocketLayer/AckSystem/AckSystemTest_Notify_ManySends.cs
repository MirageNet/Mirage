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
        const int messageCount = 5;

        AckTestInstance instance;

        [SetUp]
        public void SetUp()
        {
            instance = new AckTestInstance();
            instance.connection = new SubIRawConnection();
            instance.ackSystem = new AckSystem(instance.connection, new Config(), new Time(), bufferPool);

            // create and send n messages
            instance.messages = new List<byte[]>();
            for (int i = 0; i < messageCount; i++)
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
        public void AllPacketShouldBeNotify()
        {
            for (int i = 0; i < messageCount; i++)
            {
                int offset = 0;
                byte packetType = ByteUtils.ReadByte(instance.packet(i), ref offset);
                Assert.That((PacketType)packetType, Is.EqualTo(PacketType.Notify));
            }
        }

        [Test]
        public void PacketSequenceShouldIncrement()
        {
            for (int i = 0; i < messageCount; i++)
            {
                int offset = 1;
                ushort sequance = ByteUtils.ReadUShort(instance.packet(i), ref offset);
                Assert.That(sequance, Is.EqualTo(i), "sequnce should start at 1 and increment for each message");
            }
        }

        [Test]
        public void PacketReceivedShouldBe0()
        {
            for (int i = 0; i < messageCount; i++)
            {
                int offset = 3;
                ushort received = ByteUtils.ReadUShort(instance.packet(i), ref offset);
                Assert.That(received, Is.EqualTo(0), "Received should stay 0");
            }
        }
        [Test]
        public void PacketMaskShouldBe0()
        {
            for (int i = 0; i < messageCount; i++)
            {
                int offset = 5;
                ushort mask = ByteUtils.ReadUShort(instance.packet(i), ref offset);
                Assert.That(mask, Is.EqualTo(0), "Received should stay 0");
            }
        }

        [Test]
        public void PacketShouldContainMessage()
        {
            for (int i = 0; i < messageCount; i++)
            {
                AssertAreSameFromOffsets(instance.message(i), 0, instance.packet(i), AckSystem.HEADER_SIZE_NOTIFY, instance.message(i).Length);
            }
        }
    }
}
