using System;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests.AckSystemTests
{
    [Category("SocketLayer")]
    public class AckSystemTest_BufferSize : AckSystemTestBase
    {
        [Test]
        public void ThrowsIfTooManyMessageAreSent()
        {
            var time = new Time();
            var config = new Config
            {
                MaxReliablePacketsInSendBufferPerConnection = 50,
                SequenceSize = 8
            };
            var instance = new AckTestInstance
            {
                connection = new SubIRawConnection()
            };
            instance.ackSystem = new AckSystem(instance.connection, config, MAX_PACKET_SIZE, time, bufferPool);

            for (var i = 0; i < 50; i++)
            {
                instance.ackSystem.SendReliable(createRandomData(i));
                // update to send batch
                instance.ackSystem.Update();
            }

            var exception = Assert.Throws<BufferFullException>(() =>
            {
                instance.ackSystem.SendReliable(createRandomData(51));
                instance.ackSystem.Update();
            });
            var expected = new InvalidOperationException($"Max packets in send buffer reached for {instance.connection}");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        public void ThrowIfRingBufferIsfull()
        {
            var time = new Time();
            var config = new Config
            {
                MaxReliablePacketsInSendBufferPerConnection = 500,
                SequenceSize = 8
            };
            var instance = new AckTestInstance
            {
                connection = new SubIRawConnection()
            };
            instance.ackSystem = new AckSystem(instance.connection, config, MAX_PACKET_SIZE, time, bufferPool);

            for (var i = 0; i < 255; i++)
            {
                instance.ackSystem.SendReliable(createRandomData(i));
                // update to send batch
                instance.ackSystem.Update();
            }

            var exception = Assert.Throws<BufferFullException>(() =>
            {
                instance.ackSystem.SendReliable(createRandomData(0));
                instance.ackSystem.Update();
            });
            var expected = new BufferFullException($"Sent queue is full for {instance.connection}");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
    }
}
