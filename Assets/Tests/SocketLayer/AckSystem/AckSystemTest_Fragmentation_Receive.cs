using NUnit.Framework;

namespace Mirage.SocketLayer.Tests.AckSystemTests
{
    /// <summary>
    /// Send is done in setup, and then tests just valid that the sent data is correct
    /// </summary>
    [Category("SocketLayer")]
    public class AckSystemTest_Fragmentation_Receive : AckSystemTestBase
    {
        private AckSystem ackSystem;
        private Config config;
        private byte[] message;
        private byte[] packet1;
        private byte[] packet2;

        [SetUp]
        public void SetUp()
        {
            config = new Config();
            int mtu = config.MaxPacketSize;
            int bigSize = (int)(mtu * 1.5f);

            message = CreateBigData(1, bigSize);

            var sender = new AckTestInstance();
            sender.connection = new SubIRawConnection();
            sender.ackSystem = new AckSystem(sender.connection, config, new Time(), bufferPool);
            sender.ackSystem.SendReliable(message);
            packet1 = sender.packet(0);
            packet2 = sender.packet(1);


            var connection = new SubIRawConnection();
            ackSystem = new AckSystem(connection, config, new Time(), bufferPool);
        }

        byte[] CreateBigData(int id, int size)
        {
            byte[] buffer = new byte[size];
            rand.NextBytes(buffer);
            buffer[0] = (byte)id;

            return buffer;
        }


        [Test]
        [TestCase(-2, ExpectedResult = false)]
        [TestCase(-1, ExpectedResult = false)]
        [TestCase(0, ExpectedResult = true, Description = "equal to max is invalid")]
        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = true)]
        [TestCase(5, ExpectedResult = true)]
        public bool ShouldBeInvalidIfFragmentIsOverMax(int differenceToMax)
        {
            int max = config.MaxReliableFragments;
            byte[] badPacket = new byte[AckSystem.MIN_RELIABLE_FRAGMENT_HEADER_SIZE];
            int offset = 0;
            // write as if it is normal packet
            ByteUtils.WriteByte(badPacket, ref offset, 0);
            ByteUtils.WriteUShort(badPacket, ref offset, 0);
            ByteUtils.WriteUShort(badPacket, ref offset, 0);
            ByteUtils.WriteULong(badPacket, ref offset, 0);
            ByteUtils.WriteUShort(badPacket, ref offset, 0);
            // write bad index (over max)
            int fragment = max + differenceToMax;
            ByteUtils.WriteByte(badPacket, ref offset, (byte)fragment);

            return ackSystem.InvalidFragment(badPacket);
        }


        [Test]
        public void MessageShouldBeInQueueAfterReceive()
        {
            ackSystem.ReceiveReliable(packet1, packet1.Length, true);

            Assert.IsFalse(ackSystem.NextReliablePacket(out AckSystem.ReliableReceived _));

            ackSystem.ReceiveReliable(packet2, packet2.Length, true);

            int bytesIn1 = config.MaxPacketSize - AckSystem.MIN_RELIABLE_FRAGMENT_HEADER_SIZE;
            int bytesIn2 = message.Length - bytesIn1;

            Assert.IsTrue(ackSystem.NextReliablePacket(out AckSystem.ReliableReceived first));

            Assert.IsTrue(first.isFragment);
            Assert.That(first.buffer.array[0], Is.EqualTo(1), "First fragment should have index 1");
            Assert.That(first.length, Is.EqualTo(bytesIn1 + 1));
            AssertAreSameFromOffsets(message, 0, first.buffer.array, 1, bytesIn1);

            AckSystem.ReliableReceived second = ackSystem.GetNextFragment();
            Assert.IsTrue(second.isFragment);
            Assert.That(second.buffer.array[0], Is.EqualTo(0), "Second fragment should have index 0");
            Assert.That(second.length, Is.EqualTo(bytesIn2 + 1));
            AssertAreSameFromOffsets(message, bytesIn1, second.buffer.array, 1, bytesIn2);
        }
    }
}
