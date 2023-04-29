using System;
using System.Collections;
using Mirage.Tests;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    [Category("SocketLayer"), Description("tests for Peer that only apply to client")]
    public class PeerTestAsClient : PeerTestBase
    {
        [Test]
        public void ConnectShouldSendMessageToSocket()
        {
            var endPoint = TestEndPoint.CreateSubstitute();
            peer.Connect(endPoint);

            var expected = connectRequest;

            socket.Received(1).Send(
                Arg.Is(endPoint),
                Arg.Is<byte[]>(actual => actual.AreEquivalentIgnoringLength(expected)),
                Arg.Is(expected.Length)
                );
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ConnectShouldReturnANewConnection(bool disableReliable)
        {
            config.DisableReliableLayer = disableReliable;

            var endPoint = TestEndPoint.CreateSubstitute();
            var conn = peer.Connect(endPoint);
            if (disableReliable)
                Assert.That(conn, Is.TypeOf<NoReliableConnection>(), "returned type should be connection");
            else
                Assert.That(conn, Is.TypeOf<ReliableConnection>(), "returned type should be connection");

            Assert.That(conn.State, Is.EqualTo(ConnectionState.Connecting), "new connection should be connecting");
        }

        [Test]
        public void InvokesConnectEventAfterReceiveingAcccept()
        {
            Assert.Ignore("not implemented");
        }


        [UnityTest]
        public IEnumerator ShouldResendConnectMessageIfNoReply()
        {
            var endPoint = TestEndPoint.CreateSubstitute();
            _ = peer.Connect(endPoint);

            var expected = connectRequest;

            // wait enough time so that  would have been called
            // make sure to call update so events are invoked
            // 0.5 little extra to be sure
            var end = time.Now + (config.MaxConnectAttempts * config.ConnectAttemptInterval) + 0.5f;
            float nextSendCheck = 0;
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
                    socket.Received(expectedCount).Send(
                        Arg.Is(endPoint),
                        Arg.Is<byte[]>(actual => actual.AreEquivalentIgnoringLength(expected)),
                        Arg.Is(expected.Length)
                    );
                }
                yield return null;
            }

            // check send is called max attempts times
            socket.Received(config.MaxConnectAttempts).Send(
                Arg.Is(endPoint),
                Arg.Is<byte[]>(actual => actual.AreEquivalentIgnoringLength(expected)),
                Arg.Is(expected.Length)
            );
        }

        [UnityTest]
        public IEnumerator ShouldInvokeConnectionFailedIfNoReplyAfterMax()
        {
            var endPoint = TestEndPoint.CreateSubstitute();
            var conn = peer.Connect(endPoint);

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
            var conn = peer.Connect(endPoint);

            socket.SetupReceiveCall(new byte[3] {
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
            var conn = peer.Connect(endPoint);

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
            var conn = peer.Connect(endPoint);

            peer.Close();

            var expected = new byte[3]
            {
                (byte)PacketType.Command,
                (byte)Commands.Disconnect,
                (byte)DisconnectReason.RequestedByRemotePeer,
            };
            socket.Received(1).Send(
                Arg.Is(endPoint),
                Arg.Is<byte[]>(actual => actual.AreEquivalentIgnoringLength(expected)),
                Arg.Is(expected.Length)
                );
        }

        [Test]
        public void IgnoresRequestToConnect()
        {
            Assert.Ignore("not implemented");
            peer.Connect(TestEndPoint.CreateSubstitute());

            var connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            var expected = connectRequest;
            var endPoint = TestEndPoint.CreateSubstitute();
            socket.SetupReceiveCall(expected, endPoint);
            peer.UpdateTest();

            // server sends accept and invokes event locally
            socket.Received(1).Send(endPoint, Arg.Is<byte[]>(x =>
                x.Length >= 2 &&
                x[0] == (byte)PacketType.Command &&
                x[1] == (byte)Commands.ConnectionAccepted
            ), 2);
            connectAction.ReceivedWithAnyArgs(1).Invoke(default);
        }
    }
}
