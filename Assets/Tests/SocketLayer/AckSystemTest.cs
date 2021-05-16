using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.SocketLayer.Tests.AckSystemTests
{
    /// <summary>
    /// helper methods for testing AckSystem
    /// </summary>
    public class AckSystemTestBase
    {
        System.Random rand = new System.Random();
        protected BufferPool bufferPool = new BufferPool(1300, 100, 1000);

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

        /// <summary>
        /// more effecient that CollectionAssert
        /// </summary>
        protected static void AssertAreSameFromOffsets(byte[] expected, int expectedOffset, byte[] actual, int actualOffset, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (expected[i + expectedOffset] != actual[i + actualOffset])
                {
                    byte e = expected[i + expectedOffset];
                    byte a = actual[i + actualOffset];
                    Assert.Fail($"Arrays are not the same\n  expected {e}\n  actual {a}");
                }
            }
        }
    }

    // NSubstitute doesn't work for this type because interface is internal
    class SubIRawConnection : IRawConnection
    {
        public List<byte[]> packets = new List<byte[]>();

        public void SendRaw(byte[] packet, int length)
        {
            byte[] clone = new byte[length];
            System.Buffer.BlockCopy(packet, 0, clone, 0, length);
            packets.Add(clone);
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
            ackSystem = new AckSystem(connection, new Config(), new Time(), bufferPool);

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
            Assert.That(sequance, Is.EqualTo(0));
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
            AssertAreSameFromOffsets(message, 0, packet, AckSystem.HEADER_SIZE_NOTIFY, message.Length);
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

                // give to instance2 if received
                if (received2[i])
                    instance2.ackSystem.ReceiveNotify(instance1.connection.packets[i], instance1.connection.packets[i].Length);

                // send to conn2
                instance2.ackSystem.SendNotify(instance2.messages[i]);
                // give to instance1 if received
                if (received1[i])
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
                    nextReceive = (ushort)(i);
            }

            nextReceive = 0;
            for (int i = 0; i < messageCount; i++)
            {
                // do at start becuase 2 is sending second
                if (received2[i])
                    nextReceive = (ushort)(i);

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
    public class AckSystemTest_Reliable : AckSystemTestBase
    {
        class Time : ITime
        {
            public float Now { get; set; }
        }
        class BadSocket
        {
            readonly AckSystem ackSystem1;
            readonly AckSystem ackSystem2;

            readonly SubIRawConnection connection1;
            readonly SubIRawConnection connection2;

            int processed1 = 0;
            int processed2 = 0;

            List<byte[]> ToSend1 = new List<byte[]>();
            List<byte[]> ToSend2 = new List<byte[]>();

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
            public (List<byte[]>, List<byte[]>) Update(float dropChance, float skipChance)
            {
                List<byte[]> r2 = Update(ref processed1, ToSend1, connection1, ackSystem2, dropChance, skipChance);
                List<byte[]> r1 = Update(ref processed2, ToSend2, connection2, ackSystem1, dropChance, skipChance);
                return (r1, r2);
            }
            static List<byte[]> Update(ref int processed, List<byte[]> ToSend, SubIRawConnection connection, AckSystem ackSystem, float dropChance, float skipChance)
            {
                int count1 = connection.packets.Count;
                for (int i = processed; i < count1; i++)
                {
                    byte[] packet = connection.packets[i];
                    if (Random.value > dropChance)
                    {
                        ToSend.Add(packet);
                    }
                }
                processed = count1;

                var newPackets = new List<byte[]>();
                for (int i = 0; i < ToSend.Count; i++)
                {
                    if (Random.value < skipChance) { continue; }
                    newPackets.AddRange(Receive(ackSystem, ToSend[i]));
                    ToSend.RemoveAt(i);
                    i--;
                }

                return newPackets;
            }

            private static List<byte[]> Receive(AckSystem ackSystem, byte[] packet)
            {
                var received = new List<byte[]>();
                var type = (PacketType)packet[0];
                switch (type)
                {
                    case PacketType.Reliable:
                        ackSystem.ReceiveReliable(packet, packet.Length);
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

                while (ackSystem.NextReliablePacket(out System.ArraySegment<byte> outBuffer))
                {
                    received.Add(outBuffer.ToArray());
                }
                return received;
            }
        }

        const float tick = 0.02f;

        float timeout;
        BadSocket badSocket;
        Time time;
        AckTestInstance instance1;
        AckTestInstance instance2;

        List<byte[]> receives1;
        List<byte[]> receives2;

        [SetUp]
        public void SetUp()
        {
            time = new Time();
            var config = new Config();
            timeout = config.TimeBeforeEmptyAck;

            instance1 = new AckTestInstance();
            instance1.connection = new SubIRawConnection();
            instance1.ackSystem = new AckSystem(instance1.connection, config, time, bufferPool);


            instance2 = new AckTestInstance();
            instance2.connection = new SubIRawConnection();
            instance2.ackSystem = new AckSystem(instance2.connection, config, time, bufferPool);

            badSocket = new BadSocket(instance1, instance2);

            // create and send n messages
            instance1.messages = new List<byte[]>();
            instance2.messages = new List<byte[]>();

            receives1 = new List<byte[]>();
            receives2 = new List<byte[]>();
        }

        [TearDown]
        public void TearDown()
        {
            time = null;
            badSocket = null;
            instance1 = null;
            instance2 = null;
            receives1 = null;
            receives2 = null;
            System.GC.Collect();
        }

        [UnityTest]
        [TestCase(true, 100, 0f, 0f)]
        [TestCase(true, 100, 0.2f, 0f)]
        [TestCase(true, 100, 0.2f, 0.4f)]
        [TestCase(true, 3000, 0.2f, 0f)]
        [TestCase(true, 3000, 0.2f, 0.4f)]
        [TestCase(false, 100, 0f, 0f)]
        [TestCase(false, 100, 0.2f, 0f)]
        [TestCase(false, 100, 0.2f, 0.4f)]
        [TestCase(false, 3000, 0.2f, 0f)]
        [TestCase(false, 3000, 0.2f, 0.4f)]
        [Repeat(100)]
        public void AllMessagesShouldHaveBeenReceivedInOrder(bool instance2Sends, int messageCount, float dropChance, float skipChance)
        {
            SendManyMessages(instance2Sends, messageCount, dropChance, skipChance);

            // ---- asserts ---- // 
            Assert.That(receives2, Has.Count.EqualTo(messageCount + 1));
            Assert.That(receives1, Has.Count.EqualTo(instance2Sends ? messageCount + 1 : 0));

            // check all message reached other side
            for (int i = 0; i < messageCount; i++)
            {
                byte[] message = receives2[i];

                byte[] expected = instance1.message(i);
                AssertAreSameFromOffsets(expected, 0, message, 0, expected.Length);
            }


            if (instance2Sends)
            {
                for (int i = 0; i < messageCount; i++)
                {
                    byte[] message = receives1[i];

                    byte[] expected = instance2.message(i);
                    AssertAreSameFromOffsets(expected, 0, message, 0, expected.Length);
                }
            }
        }

        void SendManyMessages(bool instance2Sends, int messageCount, float dropChance, float skipChance)
        {
            // send all messages
            for (int i = 0; i < messageCount; i++)
            {
                instance1.messages.Add(createRandomData(i + 1));
                instance2.messages.Add(createRandomData(i + 1));

                // send inside loop so message sending alternates between 1 and 2

                // send to conn1
                instance1.ackSystem.SendReliable(instance1.messages[i]);

                if (instance2Sends)
                {
                    // send to conn2
                    instance2.ackSystem.SendReliable(instance2.messages[i]);
                }

                // fake Update
                Tick(dropChance, skipChance);
            }

            // send 1 more message so that other side will for sure get last message
            // if we dont do then last message could be forgot and we receive 99/100
            instance1.ackSystem.SendReliable(new byte[1] { 0 });
            if (instance2Sends)
            {
                instance2.ackSystem.SendReliable(new byte[1] { 0 });
            }
            // run for enough updates that all message should be received
            // wait more than timeout incase 
            for (float t = 0; t < timeout * 2f; t += tick)
            {
                // fake Update
                Tick(0, 0);
            }

            // should not have null data
            Assert.That(instance1.connection.packets, Does.Not.Contain(null));
            Assert.That(instance2.connection.packets, Does.Not.Contain(null));
        }

        private void Tick(float dropChance, float skipChance)
        {
            time.Now += tick;
            instance1.ackSystem.Update();
            instance2.ackSystem.Update();
            (List<byte[]>, List<byte[]>) newMessages = badSocket.Update(dropChance, skipChance);
            receives1.AddRange(newMessages.Item1);
            receives2.AddRange(newMessages.Item2);
        }


        [UnityTest]
        [Explicit("Explicit Test: this test takes about 50 seconds to run")]
        public IEnumerator AllMessagesShouldHaveBeenReceivedInOrderFrames()
        {
            bool instance2Sends = false;
            int messageCount = 3000;
            float dropChance = 0.2f;
            float skipChance = 0.4f;

            // send all messages
            for (int i = 0; i < messageCount; i++)
            {
                instance1.messages.Add(createRandomData(i + 1));
                instance2.messages.Add(createRandomData(i + 1));

                // send inside loop so message sending alternates between 1 and 2

                // send to conn1
                instance1.ackSystem.SendReliable(instance1.messages[i]);

                if (instance2Sends)
                {
                    // send to conn2
                    instance2.ackSystem.SendReliable(instance2.messages[i]);
                }

                // fake Update
                Tick(dropChance, skipChance);
                yield return null;
            }

            // send 1 more message so that other side will for sure get last message
            // if we dont do then last message could be forgot and we receive 99/100
            instance1.ackSystem.SendReliable(new byte[1] { 0 });
            if (instance2Sends)
            {
                instance2.ackSystem.SendReliable(new byte[1] { 0 });
            }
            // run for enough updates that all message should be received
            // wait more than timeout incase 
            for (float t = 0; t < timeout * 2f; t += tick)
            {
                // fake Update
                Tick(0, 0);
            }

            // should not have null data
            Assert.That(instance1.connection.packets, Does.Not.Contain(null));
            Assert.That(instance2.connection.packets, Does.Not.Contain(null));

            // ---- asserts ---- // 
            Assert.That(receives2, Has.Count.EqualTo(messageCount + 1));
            Assert.That(receives1, Has.Count.EqualTo(instance2Sends ? messageCount + 1 : 0));

            // check all message reached other side
            for (int i = 0; i < messageCount; i++)
            {
                byte[] message = receives2[i];

                byte[] expected = instance1.message(i);
                AssertAreSameFromOffsets(expected, 0, message, 0, expected.Length);
            }


            if (instance2Sends)
            {
                for (int i = 0; i < messageCount; i++)
                {
                    byte[] message = receives1[i];

                    byte[] expected = instance2.message(i);
                    AssertAreSameFromOffsets(expected, 0, message, 0, expected.Length);
                }
            }
        }

    }
}
