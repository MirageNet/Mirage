using System;
using Mirage.Tests;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    [Category("SocketLayer"), Description("tests for Peer that only apply to server")]
    [TestFixture(SocketBehavior.PollReceive, ConnectionHandleBehavior.Stateful)]
    [TestFixture(SocketBehavior.PollReceive, ConnectionHandleBehavior.Stateless)]
    [TestFixture(SocketBehavior.TickEvent, ConnectionHandleBehavior.Stateful)]
    [TestFixture(SocketBehavior.TickEvent, ConnectionHandleBehavior.Stateless)]
    public class PeerTestAsServer : PeerTestBase
    {
        private readonly ConnectionHandleBehavior _handleBehavior;

        public PeerTestAsServer(SocketBehavior behavior, ConnectionHandleBehavior handleBehavior) : base(behavior)
        {
            _handleBehavior = handleBehavior;
        }

        [Test]
        public void BindShouldCallSocketBind()
        {
            var endPoint = Substitute.For<IBindEndPoint>();
            peer.Bind(endPoint);

            socket.Received(1).Bind(Arg.Is(endPoint));
        }

        [Test]
        public void CloseSendsDisconnectMessageToAllConnections()
        {
            var endPoint = TestEndPoint.CreateSubstitute(_handleBehavior);
            peer.Bind(Substitute.For<IBindEndPoint>());

            var endPoints = new IConnectionHandle[maxConnections];
            for (var i = 0; i < maxConnections; i++)
            {
                endPoints[i] = TestEndPoint.CreateSubstitute(_handleBehavior);

                socket.AsMock().QueueReceiveCall(connectRequest, endPoints[i]);
                peer.UpdateTest();
            }

            for (var i = 0; i < maxConnections; i++)
            {
                socket.AsMock().ClearSendAndReceivedCalls();
            }

            peer.Close();

            var disconnectCommand = new byte[3]
            {
                (byte)PacketType.Command,
                (byte)Commands.Disconnect,
                (byte)DisconnectReason.RequestedByRemotePeer,
            };
            for (var i = 0; i < maxConnections; i++)
            {
                socket.AsMock().AssertSendCall(1, endPoints[i], disconnectCommand.Length,
                    actual => actual.AreEquivalentIgnoringLength(disconnectCommand));
            }
        }

        [Test]
        public void AcceptsConnectionForValidMessage()
        {
            peer.Bind(Substitute.For<IBindEndPoint>());

            var connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            var endPoint = TestEndPoint.CreateSubstitute(_handleBehavior);
            socket.AsMock().QueueReceiveCall(connectRequest, endPoint);
            peer.UpdateTest();

            // server sends accept and invokes event locally
            socket.AsMock().AssertSendCall(1, endPoint, 2, sent =>
                sent[0] == (byte)PacketType.Command &&
                sent[1] == (byte)Commands.ConnectionAccepted
            );
            connectAction.ReceivedWithAnyArgs(1).Invoke(default);
        }

        [Test]
        public void AcceptsConnectionsUpToMax()
        {
            peer.Bind(Substitute.For<IBindEndPoint>());

            var connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;


            var endPoints = new IConnectionHandle[maxConnections];
            for (var i = 0; i < maxConnections; i++)
            {
                endPoints[i] = TestEndPoint.CreateSubstitute(_handleBehavior);

                socket.AsMock().QueueReceiveCall(connectRequest, endPoints[i]);
                peer.UpdateTest();
            }


            // server sends accept and invokes event locally
            connectAction.ReceivedWithAnyArgs(maxConnections).Invoke(default);
            for (var i = 0; i < maxConnections; i++)
            {
                socket.AsMock().AssertSendCall(1, endPoints[i], 2, x =>
                    x[0] == (byte)PacketType.Command &&
                    x[1] == (byte)Commands.ConnectionAccepted);
            }
        }

        [Test]
        public void RejectsConnectionOverMax()
        {
            peer.Bind(Substitute.For<IBindEndPoint>());

            var connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            for (var i = 0; i < maxConnections; i++)
            {
                socket.AsMock().QueueReceiveCall(connectRequest, TestEndPoint.CreateSubstitute(_handleBehavior));
                peer.UpdateTest();
            }

            // clear calls from valid connections
            socket.AsMock().ClearSendAndReceivedCalls();
            connectAction.ClearReceivedCalls();

            var overMaxEndpoint = TestEndPoint.CreateSubstitute(_handleBehavior);
            socket.AsMock().QueueReceiveCall(connectRequest, overMaxEndpoint);

            peer.UpdateTest();

            const int length = 3;
            socket.AsMock().AssertSendCall(1, overMaxEndpoint, length, x =>
                x[0] == (byte)PacketType.Command &&
                x[1] == (byte)Commands.ConnectionRejected &&
                x[2] == (byte)RejectReason.ServerFull
            );
            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }
}
