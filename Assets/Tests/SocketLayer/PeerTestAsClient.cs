using System.Collections;
using Mirage.Tests;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    [Category("SocketLayer"), Description("tests for Peer that only apply to client")]
    [TestFixture(SocketBehavior.PollReceive)]
    [TestFixture(SocketBehavior.TickEvent)]
    public class PeerTestAsClient : PeerTestBase
    {
        public PeerTestAsClient(SocketBehavior behavior) : base(behavior)
        {
        }

        [Test]
        public void ConnectShouldSendMessageToSocket()
        {
            var handle = TestEndPoint.CreateSubstitute();
            peer.Connect((IConnectEndPoint)handle);

            var expected = connectRequest;

            socket.AsMock().AssertSendCall(1, handle, expected.Length,
                sent => sent.AreEquivalentIgnoringLength(expected));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ConnectShouldReturnANewConnection(bool disableReliable)
        {
            config.DisableReliableLayer = disableReliable;

            var endPoint = TestEndPoint.CreateSubstitute();
            var conn = peer.Connect((IConnectEndPoint)endPoint);
            if (disableReliable)
                Assert.That(conn, Is.TypeOf<NoReliableConnection>(), "returned type should be connection");
            else
                Assert.That(conn, Is.TypeOf<ReliableConnection>(), "returned type should be connection");

            Assert.That(conn.State, Is.EqualTo(ConnectionState.Connecting), "new connection should be connecting");
        }

        [Test]
        public void InvokesConnectEventAfterReceivingAccept()
        {
            var handle = TestEndPoint.CreateSubstitute();
            var conn = peer.Connect((IConnectEndPoint)handle);

            socket.AsMock().QueueReceiveCall(new byte[2] {
                (byte) PacketType.Command,
                (byte) Commands.ConnectionAccepted,
            }, handle);

            peer.UpdateTest();

            connectAction.Received(1).Invoke(conn);
        }


        [UnityTest]
        public IEnumerator ShouldResendConnectMessageIfNoReply()
        {
            var endPoint = TestEndPoint.CreateSubstitute();
            _ = peer.Connect((IConnectEndPoint)endPoint);

            var expected = connectRequest;

            // wait enough time so that  would have been called
            // make sure to call update so events are invoked
            // 0.5 little extra to be sure
            var end = time.Now + (config.MaxConnectAttempts * config.ConnectAttemptInterval) + 0.5f;
            double nextSendCheck = 0;
            var sendCount = 0;
            while (end > time.Now)
            {
                peer.UpdateTest();
                if (nextSendCheck < time.Now)
                {
                    nextSendCheck = time.Now + (config.ConnectAttemptInterval * 1.1f);
                    sendCount++;

                    // check send
                    var expectedCount = Mathf.Min(sendCount, config.MaxConnectAttempts);
                    socket.AsMock().AssertSendCall(expectedCount, endPoint, expected.Length,
                        sent => sent.AreEquivalentIgnoringLength(expected));
                }
                yield return null;
            }

            // check send is called max attempts times
            socket.AsMock().AssertSendCall(config.MaxConnectAttempts, endPoint, expected.Length,
                        sent => sent.AreEquivalentIgnoringLength(expected));
        }

        [UnityTest]
        public IEnumerator ShouldInvokeConnectionFailedIfNoReplyAfterMax()
        {
            var endPoint = TestEndPoint.CreateSubstitute();
            var conn = peer.Connect((IConnectEndPoint)endPoint);

            // wait enough time so that  would have been called
            // make sure to call update so events are invoked
            // 0.5 little extra to be sure
            var end = time.Now + (config.MaxConnectAttempts * config.ConnectAttemptInterval) + 0.5f;
            while (end > time.Now)
            {
                peer.UpdateTest();
                yield return null;
            }

            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
            disconnectAction.DidNotReceiveWithAnyArgs().Invoke(default, default);
            connectFailedAction.Received(1).Invoke(conn, RejectReason.Timeout);
        }
        [Test]
        public void ShouldInvokeConnectionFailedIfServerRejects()
        {
            var endPoint = TestEndPoint.CreateSubstitute();
            var conn = peer.Connect((IConnectEndPoint)endPoint);
            Debug.Assert(conn.Handle == endPoint, "Mock Socket should have returned the same connection");

            socket.AsMock().QueueReceiveCall(new byte[3] {
                (byte) PacketType.Command,
                (byte) Commands.ConnectionRejected,
                (byte)RejectReason.ServerFull,
            }, endPoint);
            peer.UpdateTest();

            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
            disconnectAction.DidNotReceiveWithAnyArgs().Invoke(default, default);
            connectFailedAction.Received(1).Invoke(conn, RejectReason.ServerFull);
        }


        [UnityTest]
        public IEnumerator InvokesConnectFailedIfClosedBeforeConnect()
        {
            var endPoint = TestEndPoint.CreateSubstitute();
            var conn = peer.Connect((IConnectEndPoint)endPoint);

            peer.Close();

            // wait enough time so that OnDisconnected would have been called
            // make sure to call update so events are invoked
            var start = UnityEngine.Time.time;
            // 0.5 little extra to be sure
            var maxTime = (config.MaxConnectAttempts * config.ConnectAttemptInterval) + 0.5f;
            while (start + maxTime < UnityEngine.Time.time)
            {
                peer.UpdateTest();
                yield return null;
            }

            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
            disconnectAction.DidNotReceiveWithAnyArgs().Invoke(default, default);
            connectFailedAction.Received(1).Invoke(conn, RejectReason.ClosedByPeer);
        }

        [Test]
        public void CloseSendsDisconnectMessageIfConnected()
        {
            // todo set up connected connections,
            // todo test as server with multiple connections
            // todo test as client with 1 connection
            Assert.Ignore("new NotImplementedException(What should happen if close / disconnect is called while still connecting)");

            var endPoint = TestEndPoint.CreateSubstitute();
            var conn = peer.Connect((IConnectEndPoint)endPoint);

            peer.Close();

            var expected = new byte[3]
            {
                (byte)PacketType.Command,
                (byte)Commands.Disconnect,
                (byte)DisconnectReason.RequestedByRemotePeer,
            };
            socket.AsMock().AssertSendCall(1, endPoint, expected.Length,
                sent => sent.AreEquivalentIgnoringLength(expected));
        }
    }
}
