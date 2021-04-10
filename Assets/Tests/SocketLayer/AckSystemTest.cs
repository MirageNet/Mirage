using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests
{
    /// <summary>
    /// helper methods for testing AckSystem
    /// </summary>
    public class AckSystemTestBase
    {
        System.Random rand = new System.Random();

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
            //return;
            ////int length = maxLength ?? Mathf.Min(A.Length - offsetA, B.Length - offsetB);
            //for (int i = 0; i < length; i++)
            //{
            //    int ia = i + expectedOffset;
            //    int ib = i + actualOffset;

            //    Assert.That(expected[ia], Is.EqualTo(actual[ib]),
            //        $"Arrays not the same offsets:[A:{expectedOffset}, B:{actualOffset}]\n" +
            //        $"  A[{ia}] = {expected[ia]}\n" +
            //        $"  B[{ib}] = {actual[ib]}\n");


            //}
        }
    }
    // NSubstitute doesn't work for this type because interface is internal
    class SubIRawConnection : IRawConnection
    {
        public List<byte[]> packets = new List<byte[]>();

        public void SendRaw(byte[] packet)
        {
            packets.Add(packet);
        }
    }

    /// <summary>
    /// Send it done in setup, and then tests just valid that the sent data is correct
    /// </summary>
    [Category("SocketLayer")]
    public class AckSystemTest_1stSend : AckSystemTestBase
    {
        private SubIRawConnection connection;
        private AckSystem ackSystem;

        /// <summary>
        /// Bytes given to ack system
        /// </summary>
        byte[] message;
        /// <summary>
        /// Bytes out of ack system
        /// </summary>
        byte[] packet;

        [SetUp]
        public void SetUp()
        {
            connection = new SubIRawConnection();
            ackSystem = new AckSystem(connection);

            message = createRandomData(1);
            ackSystem.Send(message);

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
        public void PacketSequenceShouldBe1()
        {
            int offset = 1;
            ushort sequance = ByteUtils.ReadUShort(packet, ref offset);
            Assert.That(sequance, Is.EqualTo(1));
        }

        [Test]
        public void PacketReceivedShouldBe0()
        {
            int offset = 3;
            ushort received = ByteUtils.ReadUShort(packet, ref offset);
            Assert.That(received, Is.EqualTo(0));
        }
        [Test]
        public void PacketMaskShouldBe0()
        {
            int offset = 5;
            ushort mask = ByteUtils.ReadUShort(packet, ref offset);
            Assert.That(mask, Is.EqualTo(0));
        }

        [Test]
        public void PacketShouldContainMessage()
        {
            const int headerSize = 9;
            AssertIsSameFromOffset(message, 0, packet, headerSize, message.Length);
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

        public byte[] message(int i) => messages[i];
        public byte[] packet(int i) => connection.packets[i];
    }

    /// <summary>
    /// Send it done in setup, and then tests just valid that the sent data is correct
    /// </summary>
    [Category("SocketLayer")]
    public class AckSystemTest_ManySends : AckSystemTestBase
    {
        const int messageCount = 5;

        AckTestInstance instance;

        [SetUp]
        public void SetUp()
        {
            instance = new AckTestInstance();
            instance.connection = new SubIRawConnection();
            instance.ackSystem = new AckSystem(instance.connection);

            // create and send n messages
            instance.messages = new List<byte[]>();
            for (int i = 0; i < messageCount; i++)
            {
                instance.messages.Add(createRandomData(i + 1));
                instance.ackSystem.Send(instance.messages[i]);
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
                Assert.That(sequance, Is.EqualTo(i + 1), "sequnce should start at 1 and increment for each message");
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
                const int headerSize = 9;
                AssertIsSameFromOffset(instance.message(i), 0, instance.packet(i), headerSize, instance.message(i).Length);
            }
        }
    }

    /// <summary>
    /// Send it done in setup, and then tests just valid that the sent data is correct
    /// </summary>
    [Category("SocketLayer")]
    public class AckSystemTest_NoDroppedSends : AckSystemTestBase
    {
        const int messageCount = 5;

        AckTestInstance instance1;
        AckTestInstance instance2;


        [SetUp]
        public void SetUp()
        {
            instance1 = new AckTestInstance();
            instance1.connection = new SubIRawConnection();
            instance1.ackSystem = new AckSystem(instance1.connection);


            instance2 = new AckTestInstance();
            instance2.connection = new SubIRawConnection();
            instance2.ackSystem = new AckSystem(instance2.connection);

            // create and send n messages
            instance1.messages = new List<byte[]>();
            instance2.messages = new List<byte[]>();
            for (int i = 0; i < messageCount; i++)
            {
                instance1.messages.Add(createRandomData(i + 1));
                instance2.messages.Add(createRandomData(i + 1));

                // send inside loop so message sending alternates between 1 and 2

                // send to conn1
                instance1.ackSystem.Send(instance1.messages[i]);
                // give to instance2 from conn1
                instance2.ackSystem.Receive(instance1.connection.packets[i]);

                // send to conn2
                instance2.ackSystem.Send(instance2.messages[i]);
                // give to instance1 from conn2
                instance1.ackSystem.Receive(instance2.connection.packets[i]);
            }

            // should have got 1 packet
            Assert.That(instance1.connection.packets.Count, Is.EqualTo(messageCount));
            Assert.That(instance2.connection.packets.Count, Is.EqualTo(messageCount));

            // should not have null data
            Assert.That(instance1.connection.packets, Does.Not.Contain(null));
            Assert.That(instance2.connection.packets, Does.Not.Contain(null));

        }


        [Test]
        public void AllPacketShouldBeNotify()
        {
            for (int i = 0; i < messageCount; i++)
            {
                int offset = 0;
                byte packetType = ByteUtils.ReadByte(instance1.packet(i), ref offset);
                Assert.That((PacketType)packetType, Is.EqualTo(PacketType.Notify));
            }

            for (int i = 0; i < messageCount; i++)
            {
                int offset = 0;
                byte packetType = ByteUtils.ReadByte(instance2.packet(i), ref offset);
                Assert.That((PacketType)packetType, Is.EqualTo(PacketType.Notify));
            }
        }

        [Test]
        public void PacketSequenceShouldIncrementPerSystem()
        {
            for (int i = 0; i < messageCount; i++)
            {
                int offset = 1;
                ushort sequance = ByteUtils.ReadUShort(instance1.packet(i), ref offset);
                Assert.That(sequance, Is.EqualTo(i + 1), "sequnce should start at 1 and increment for each message");
            }

            for (int i = 0; i < messageCount; i++)
            {
                int offset = 1;
                ushort sequance = ByteUtils.ReadUShort(instance2.packet(i), ref offset);
                Assert.That(sequance, Is.EqualTo(i + 1), "sequnce should start at 1 and increment for each message");
            }
        }

        [Test]
        public void PacketReceivedShouldTheSequenceOfReceived()
        {
            for (int i = 0; i < messageCount; i++)
            {
                int offset = 3;
                ushort received = ByteUtils.ReadUShort(instance1.packet(i), ref offset);
                Assert.That(received, Is.EqualTo(i), "Received should start at 0 and increment each time");
            }

            for (int i = 0; i < messageCount; i++)
            {
                int offset = 3;
                ushort received = ByteUtils.ReadUShort(instance2.packet(i), ref offset);
                Assert.That(received, Is.EqualTo(i + 1), "Received should start at 1 (received first message before sending) and increment each time");
            }
        }
        [Test]
        public void PacketMaskShouldBePreviousSequences()
        {
            uint[] expectedMask = new uint[6] {
                0b0,
                0b1,
                0b11,
                0b111,
                0b1111,
                0b1_1111,
            };
            uint[] mask = new uint[5];

            for (int i = 0; i < messageCount; i++)
            {
                int offset = 5;
                mask[i] = ByteUtils.ReadUInt(instance1.packet(i), ref offset);
            }
            // do 2nd loop so we can log all values to debug
            for (int i = 0; i < messageCount; i++)
            {
                Assert.That(mask[i], Is.EqualTo(expectedMask[i]), $"Received should contain previous receives\n  instance 1, index{i}\n{string.Join(",", mask.Select(x => x.ToString()))}");
            }


            // start at 1
            for (int i = 0; i < messageCount; i++)
            {
                int offset = 5;
                mask[i] = ByteUtils.ReadUInt(instance2.packet(i), ref offset);
            }
            // do 2nd loop so we can log all values to debug
            for (int i = 0; i < messageCount; i++)
            {
                Assert.That(mask[i], Is.EqualTo(expectedMask[i + 1]), $"Received should contain previous receives\n  instance 2, index{i}\n{string.Join(",", mask.Select(x => x.ToString()))}");
            }
        }

        [Test]
        public void PacketShouldContainMessage()
        {
            for (int i = 0; i < messageCount; i++)
            {
                const int headerSize = 9;
                AssertIsSameFromOffset(instance1.message(i), 0, instance1.packet(i), headerSize, instance1.message(i).Length);
            }

            for (int i = 0; i < messageCount; i++)
            {
                const int headerSize = 9;
                AssertIsSameFromOffset(instance2.message(i), 0, instance2.packet(i), headerSize, instance2.message(i).Length);
            }
        }

    }
}
