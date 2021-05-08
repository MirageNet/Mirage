using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.SocketLayer.Tests.AckSystemTests
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
            byte[] buffer = new byte[rand.Next(2, 12)];
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
        }
    }

    // NSubstitute doesn't work for this type because interface is internal
    class SubIRawConnection : IRawConnection
    {
        public List<byte[]> packets = new List<byte[]>();

        public void SendRaw(byte[] packet, int length)
        {
            packets.Add(packet);
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

        /// <summary>Sent messages</summary>
        public byte[] message(int i) => messages[i];
        /// <summary>received packet</summary>
        public byte[] packet(int i) => connection.packets[i];
    }

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
        byte[] message;
        /// <summary>
        /// Bytes out of ack system
        /// </summary>
        byte[] packet;

        [SetUp]
        public void SetUp()
        {
            connection = new SubIRawConnection();
            ackSystem = new AckSystem(connection, default, new Time());

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
            instance.ackSystem = new AckSystem(instance.connection, default, new Time());

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
            instance1.ackSystem = new AckSystem(instance1.connection, default, new Time());


            instance2 = new AckTestInstance();
            instance2.connection = new SubIRawConnection();
            instance2.ackSystem = new AckSystem(instance2.connection, default, new Time());

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
                instance2.ackSystem.ReceiveNotify(instance1.connection.packets[i]);

                // send to conn2
                instance2.ackSystem.SendNotify(instance2.messages[i]);
                // give to instance1 from conn2
                instance1.ackSystem.ReceiveNotify(instance2.connection.packets[i]);
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
        public void ReceivedShouldBeEqualToLatest()
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

    /// <summary>
    /// Send is done in setup, and then tests just valid that the sent data is correct
    /// </summary>
    [Category("SocketLayer")]
    public class AckSystemTest_Notify_DroppedSends : AckSystemTestBase
    {
        const int messageCount = 5;

        AckTestInstance instance1;
        AckTestInstance instance2;

        // what message get received each instance
        bool[] received1 = new bool[messageCount] {
            true,
            false,
            false,
            true,
            true,
        };
        bool[] received2 = new bool[messageCount] {
            false,
            true,
            true,
            false,
            true,
        };

        [SetUp]
        public void SetUp()
        {
            instance1 = new AckTestInstance();
            instance1.connection = new SubIRawConnection();
            instance1.ackSystem = new AckSystem(instance1.connection, default, new Time());


            instance2 = new AckTestInstance();
            instance2.connection = new SubIRawConnection();
            instance2.ackSystem = new AckSystem(instance2.connection, default, new Time());

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

                // give to instance2 if received
                if (received2[i])
                    instance2.ackSystem.ReceiveNotify(instance1.connection.packets[i]);

                // send to conn2
                instance2.ackSystem.SendNotify(instance2.messages[i]);
                // give to instance1 if received
                if (received1[i])
                    instance1.ackSystem.ReceiveNotify(instance2.connection.packets[i]);
            }

            // should have got 1 packet
            Assert.That(instance1.connection.packets.Count, Is.EqualTo(messageCount));
            Assert.That(instance2.connection.packets.Count, Is.EqualTo(messageCount));

            // should not have null data
            Assert.That(instance1.connection.packets, Does.Not.Contain(null));
            Assert.That(instance2.connection.packets, Does.Not.Contain(null));
        }


        [Test]
        public void ReceivedShouldBeEqualToLatest()
        {
            ushort nextReceive = 0;
            for (int i = 0; i < messageCount; i++)
            {
                int offset = 3;
                ushort received = ByteUtils.ReadUShort(instance1.packet(i), ref offset);
                Assert.That(received, Is.EqualTo(nextReceive), "Received should start at 0 and increment each time");

                // do at end becuase 1 is sending first
                if (received1[i])
                    nextReceive = (ushort)(i + 1);
            }

            nextReceive = 0;
            for (int i = 0; i < messageCount; i++)
            {
                // do at start becuase 2 is sending second
                if (received2[i])
                    nextReceive = (ushort)(i + 1);

                int offset = 3;
                ushort received = ByteUtils.ReadUShort(instance2.packet(i), ref offset);
                Assert.That(received, Is.EqualTo(nextReceive), "Received should start at 1 (received first message before sending) and increment each time");
            }
        }

        [Test]
        public void MaskShouldBePreviousSequences()
        {
            uint[] expectedMask1 = new uint[5] {
                0b0,    // no received
                0b1,    // i=0 received
                0b1,    // still just i=0
                0b1,    // still just i=0
                0b1001, // received i=3
            };
            uint[] expectedMask2 = new uint[5] {
                0b0,    // i=0 not received
                0b1,    // i=1 received
                0b11,   // i=2 received
                0b11,   // still just i=2
                0b1101, // received i=4
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
                Assert.That(mask[i], Is.EqualTo(expectedMask1[i]), $"Received should contain previous receives\n  instance 1, index{i}\n{string.Join(",", mask.Select(x => x.ToString()))}");
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
                Assert.That(mask[i], Is.EqualTo(expectedMask2[i]), $"Received should contain previous receives\n  instance 2, index{i}\n{string.Join(",", mask.Select(x => x.ToString()))}");
            }
        }
    }

    /// <summary>
    /// Send is done in setup, and then tests just valid that the sent data is correct
    /// </summary>
    [Category("SocketLayer")]
    public class AckSystemTest_Reliable_HighDroppedSends : AckSystemTestBase
    {
        class BadSocket
        {
            readonly AckSystem ackSystem1;
            readonly AckSystem ackSystem2;

            readonly SubIRawConnection connection1;
            readonly SubIRawConnection connection2;

            int processed1 = 0;
            int processed2 = 0;


            public BadSocket(AckTestInstance instance1, AckTestInstance instance2)
            {
                ackSystem1 = instance1.ackSystem;
                ackSystem2 = instance2.ackSystem;
                connection1 = instance1.connection;
                connection2 = instance2.connection;
            }

            /// <summary>
            /// Passes message from connection 1 to acksystem 2
            /// </summary>
            /// <param name="dropChance"></param>
            public void Update(float dropChance)
            {
                Update(ref processed1, connection1, ackSystem2, dropChance);
                Update(ref processed2, connection2, ackSystem1, dropChance);
            }
            static void Update(ref int processed, SubIRawConnection connection, AckSystem ackSystem, float dropChance)
            {
                int count1 = connection.packets.Count;
                for (int i = processed; i < count1; i++)
                {
                    byte[] packet = connection.packets[i];
                    if (Random.value > dropChance)
                    {
                        var type = (PacketType)packet[0];
                        switch (type)
                        {
                            case PacketType.Reliable:
                                ackSystem.ReceiveReliable(packet);
                                break;
                            case PacketType.Ack:
                                ackSystem.ReceiveAck(packet);
                                break;
                            case PacketType.Command:
                            case PacketType.Unreliable:
                            case PacketType.Notify:
                            case PacketType.KeepAlive:
                            default:
                                break;
                        }
                    }
                }
                processed = count1;
            }
        }

        class Time : ITime
        {
            public float Now { get; set; }
        }

        const int messageCount = 100;
        const float dropChance = 0.2f;
        const float tick = 0.02f;
        const float timeout = 0.2f;

        BadSocket badSocket;
        Time time;
        AckTestInstance instance1;
        AckTestInstance instance2;


        [SetUp]
        public void SetUp()
        {
            time = new Time();

            instance1 = new AckTestInstance();
            instance1.connection = new SubIRawConnection();
            instance1.ackSystem = new AckSystem(instance1.connection, timeout, time);


            instance2 = new AckTestInstance();
            instance2.connection = new SubIRawConnection();
            instance2.ackSystem = new AckSystem(instance2.connection, timeout, time);

            badSocket = new BadSocket(instance1, instance2);

            // create and send n messages
            instance1.messages = new List<byte[]>();
            instance2.messages = new List<byte[]>();
            for (int i = 0; i < messageCount; i++)
            {
                instance1.messages.Add(createRandomData(i + 1));
                instance2.messages.Add(createRandomData(i + 1));

                // send inside loop so message sending alternates between 1 and 2

                // send to conn1
                instance1.ackSystem.SendReliable(instance1.messages[i]);

                //// send to conn2
                instance2.ackSystem.SendReliable(instance2.messages[i]);


                // fake Update
                time.Now += tick;
                instance1.ackSystem.Update();
                instance2.ackSystem.Update();
                badSocket.Update(dropChance);
            }

            // run for enough updates that all message should be received
            for (float t = 0; t < timeout * 1.2f; t += tick)
            {
                // fake Update
                time.Now += tick;
                instance1.ackSystem.Update();
                instance2.ackSystem.Update();
                // no dropchance now to make sure all reach other side
                badSocket.Update(0);
            }

            // should not have null data
            Assert.That(instance1.connection.packets, Does.Not.Contain(null));
            Assert.That(instance2.connection.packets, Does.Not.Contain(null));
        }


        [Test]
        public void AllMessagesShouldHaveBeenReceivedInOrder()
        {
            //for (int i = 0; i < messageCount; i++)
            //{
            //    bool hasNext = instance1.ackSystem.NextReliablePacket(out byte[] packet);
            //    Assert.IsTrue(hasNext);

            //    byte[] expected = instance2.message(i);
            //    AssertIsSameFromOffset(expected, 0, packet, 0, expected.Length);
            //}

            for (int i = 0; i < messageCount; i++)
            {
                bool hasNext = instance2.ackSystem.NextReliablePacket(out byte[] packet);
                Assert.IsTrue(hasNext, $"Missing {i}");

                byte[] expected = instance1.message(i);
                AssertIsSameFromOffset(expected, 0, packet, 0, expected.Length);
            }
        }
    }
}
