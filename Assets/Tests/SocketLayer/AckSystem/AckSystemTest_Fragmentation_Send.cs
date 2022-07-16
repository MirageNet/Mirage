using System.Collections.Generic;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests.AckSystemTests
{
    /// <summary>
    /// Send is done in setup, and then tests just valid that the sent data is correct
    /// </summary>
    [Category("SocketLayer")]
    public class AckSystemTest_Fragmentation_Send : AckSystemTestBase
    {
        private AckTestInstance instance;

        [SetUp]
        public void SetUp()
        {
            var config = new Config();
            int mtu = MAX_PACKET_SIZE;
            int bigSize = (int)(mtu * 1.5f);

            byte[] message = CreateBigData(1, bigSize);

            instance = new AckTestInstance();
            instance.connection = new SubIRawConnection();
            instance.ackSystem = new AckSystem(instance.connection, config, MAX_PACKET_SIZE, new Time(), bufferPool);

            // create and send n messages
            instance.messages = new List<byte[]>();
            instance.messages.Add(message);
            instance.ackSystem.SendReliable(message);

            // should not have null data
            Assert.That(instance.connection.packets, Does.Not.Contain(null));
        }

        private byte[] CreateBigData(int id, int size)
        {
            byte[] buffer = new byte[size];
            rand.NextBytes(buffer);
            buffer[0] = (byte)id;

            return buffer;
        }

        [Test]
        public void ShouldHaveSent2Packets()
        {
            Assert.That(instance.connection.packets.Count, Is.EqualTo(2));
        }

        [Test]
        public void MessageShouldBeReliableFragment()
        {
            foreach (byte[] packet in instance.connection.packets)
            {
                int offset = 0;
                byte packetType = ByteUtils.ReadByte(packet, ref offset);
                Assert.That((PacketType)packetType, Is.EqualTo(PacketType.ReliableFragment));
            }
        }

        [Test]
        public void EachPacketHasDifferentAckSequence()
        {
            for (int i = 0; i < instance.connection.packets.Count; i++)
            {
                int offset = 1;
                ushort sequance = ByteUtils.ReadUShort(instance.packet(i), ref offset);
                Assert.That(sequance, Is.EqualTo(i));
            }
        }

        [Test]
        public void EachPacketHasDifferentReliableOrder()
        {
            for (int i = 0; i < instance.connection.packets.Count; i++)
            {
                int offset = 1 + 2 + 2 + 8;
                ushort reliableOrder = ByteUtils.ReadUShort(instance.packet(i), ref offset);

                Assert.That(reliableOrder, Is.EqualTo(i));
            }
        }

        [Test]
        public void EachPacketHasDifferentFragmentIndex()
        {
            for (int i = 0; i < instance.connection.packets.Count; i++)
            {
                int offset = 1 + 2 + 2 + 8 + 2;
                ushort fragmentIndex = ByteUtils.ReadByte(instance.packet(i), ref offset);
                Assert.That(fragmentIndex, Is.EqualTo(1 - i), "Should be reverse Index, first packet should have 1 and second should have 0");
            }
        }
    }
}
