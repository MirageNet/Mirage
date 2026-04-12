using NUnit.Framework;
using System;

namespace Mirage.SocketLayer.Tests.AckSystemTests
{
    [Category("SocketLayer")]
    public class AckSystemTest_Security : AckSystemTestBase
    {
        private class Time : ITime
        {
            public double Now { get; set; }
        }

        private AckSystem ackSystem;
        private int invokedCount;

        [SetUp]
        public void SetUp()
        {
            invokedCount = 0;
            var config = new Config();
            var connection = new SubIRawConnection();
            // Using a local Time class just like other tests do
            ackSystem = new AckSystem(connection, config, MAX_PACKET_SIZE, new Time(), bufferPool, onInvalidPacket: () => invokedCount++);
        }

        [Test]
        public void ReceiveNotify_ShouldInvokeCallback_IfSequenceIsTooHigh()
        {
            var config = new Config();
            var capacity = 1 << config.SequenceSize;
            
            // +1 for PacketType byte which is skipped
            var packet = new byte[AckSystem.NOTIFY_HEADER_SIZE + 1];
            var offset = 1;
            ByteUtils.WriteUShort(packet, ref offset, (ushort)capacity); // Invalid sequence
            ByteUtils.WriteUShort(packet, ref offset, 0);
            ByteUtils.WriteULong(packet, ref offset, 0);

            ackSystem.ReceiveNotify(packet.AsSpan());

            Assert.That(invokedCount, Is.EqualTo(1));
        }

        [Test]
        public void ReceiveNotify_ShouldInvokeCallback_IfAckSequenceIsTooHigh()
        {
            var config = new Config();
            var capacity = 1 << config.SequenceSize;
            
            var packet = new byte[AckSystem.NOTIFY_HEADER_SIZE + 1];
            var offset = 1;
            ByteUtils.WriteUShort(packet, ref offset, 0);
            ByteUtils.WriteUShort(packet, ref offset, (ushort)capacity); // Invalid ackSequence
            ByteUtils.WriteULong(packet, ref offset, 0);

            ackSystem.ReceiveNotify(packet.AsSpan());

            Assert.That(invokedCount, Is.EqualTo(1));
        }

        [Test]
        public void ReceiveReliable_ShouldInvokeCallback_IfSequenceIsTooHigh()
        {
            var config = new Config();
            var capacity = 1 << config.SequenceSize;
            
            var packet = new byte[AckSystem.RELIABLE_HEADER_SIZE + 1];
            var offset = 1;
            ByteUtils.WriteUShort(packet, ref offset, (ushort)capacity); // Invalid sequence
            ByteUtils.WriteUShort(packet, ref offset, 0);
            ByteUtils.WriteULong(packet, ref offset, 0);
            ByteUtils.WriteUShort(packet, ref offset, 0);

            ackSystem.ReceiveReliable(packet.AsSpan(), false);

            Assert.That(invokedCount, Is.EqualTo(1));
        }

        [Test]
        public void ReceiveReliable_ShouldInvokeCallback_IfReliableSequenceIsTooHigh()
        {
            var config = new Config();
            var capacity = 1 << config.SequenceSize;
            
            var packet = new byte[AckSystem.RELIABLE_HEADER_SIZE + 1];
            var offset = 1;
            ByteUtils.WriteUShort(packet, ref offset, 0);
            ByteUtils.WriteUShort(packet, ref offset, 0);
            ByteUtils.WriteULong(packet, ref offset, 0);
            ByteUtils.WriteUShort(packet, ref offset, (ushort)capacity); // Invalid reliableSequence

            ackSystem.ReceiveReliable(packet.AsSpan(), false);

            Assert.That(invokedCount, Is.EqualTo(1));
        }

        [Test]
        public void ReceiveAck_ShouldInvokeCallback_IfAckSequenceIsTooHigh()
        {
            var config = new Config();
            var capacity = 1 << config.SequenceSize;
            
            var packet = new byte[AckSystem.ACK_HEADER_SIZE + 1];
            var offset = 1;
            ByteUtils.WriteUShort(packet, ref offset, (ushort)capacity); // Invalid ackSequence
            ByteUtils.WriteULong(packet, ref offset, 0);

            ackSystem.ReceiveAck(packet.AsSpan());

            Assert.That(invokedCount, Is.EqualTo(1));
        }
    }
}
