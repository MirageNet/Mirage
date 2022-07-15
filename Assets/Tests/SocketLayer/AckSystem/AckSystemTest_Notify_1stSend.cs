using NUnit.Framework;

namespace Mirage.SocketLayer.Tests.AckSystemTests
{
    /// <summary>
    /// Send is done in setup, and then tests just valid that the sent data is correct
    /// </summary>
    [Category("SocketLayer")]
    public class AckSystemTest_Notify_1stSend : AckSystemTestBase
    {
        private SubIRawConnection connection;
        private AckSystem ackSystem;

        /// <summary>
        /// Bytes given to ack system
        /// </summary>
        private byte[] message;

        /// <summary>
        /// Bytes out of ack system
        /// </summary>
        private byte[] packet;
        private ushort maxSequence;

        [SetUp]
        public void SetUp()
        {
            var config = new Config();
            maxSequence = (ushort)((1 << config.SequenceSize) - 1);

            connection = new SubIRawConnection();
            ackSystem = new AckSystem(connection, config, MAX_PACKET_SIZE, new Time(), bufferPool);

            message = createRandomData(1);
            ackSystem.SendNotify(message);

            // should have got 1 packet
            Assert.That(connection.packets.Count, Is.EqualTo(1));
            packet = connection.packets[0];

            // should have sent data
            Assert.That(packet, Is.Not.Null);
        }

        [Test]
        public void PacketShouldBeNotify()
        {
            int offset = 0;
            byte packetType = ByteUtils.ReadByte(packet, ref offset);
            Assert.That((PacketType)packetType, Is.EqualTo(PacketType.Notify));
        }

        [Test]
        public void SentSequenceShouldBe1()
        {
            int offset = 1;
            ushort sequance = ByteUtils.ReadUShort(packet, ref offset);
            Assert.That(sequance, Is.EqualTo(0));
        }

        [Test]
        public void LatestReceivedShouldBeMax()
        {
            int offset = 3;
            ushort received = ByteUtils.ReadUShort(packet, ref offset);
            Assert.That(received, Is.EqualTo(maxSequence));
        }
        [Test]
        public void ReceivedMaskShouldBe0()
        {
            int offset = 5;
            ushort mask = ByteUtils.ReadUShort(packet, ref offset);
            Assert.That(mask, Is.EqualTo(0));
        }

        [Test]
        public void PacketShouldContainMessage()
        {
            AssertAreSameFromOffsets(message, 0, packet, AckSystem.NOTIFY_HEADER_SIZE, message.Length);
        }
    }
}
