using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.SocketLayer.Tests.AckSystemTests
{
    /// <summary>
    /// Send is done in setup, and then tests just valid that the sent data is correct
    /// </summary>
    [Category("SocketLayer")]
    public class AckSystemTest_Notify_NoDroppedSends : AckSystemTestBase
    {
        const int messageCount = 5;

        AckTestInstance instance1;
        AckTestInstance instance2;


        [SetUp]
        public void SetUp()
        {
            instance1 = new AckTestInstance();
            instance1.connection = new SubIRawConnection();
            instance1.ackSystem = new AckSystem(instance1.connection, new Config(), new Time(), bufferPool);


            instance2 = new AckTestInstance();
            instance2.connection = new SubIRawConnection();
            instance2.ackSystem = new AckSystem(instance2.connection, new Config(), new Time(), bufferPool);

            // create and send n messages
            instance1.messages = new List<byte[]>();
            instance2.messages = new List<byte[]>();
            for (int i = 0; i < messageCount; i++)
            {
                instance1.messages.Add(createRandomData(i + 1));
                instance2.messages.Add(createRandomData(i + 1));

                // send inside loop so message sending alternates between 1 and 2

                // send to conn1
                instance1.ackSystem.SendNotify(instance1.messages[i]);
                // give to instance2 from conn1
                instance2.ackSystem.ReceiveNotify(instance1.connection.packets[i], instance1.connection.packets[i].Length);

                // send to conn2
                instance2.ackSystem.SendNotify(instance2.messages[i]);
                // give to instance1 from conn2
                instance1.ackSystem.ReceiveNotify(instance2.connection.packets[i], instance2.connection.packets[i].Length);
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
        public void SequenceShouldIncrementPerSystem()
        {
            for (int i = 0; i < messageCount; i++)
            {
                int offset = 1;
                ushort sequance = ByteUtils.ReadUShort(instance1.packet(i), ref offset);
                Assert.That(sequance, Is.EqualTo(i), "sequnce should start at 1 and increment for each message");
            }

            for (int i = 0; i < messageCount; i++)
            {
                int offset = 1;
                ushort sequance = ByteUtils.ReadUShort(instance2.packet(i), ref offset);
                Assert.That(sequance, Is.EqualTo(i), "sequnce should start at 1 and increment for each message");
            }
        }

        [Test]
        public void ReceivedShouldBeEqualToLatest()
        {
            for (int i = 0; i < messageCount; i++)
            {
                int offset = 3;
                ushort received = ByteUtils.ReadUShort(instance1.packet(i), ref offset);
                Assert.That(received, Is.EqualTo(Mathf.Clamp(i - 1, 0, messageCount)), "Received should start at 0 and increment each time");
            }

            for (int i = 0; i < messageCount; i++)
            {
                int offset = 3;
                ushort received = ByteUtils.ReadUShort(instance2.packet(i), ref offset);
                Assert.That(received, Is.EqualTo(i), "Received should start at 1 (received first message before sending) and increment each time");
            }
        }
        [Test]
        public void MaskShouldBePreviousSequences()
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
                AssertAreSameFromOffsets(instance1.message(i), 0, instance1.packet(i), AckSystem.HEADER_SIZE_NOTIFY, instance1.message(i).Length);
            }

            for (int i = 0; i < messageCount; i++)
            {
                AssertAreSameFromOffsets(instance2.message(i), 0, instance2.packet(i), AckSystem.HEADER_SIZE_NOTIFY, instance2.message(i).Length);
            }
        }
    }
}
