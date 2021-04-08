using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests
{
    public class AckSystemTestBase
    {
        System.Random rand = new System.Random();
        protected Queue<byte[]> toSend;

        protected void GenerateToSend(int count)
        {
            toSend = new Queue<byte[]>();
            for (int i = 0; i < count; i++)
            {
                toSend.Enqueue(createRandomData(i));
            }
        }

        protected byte[] createRandomData(int id)
        {
            // random size messages
            byte[] buffer = new byte[rand.Next(10, 1000)];
            // fill array with random
            rand.NextBytes(buffer);

            // first bytes can be ID
            buffer[0] = (byte)id;
            return buffer;
        }

        protected void AssertIsSameFromOffset(byte[] expected, int expectedOffset, byte[] actual, int actualOffset, int length)
        {
            CollectionAssert.AreEqual(
                expected.Skip(expectedOffset).Take(length),
                actual.Skip(actualOffset).Take(length)
                );

            //todo remove this
            return;
            //int length = maxLength ?? Mathf.Min(A.Length - offsetA, B.Length - offsetB);
            for (int i = 0; i < length; i++)
            {
                int ia = i + expectedOffset;
                int ib = i + actualOffset;

                Assert.That(expected[ia], Is.EqualTo(actual[ib]),
                    $"Arrays not the same offsets:[A:{expectedOffset}, B:{actualOffset}]\n" +
                    $"  A[{ia}] = {expected[ia]}\n" +
                    $"  B[{ib}] = {actual[ib]}\n");


            }
        }
    }
    // NSubstitute doesn't work for this type because interface is internal
    class SubIRawConnection : IRawConnection
    {
        public List<byte[]> packets = new List<byte[]>();
        public void SendRaw(byte[] data)
        {
            packets.Add(data);
        }
    }

    /// <summary>
    /// Send it done in setup, and then tests just valid that the sent data is correct
    /// </summary>
    [Category("SocketLayer")]
    public class AckSystemTest_FirstSend : AckSystemTestBase
    {
        private SubIRawConnection connection;
        private AckSystem ackSystem;

        /// <summary>
        /// Bytes given to ack system
        /// </summary>
        byte[] msg;
        /// <summary>
        /// Bytes out of ack system
        /// </summary>
        byte[] packet;

        [SetUp]
        public void SetUp()
        {
            connection = new SubIRawConnection();
            ackSystem = new AckSystem(connection);

            msg = createRandomData(1);
            ackSystem.Send(msg);

            // should have got 1 packet
            Assert.That(connection.packets.Count, Is.EqualTo(1));
            packet = connection.packets[0];

            // should have sent data
            Assert.That(packet, Is.Not.Null);
        }

        [Test]
        public void PacketShouldBeNotify()
        {
            Assert.That((PacketType)packet[0], Is.EqualTo(PacketType.Notify));
        }

        [Test]
        public void PacketSequenceShouldBe0()
        {
            int sequance = packet[1] | (packet[2] << 8);
            Assert.That(sequance, Is.EqualTo(0));
        }

        [Test]
        public void PacketRecievedShouldBe0()
        {
            throw new NotImplementedException();
            int sequance = packet[1] | (packet[2] << 8);
            Assert.That(sequance, Is.EqualTo(0));
        }
        [Test]
        public void PacketMaskShouldBe0()
        {
            throw new NotImplementedException();
            int sequance = packet[1] | (packet[2] << 8);
            Assert.That(sequance, Is.EqualTo(0));
        }

        [Test]
        public void PacketShouldContainMessage()
        {
            const int headerSize = 9;
            AssertIsSameFromOffset(msg, 0, packet, headerSize, msg.Length);
        }
    }
}
