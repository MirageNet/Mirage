using System;
using System.Linq;
using Mirage.Tests;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    [Category("SocketLayer")]
    [TestFixture(SocketBehavior.PollReceive)]
    [TestFixture(SocketBehavior.TickEvent)]
    public class StatefulPeerTest : PeerTestBase
    {
        public StatefulPeerTest(SocketBehavior behavior) : base(behavior)
        {
        }

        private IConnectionHandle clientStatefulHandle;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            clientStatefulHandle = TestEndPoint.CreateSubstitute(ConnectionHandleBehavior.Stateful);

            // override the default connect behaviour
            socket.Connect(Arg.Any<IConnectEndPoint>()).Returns(clientStatefulHandle);
        }

        [Test]
        public void ConnectShouldReturnAStatefulConnection()
        {
            var conn = peer.Connect((IConnectEndPoint)TestEndPoint.CreateSubstitute(ConnectionHandleBehavior.Stateful));

            Assert.That(conn, Is.TypeOf<ReliableConnection>());
            Assert.That(conn.State, Is.EqualTo(ConnectionState.Connecting));
            Assert.That(conn.Handle, Is.EqualTo(clientStatefulHandle));
            Assert.That(clientStatefulHandle.SocketLayerConnection, Is.EqualTo(conn));
            clientStatefulHandle.DidNotReceive().CreateCopy();
        }

        [Test]
        public void ServerShouldUseSameHandleForStatefulConnection()
        {
            peer.Bind(Substitute.For<IBindEndPoint>());

            var serverHandle = TestEndPoint.CreateSubstitute(ConnectionHandleBehavior.Stateful);

            socket.AsMock().QueueReceiveCall(connectRequest, serverHandle);

            IConnection capturedConnection = null;
            connectAction.When(x => x.Invoke(Arg.Any<IConnection>()))
                         .Do(info => capturedConnection = info.Arg<IConnection>());

            peer.UpdateTest();

            Assert.That(capturedConnection, Is.Not.Null);
            Assert.That(capturedConnection.Handle, Is.EqualTo(serverHandle));

            // Now the property should be set to the new connection
            Assert.That(serverHandle.SocketLayerConnection, Is.EqualTo(capturedConnection));

            serverHandle.DidNotReceive().CreateCopy();
        }

        [Test]
        public void ShouldCallDisconnectOnHandleWhenSocketDisconnects()
        {
            // connect first
            var conn = peer.Connect((IConnectEndPoint)TestEndPoint.CreateSubstitute(ConnectionHandleBehavior.Stateful));
            socket.AsMock().QueueReceiveCall(new byte[] { (byte)PacketType.Command, (byte)Commands.ConnectionAccepted }, clientStatefulHandle);
            peer.UpdateTest();
            Assert.That(conn.State, Is.EqualTo(ConnectionState.Connected));

            socket.AsMock().QueueDisconnectCall(clientStatefulHandle, DisconnectReason.Timeout);
            peer.UpdateReceive();

            disconnectAction.Received(1).Invoke(conn, DisconnectReason.Timeout);
            clientStatefulHandle.Received(1).Disconnect(null);
        }

        [Test]
        public void ShouldProcessDataFromOnDataEvent()
        {
            // connect first
            var conn = peer.Connect((IConnectEndPoint)TestEndPoint.CreateSubstitute(ConnectionHandleBehavior.Stateful));
            socket.AsMock().QueueReceiveCall(new byte[] { (byte)PacketType.Command, (byte)Commands.ConnectionAccepted }, clientStatefulHandle);
            peer.UpdateTest();
            Assert.That(conn.State, Is.EqualTo(ConnectionState.Connected));
            dataHandler.ClearReceivedCalls();

            var message = new byte[] { 1, 2, 3 };
            var packet = new byte[1 + 2 + message.Length];
            packet[0] = (byte)PacketType.Unreliable;
            var offset = 1;
            ByteUtils.WriteUShort(packet, ref offset, (ushort)message.Length);
            Buffer.BlockCopy(message, 0, packet, offset, message.Length);

            socket.AsMock().QueueReceiveCall(packet, clientStatefulHandle);
            peer.UpdateTest();

            dataHandler.Received(1).ReceiveMessage(conn, Arg.Is<ArraySegment<byte>>(x => x.SequenceEqual(new ArraySegment<byte>(message))));
        }

        [Test]
        public void SafeDisconnectFromErrorShouldDisconnectStatefulConnection()
        {
            // connect first
            var conn = peer.Connect((IConnectEndPoint)TestEndPoint.CreateSubstitute(ConnectionHandleBehavior.Stateful));
            socket.AsMock().QueueReceiveCall(new byte[] { (byte)PacketType.Command, (byte)Commands.ConnectionAccepted }, clientStatefulHandle);
            peer.UpdateTest();
            Assert.That(conn.State, Is.EqualTo(ConnectionState.Connected));

            const int aboveMTU = MAX_PACKET_SIZE + 10;
            socket.AsMock().QueueReceiveCall(new byte[1000], clientStatefulHandle, length: aboveMTU);

            LogAssert.Expect(LogType.Error, $"Socket returned length above MTU. MaxPacketSize:{MAX_PACKET_SIZE} length:{aboveMTU}");
            peer.UpdateTest();

            disconnectAction.Received(1).Invoke(conn, DisconnectReason.InvalidPacket);
            clientStatefulHandle.Received(1).Disconnect(null);
        }
    }
}
