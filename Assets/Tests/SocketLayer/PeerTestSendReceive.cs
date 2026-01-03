using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirage.Tests;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    public static class PeerTestExtensions
    {
        public static void UpdateTest(this Peer peer)
        {
            peer.UpdateReceive();
            peer.UpdateSent();
        }
    }
    [Category("SocketLayer"), Description("integration test to make sure that send and receiving works as a whole")]
    [TestFixture(SocketBehavior.PollReceive)]
    [TestFixture(SocketBehavior.TickEvent)]
    public class PeerTestSendReceive
    {
        private const int ClientCount = 4;
        private PeerInstanceWithSocket server;
        private PeerInstanceWithSocket[] clients;
        private List<IConnection> clientConnections;
        private List<IConnection> serverConnections;
        private int maxFragmentMessageSize;
        private float NotifyWaitTime;
        private readonly SocketBehavior _behavior;

        public PeerTestSendReceive(SocketBehavior behavior)
        {
            _behavior = behavior;
        }

        [SetUp]
        public void SetUp()
        {
            clientConnections = new List<IConnection>();
            serverConnections = new List<IConnection>();

            var config = new Config { MaxConnections = ClientCount };
            maxFragmentMessageSize = config.MaxReliableFragments * (PeerTestBase.MAX_PACKET_SIZE - AckSystem.MIN_RELIABLE_FRAGMENT_HEADER_SIZE);
            NotifyWaitTime = config.TimeBeforeEmptyAck * 2;


            server = new PeerInstanceWithSocket(_behavior, config);
            clients = new PeerInstanceWithSocket[ClientCount];
            Action<IConnection> serverConnect = (conn) => serverConnections.Add(conn);
            server.peer.OnConnected += serverConnect;

            server.peer.Bind((IBindEndPoint)TestEndPoint.CreateSubstitute());
            for (var i = 0; i < ClientCount; i++)
            {
                clients[i] = new PeerInstanceWithSocket(_behavior, config);
                clientConnections.Add(clients[i].peer.Connect((IConnectEndPoint)server.endPoint));
            }

            UpdateAll();
        }

        private void UpdateAll()
        {
            server.peer.UpdateTest();
            for (var i = 0; i < ClientCount; i++)
            {
                clients[i].peer.UpdateTest();
            }
        }

        private void CheckClientReceived(byte[] message)
        {
            UpdateAll();
            UpdateAll();

            // check each client got packet once
            for (var i = 0; i < ClientCount; i++)
            {
                var handler = clients[i].dataHandler;

                //handler.Received(1).ReceiveMessage(clientConnections[i], Arg.Is<ArraySegment<byte>>(x => x.SequenceEqual(message)));
                handler.Received(1).ReceiveMessage(clientConnections[i], Arg.Is<ArraySegment<byte>>(x => DebugSequenceEqual(message, x)));
            }
        }

        private void CheckServerReceived(byte[] message)
        {
            UpdateAll();
            UpdateAll();

            var handler = server.dataHandler;
            // check each client sent packet once
            for (var i = 0; i < ClientCount; i++)
            {
                //handler.Received(1).ReceiveMessage(serverConnections[i], Arg.Is<ArraySegment<byte>>(x => x.SequenceEqual(message)));
                handler.Received(1).ReceiveMessage(serverConnections[i], Arg.Is<ArraySegment<byte>>(x => DebugSequenceEqual(message, x)));
            }
        }

        [Test]
        public void SeverUnreliableSend()
        {
            var message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            // send 1 message to each client
            for (var i = 0; i < ClientCount; i++)
            {
                serverConnections[i].SendUnreliable(message);
            }

            CheckClientReceived(message);
        }


        [Test]
        public void ClientUnreliableSend()
        {
            var message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            // send 1 message from each client
            for (var i = 0; i < ClientCount; i++)
            {
                clientConnections[i].SendUnreliable(message);
            }

            CheckServerReceived(message);
        }

        [Test]
        public void ServerNotifySend()
        {
            var message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            // send 1 message to each client
            for (var i = 0; i < ClientCount; i++)
            {
                serverConnections[i].SendNotify(message);
            }

            CheckClientReceived(message);
        }

        [Test]
        public void ClientNotifySend()
        {
            var message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            // send 1 message from each client
            for (var i = 0; i < ClientCount; i++)
            {
                clientConnections[i].SendNotify(message);
            }

            CheckServerReceived(message);
        }

        [UnityTest]
        public IEnumerator ServerNotifySendMarkedAsReceived()
        {
            var message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            var received = new Action[ClientCount];
            var lost = new Action[ClientCount];
            // send 1 message to each client
            for (var i = 0; i < ClientCount; i++)
            {
                var token = serverConnections[i].SendNotify(message);

                received[i] = Substitute.For<Action>();
                lost[i] = Substitute.For<Action>();
                token.Delivered += received[i];
                token.Lost += lost[i];
            }

            var end = UnityEngine.Time.time + NotifyWaitTime;
            while (end > UnityEngine.Time.time)
            {
                UpdateAll();
                yield return null;
            }

            for (var i = 0; i < ClientCount; i++)
            {
                received[i].Received(1).Invoke();
                lost[i].DidNotReceive().Invoke();
            }
        }

        [UnityTest]
        public IEnumerator ClientNotifySendMarkedAsReceived()
        {
            var message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            var received = new Action[ClientCount];
            var lost = new Action[ClientCount];
            // send 1 message from each client
            for (var i = 0; i < ClientCount; i++)
            {
                var token = clientConnections[i].SendNotify(message);

                received[i] = Substitute.For<Action>();
                lost[i] = Substitute.For<Action>();
                token.Delivered += received[i];
                token.Lost += lost[i];
            }

            var end = UnityEngine.Time.time + NotifyWaitTime;
            while (end > UnityEngine.Time.time)
            {
                UpdateAll();
                yield return null;
            }

            for (var i = 0; i < ClientCount; i++)
            {
                received[i].Received(1).Invoke();
                lost[i].DidNotReceive().Invoke();
            }
        }

        [UnityTest]
        public IEnumerator ServerNotifySendCallbacksMarkedAsReceived()
        {
            var message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            var callBacks = new INotifyCallBack[ClientCount];
            // send 1 message to each client
            for (var i = 0; i < ClientCount; i++)
            {
                callBacks[i] = Substitute.For<INotifyCallBack>();
                serverConnections[i].SendNotify(message, callBacks[i]);
            }

            var end = UnityEngine.Time.time + NotifyWaitTime;
            while (end > UnityEngine.Time.time)
            {
                UpdateAll();
                yield return null;
            }

            for (var i = 0; i < ClientCount; i++)
            {
                callBacks[i].Received(1).OnDelivered();
                callBacks[i].DidNotReceive().OnLost();
            }
        }

        [UnityTest]
        public IEnumerator ClientNotifySendCallbacksMarkedAsReceived()
        {
            var message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            var callBacks = new INotifyCallBack[ClientCount];
            // send 1 message from each client
            for (var i = 0; i < ClientCount; i++)
            {
                callBacks[i] = Substitute.For<INotifyCallBack>();
                clientConnections[i].SendNotify(message, callBacks[i]);
            }

            var end = UnityEngine.Time.time + NotifyWaitTime;
            while (end > UnityEngine.Time.time)
            {
                UpdateAll();
                yield return null;
            }

            for (var i = 0; i < ClientCount; i++)
            {
                callBacks[i].Received(1).OnDelivered();
                callBacks[i].DidNotReceive().OnLost();
            }
        }

        [Test]
        public void ServerReliableSend()
        {
            var message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            // send 1 message to each client
            for (var i = 0; i < ClientCount; i++)
            {
                serverConnections[i].SendReliable(message);
            }

            CheckClientReceived(message);
        }

        [Test]
        public void ClientReliableSend()
        {
            var message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            // send 1 message from each client
            for (var i = 0; i < ClientCount; i++)
            {
                clientConnections[i].SendReliable(message);
            }

            CheckServerReceived(message);
        }

        private const int DEFAULT_MAX_FRAGMENTS = 50;

        [Test]
        [TestCase(1, DEFAULT_MAX_FRAGMENTS)]
        [TestCase(0.8f, (int)(DEFAULT_MAX_FRAGMENTS * 0.8))]
        [TestCase(0.5f, (int)(DEFAULT_MAX_FRAGMENTS * 0.5))]
        [TestCase(0.3f, (int)(DEFAULT_MAX_FRAGMENTS * 0.3))]
        [TestCase(0.2f, (int)(DEFAULT_MAX_FRAGMENTS * 0.2))]
        public void FragmentedSend(float maxMultiplier, int expectedFragments)
        {
            var size = (int)(maxFragmentMessageSize * maxMultiplier);
            var message = Enumerable.Range(10, size).Select(x => (byte)x).ToArray();

            var sentCount = server.socket.Sent.Count;

            serverConnections[0].SendReliable(message);
            var handler = clients[0].dataHandler;

            // change in sent
            Assert.That(server.socket.Sent.Count - sentCount, Is.EqualTo(expectedFragments));

            UpdateAll();

            handler.Received(1).ReceiveMessage(clientConnections[0], Arg.Is<ArraySegment<byte>>(x => x.SequenceEqual(message)));
        }

        [Test]
        public void FragmentedSendThrowsIfTooBig()
        {
            var message = Enumerable.Range(10, maxFragmentMessageSize + 1).Select(x => (byte)x).ToArray();

            Assert.Throws<MessageSizeException>(() =>
            {
                serverConnections[0].SendReliable(message);
            });

            UpdateAll();

            var handler = clients[0].dataHandler;
            handler.DidNotReceive().ReceiveMessage(Arg.Any<IConnection>(), Arg.Any<ArraySegment<byte>>());
        }



        private bool DebugSequenceEqual(byte[] inMsg, ArraySegment<byte> outMsg)
        {
            if (inMsg.Length != outMsg.Count) { return false; }

            for (var i = 0; i < inMsg.Length; i++)
            {
                if (inMsg[i] != outMsg.Array[outMsg.Offset + i]) { return false; }
            }

            return true;
        }
    }
}
