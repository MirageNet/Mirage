using System;
using System.Collections;
using System.Net;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    [Category("SocketLayer"), Description("tests for Peer that only apply to client")]
    public class PeerTestAsClient : PeerTestBase
    {
        [Test]
        public void ConnectShouldSendMessageToSocket()
        {
            EndPoint endPoint = Substitute.For<EndPoint>();
            peer.Connect(endPoint);

            byte[] expected = new byte[2]
            {
                (byte)PacketType.Command,
                (byte)Commands.ConnectRequest,
            };
            socket.Received(1).Send(
                Arg.Is(endPoint),
                Arg.Is<byte[]>(actual => actual.AreEquivalentIgnoringLength(expected)),
                Arg.Is(expected.Length)
                );
        }

        [Test]
        public void ConnectShouldReturnANewConnection()
        {
            EndPoint endPoint = Substitute.For<EndPoint>();
            IConnection conn = peer.Connect(endPoint);
            Assert.That(conn, Is.TypeOf<Connection>(), "returned type should be connection");
            Assert.That(conn.State, Is.EqualTo(ConnectionState.Connecting), "new connection should be connecting");
        }

        [UnityTest]
        public IEnumerator OnDisconnectedIsNotInvokedIfClosedBeforeConnected()
        {
            Action<IConnection, DisconnectReason> disconnectAction = Substitute.For<Action<IConnection, DisconnectReason>>();
            peer.OnDisconnected += disconnectAction;

            EndPoint endPoint = Substitute.For<EndPoint>();
            IConnection conn = peer.Connect(endPoint);

            peer.Close();

            // wait enough time so that OnDisconnected would have been called
            // make sure to call update so events are invoked
            float start = UnityEngine.Time.time;
            while (start + 1.5f < UnityEngine.Time.time)
            {
                peer.Update();
                yield return null;
            }

            disconnectAction.DidNotReceiveWithAnyArgs().Invoke(default, default);
        }

        [Test]
        public void CloseSendsDisconnectMessageIfConnected()
        {
            // todo set up connected connections,
            // todo test as server with multiple connections
            // todo test as client with 1 connection
            Assert.Ignore("new NotImplementedException(What should happen if close / disconnect is called while still connecting)");

            EndPoint endPoint = Substitute.For<EndPoint>();
            IConnection conn = peer.Connect(endPoint);

            peer.Close();

            byte[] expected = new byte[3]
            {
                (byte)PacketType.Command,
                (byte)Commands.Disconnect,
                (byte)DisconnectReason.RequestedByPeer,
            };
            socket.Received(1).Send(
                Arg.Is(endPoint),
                Arg.Is<byte[]>(actual => actual.AreEquivalentIgnoringLength(expected)),
                Arg.Is(expected.Length)
                );
        }
    }
}
