using System;
using System.Linq;
using System.Text.RegularExpressions;
using Mirage.SocketLayer.Tests.AckSystemTests;
using Mirage.Tests;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    [Category("SocketLayer")]
    public class PeerFragmentBufferTests : PeerTestBase
    {
        // Use PollReceive for simpler synchronous testing
        public PeerFragmentBufferTests() : base(SocketBehavior.PollReceive) { }

        protected Pool<AckSystem.ReliablePacket> reliablePool;
        protected Pool<RingBuffer<AckSystem.AckablePacket>> ackablePacketPool;
        protected Pool<RingBuffer<AckSystem.ReliableReceived>> reliableReceivePool;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            reliablePool = new Pool<AckSystem.ReliablePacket>(AckSystem.ReliablePacket.CreateNew, 0, config.MaxReliablePacketsInSendBufferPerConnection);
            ackablePacketPool = new Pool<RingBuffer<AckSystem.AckablePacket>>((p) => new RingBuffer<AckSystem.AckablePacket>(config.SequenceSize, null), 0, 10);
            reliableReceivePool = new Pool<RingBuffer<AckSystem.ReliableReceived>>((p) => new RingBuffer<AckSystem.ReliableReceived>(config.SequenceSize, null), 0, 10);
        }

        [Test]
        public void UsesSharedBufferForFragmentedMessage()
        {
            var endPoint = TestEndPoint.CreateSubstitute();
            var connection = peer.Connect(endPoint);
            ((Connection)connection).State = ConnectionState.Connected;
            Assert.IsTrue(((Connection)connection).Connected, "Connection should be connected before receiving fragments");

            // Manually trigger a fragmented message reassembly
            // We'll create two fragments for a message
            var mtu = MAX_PACKET_SIZE;
            var payloadSize = mtu - AckSystem.MIN_RELIABLE_FRAGMENT_HEADER_SIZE;
            var message = Enumerable.Range(0, payloadSize * 2).Select(i => (byte)i).ToArray();

            var remoteConnection = new SubIRawConnection();
            var ackable = ackablePacketPool.Take();
            ackable.Reset();
            var receive = reliableReceivePool.Take();
            receive.Reset();
            var remoteAckSystem = new AckSystem(remoteConnection, config, mtu, time, peer._bufferPool,
                reliablePool, ackable, receive, () => { });
            remoteAckSystem.SendReliable(message);
            remoteAckSystem.Update(); // Flush the batch to get packets in remoteConnection

            // The first packet sent (index 1) and second (index 0)
            var packet1 = remoteConnection.packets[0];
            var packet2 = remoteConnection.packets[1];

            // Feed them into our peer's socket
            socket.AsMock().QueueReceiveCall(packet1, endPoint);
            socket.AsMock().QueueReceiveCall(packet2, endPoint);

            peer.UpdateReceive();

            // Verify that the shared buffer was used
            dataHandler.Received(1).ReceiveMessage(connection, Arg.Is<ArraySegment<byte>>(x => x.Array == peer._fragmentBuffer));
            // And that it's no longer marked as in use
            Assert.IsFalse(peer._fragmentBufferInUse);
        }

        [Test]
        public void FallbackToAllocationWhenBufferInUse()
        {
            var endPoint = TestEndPoint.CreateSubstitute();
            var connection = peer.Connect(endPoint);
            ((Connection)connection).State = ConnectionState.Connected;
            Assert.IsTrue(((Connection)connection).Connected, "Connection should be connected before receiving fragments");

            var mtu = MAX_PACKET_SIZE;
            var payloadSize = mtu - AckSystem.MIN_RELIABLE_FRAGMENT_HEADER_SIZE;
            var message = Enumerable.Range(0, payloadSize * 2).Select(i => (byte)i).ToArray();

            var remoteConnection = new SubIRawConnection();
            var ackable2 = ackablePacketPool.Take();
            ackable2.Reset();
            var receive2 = reliableReceivePool.Take();
            receive2.Reset();
            var remoteAckSystem = new AckSystem(remoteConnection, config, mtu, time, peer._bufferPool,
                reliablePool, ackable2, receive2, () => { });
            remoteAckSystem.SendReliable(message);
            remoteAckSystem.Update();

            var packet1 = remoteConnection.packets[0];
            var packet2 = remoteConnection.packets[1];

            // CORRUPT the state: mark as in use
            peer._fragmentBufferInUse = true;

            socket.AsMock().QueueReceiveCall(packet1, endPoint);
            socket.AsMock().QueueReceiveCall(packet2, endPoint);

            // Expect the error log
            LogAssert.Expect(LogType.Error, new Regex("Fragment buffer already in use.*"));

            peer.UpdateReceive();

            // Verify that a DIFFERENT array was used (fallback allocation)
            dataHandler.Received(1).ReceiveMessage(connection, Arg.Is<ArraySegment<byte>>(x => x.Array != peer._fragmentBuffer));

            // State should still be "in use" because we set it manually and the finally block only clears it if it was the one who set it
            Assert.IsTrue(peer._fragmentBufferInUse);
        }

        [Test]
        public void DisconnectsWhenMessageExceedsMaxFragments()
        {
            var endPoint = TestEndPoint.CreateSubstitute();
            var connection = peer.Connect(endPoint);
            ((Connection)connection).State = ConnectionState.Connected;

            // DIAGNOSTIC: Verify lookup and state
            Assert.IsTrue(((Connection)connection).Connected, "Connection must be connected");

            // Create a packet that claims to have more fragments than allowed
            var badPacket = new byte[AckSystem.MIN_RELIABLE_FRAGMENT_HEADER_SIZE + 1];
            var offset = 0;
            ByteUtils.WriteByte(badPacket, ref offset, (byte)PacketType.ReliableFragment);
            ByteUtils.WriteUShort(badPacket, ref offset, 0); // sequence
            ByteUtils.WriteUShort(badPacket, ref offset, 0); // ackSequence
            ByteUtils.WriteULong(badPacket, ref offset, 0); // ackMask
            ByteUtils.WriteUShort(badPacket, ref offset, 0); // reliableSequence
            ByteUtils.WriteByte(badPacket, ref offset, (byte)config.MaxReliableFragments); // Index = Max (invalid)

            socket.AsMock().QueueReceiveCall(badPacket, endPoint);

            // Expect the disconnect log (which only happens if state is Connected)
            LogAssert.Expect(LogType.Error, new Regex(".*Received invalid fragment. Disconnecting.*"));

            peer.UpdateReceive();

            // Verify disconnect
            disconnectAction.Received(1).Invoke(Arg.Any<IConnection>(), DisconnectReason.InvalidPacket);
            Assert.That(connection.State, Is.EqualTo(ConnectionState.Disconnected));
        }

        [Test]
        public void DisconnectsWhenFragmentReceivedAndDisabled()
        {
            config.MaxReliableFragments = 0;

            // Use the logger from PeerTestBase so we can catch it with LogAssert
            var localPeer = new Peer(socket, MAX_PACKET_SIZE, dataHandler, config, logger);
            localPeer.OnDisconnected += disconnectAction;

            var endPoint = TestEndPoint.CreateSubstitute();
            var connection = localPeer.Connect(endPoint);
            ((Connection)connection).State = ConnectionState.Connected;
            Assert.IsTrue(((Connection)connection).Connected, "Connection must be connected");

            var fragmentPacket = new byte[AckSystem.MIN_RELIABLE_FRAGMENT_HEADER_SIZE + 10];
            var offset = 0;
            ByteUtils.WriteByte(fragmentPacket, ref offset, (byte)PacketType.ReliableFragment);
            offset = AckSystem.SEQUENCE_HEADER + sizeof(ushort);
            ByteUtils.WriteByte(fragmentPacket, ref offset, 0);

            socket.AsMock().QueueReceiveCall(fragmentPacket, endPoint);

            LogAssert.Expect(LogType.Error, new Regex("Received fragmented message but fragmentation is disabled.*"));

            localPeer.UpdateReceive();

            disconnectAction.Received(1).Invoke(Arg.Any<IConnection>(), DisconnectReason.InvalidPacket);
            Assert.That(connection.State, Is.EqualTo(ConnectionState.Disconnected));
        }
    }
}
