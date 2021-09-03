using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.SocketLayer.Tests.AckSystemTests
{
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
                    if (UnityEngine.Random.value > dropChance)
                    {
                        ToSend.Add(packet);
                    }
                }
                processed = count1;

                var newPackets = new List<byte[]>();
                for (int i = 0; i < ToSend.Count; i++)
                {
                    if (UnityEngine.Random.value < skipChance) { continue; }
                    newPackets.AddRange(Receive(ackSystem, ToSend[i]));
                    ToSend.RemoveAt(i);
                    i--;
                }

                return newPackets;
            }

            private static List<byte[]> Receive(AckSystem ackSystem, byte[] packet)
            {
                var messages = new List<byte[]>();
                var type = (PacketType)packet[0];
                switch (type)
                {
                    case PacketType.Reliable:
                        ackSystem.ReceiveReliable(packet, packet.Length, false);
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

                while (ackSystem.NextReliablePacket(out AckSystem.ReliableReceived received))
                {
                    HandleAllMessageInPacket(messages, received);
                }
                return messages;
            }

            private static void HandleAllMessageInPacket(List<byte[]> messages, AckSystem.ReliableReceived received)
            {
                byte[] array = received.buffer.array;
                int packetLength = received.length;
                int offset = 0;
                while (offset < packetLength)
                {
                    ushort length = ByteUtils.ReadUShort(array, ref offset);
                    var message = new ArraySegment<byte>(array, offset, length);

                    byte[] outBuffer = new byte[length];
                    Buffer.BlockCopy(array, offset, outBuffer, 0, length);
                    offset += length;
                    messages.Add(outBuffer);
                }

                // release buffer after all its message have been handled
                received.buffer.Release();
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
