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
    [Category("SocketLayer"), Description("integration test to make sure that send and receiving works as a whole")]
    public class PeerTestSendReceive
    {
        const int ClientCount = 4;
        PeerInstanceWithSocket server;
        PeerInstanceWithSocket[] clients;

        List<IConnection> clientConnections;
        List<IConnection> serverConnections;

        int maxFragmentMessageSize;
        float NotifyWaitTime;

        [SetUp]
        public void SetUp()
        {
            clientConnections = new List<IConnection>();
            serverConnections = new List<IConnection>();

            var config = new Config { MaxConnections = ClientCount };
            maxFragmentMessageSize = config.MaxReliableFragments * (config.MaxPacketSize - AckSystem.MIN_RELIABLE_FRAGMENT_HEADER_SIZE);
            NotifyWaitTime = config.TimeBeforeEmptyAck * 2;


            server = new PeerInstanceWithSocket(config);
            clients = new PeerInstanceWithSocket[ClientCount];
            Action<IConnection> serverConnect = (conn) => serverConnections.Add(conn);
            server.peer.OnConnected += serverConnect;

            server.peer.Bind(TestEndPoint.CreateSubstitute());
            for (int i = 0; i < ClientCount; i++)
            {
                clients[i] = new PeerInstanceWithSocket(config);
                clientConnections.Add(clients[i].peer.Connect(server.endPoint));
            }

            UpdateAll();
        }

        void UpdateAll()
        {
            server.peer.Update();
            for (int i = 0; i < ClientCount; i++)
            {
                clients[i].peer.Update();
            }
        }

        private void CheckClientReceived(byte[] message)
        {
            UpdateAll();
            UpdateAll();

            // check each client got packet once
            for (int i = 0; i < ClientCount; i++)
            {
                IDataHandler handler = clients[i].dataHandler;

                //handler.Received(1).ReceiveMessage(clientConnections[i], Arg.Is<ArraySegment<byte>>(x => x.SequenceEqual(message)));
                handler.Received(1).ReceiveMessage(clientConnections[i], Arg.Is<ArraySegment<byte>>(x => DebugSequenceEqual(message, x)));
            }
        }

        private void CheckServerReceived(byte[] message)
        {
            UpdateAll();
            UpdateAll();

            IDataHandler handler = server.dataHandler;
            // check each client sent packet once
            for (int i = 0; i < ClientCount; i++)
            {
                //handler.Received(1).ReceiveMessage(serverConnections[i], Arg.Is<ArraySegment<byte>>(x => x.SequenceEqual(message)));
                handler.Received(1).ReceiveMessage(serverConnections[i], Arg.Is<ArraySegment<byte>>(x => DebugSequenceEqual(message, x)));
            }
        }

        [Test]
        public void SeverUnreliableSend()
        {
            byte[] message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            // send 1 message to each client
            for (int i = 0; i < ClientCount; i++)
            {
                serverConnections[i].SendUnreliable(message);
            }

            CheckClientReceived(message);
        }


        [Test]
        public void ClientUnreliableSend()
        {
            byte[] message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            // send 1 message from each client
            for (int i = 0; i < ClientCount; i++)
            {
                clientConnections[i].SendUnreliable(message);
            }

            CheckServerReceived(message);
        }

        [Test]
        public void ServerNotifySend()
        {
            byte[] message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            // send 1 message to each client
            for (int i = 0; i < ClientCount; i++)
            {
                serverConnections[i].SendNotify(message);
            }

            CheckClientReceived(message);
        }

        [Test]
        public void ClientNotifySend()
        {
            byte[] message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            // send 1 message from each client
            for (int i = 0; i < ClientCount; i++)
            {
                clientConnections[i].SendNotify(message);
            }

            CheckServerReceived(message);
        }

        [UnityTest]
        public IEnumerator ServerNotifySendMarkedAsReceived()
        {
            byte[] message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            var received = new Action[ClientCount];
            var lost = new Action[ClientCount];
            // send 1 message to each client
            for (int i = 0; i < ClientCount; i++)
            {
                INotifyToken token = serverConnections[i].SendNotify(message);

                received[i] = Substitute.For<Action>();
                lost[i] = Substitute.For<Action>();
                token.Delivered += received[i];
                token.Lost += lost[i];
            }

            float end = UnityEngine.Time.time + NotifyWaitTime;
            while (end > UnityEngine.Time.time)
            {
                UpdateAll();
                yield return null;
            }

            for (int i = 0; i < ClientCount; i++)
            {
                received[i].Received(1).Invoke();
                lost[i].DidNotReceive().Invoke();
            }
        }

        [UnityTest]
        public IEnumerator ClientNotifySendMarkedAsReceived()
        {
            byte[] message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            var received = new Action[ClientCount];
            var lost = new Action[ClientCount];
            // send 1 message from each client
            for (int i = 0; i < ClientCount; i++)
            {
                INotifyToken token = clientConnections[i].SendNotify(message);

                received[i] = Substitute.For<Action>();
                lost[i] = Substitute.For<Action>();
                token.Delivered += received[i];
                token.Lost += lost[i];
            }

            float end = UnityEngine.Time.time + NotifyWaitTime;
            while (end > UnityEngine.Time.time)
            {
                UpdateAll();
                yield return null;
            }

            for (int i = 0; i < ClientCount; i++)
            {
                received[i].Received(1).Invoke();
                lost[i].DidNotReceive().Invoke();
            }
        }


        [Test]
        public void ServerReliableSend()
        {
            byte[] message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            // send 1 message to each client
            for (int i = 0; i < ClientCount; i++)
            {
                serverConnections[i].SendReliable(message);
            }

            CheckClientReceived(message);
        }

        [Test]
        public void ClientReliableSend()
        {
            byte[] message = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            // send 1 message from each client
            for (int i = 0; i < ClientCount; i++)
            {
                clientConnections[i].SendReliable(message);
            }

            CheckServerReceived(message);
        }

        [Test]
        [TestCase(1, 5)]
        [TestCase(0.8f, 4)]
        [TestCase(0.5f, 3)]
        [TestCase(0.3f, 2)]
        [TestCase(0.2f, 1)]
        public void FragmentedSend(float maxMultiplier, int expectedFragments)
        {
            int size = (int)(maxFragmentMessageSize * maxMultiplier);
            byte[] message = Enumerable.Range(10, size).Select(x => (byte)x).ToArray();

            int sentCount = server.socket.Sent.Count;

            serverConnections[0].SendReliable(message);
            IDataHandler handler = clients[0].dataHandler;

            // change in sent
            Assert.That(server.socket.Sent.Count - sentCount, Is.EqualTo(expectedFragments));

            UpdateAll();

            handler.Received(1).ReceiveMessage(clientConnections[0], Arg.Is<ArraySegment<byte>>(x => x.SequenceEqual(message)));
        }

        [Test]
        public void FragmentedSendThrowsIfTooBig()
        {
            byte[] message = Enumerable.Range(10, maxFragmentMessageSize + 1).Select(x => (byte)x).ToArray();

            Assert.Throws<ArgumentException>(() =>
            {
                serverConnections[0].SendReliable(message);
            });

            UpdateAll();

            IDataHandler handler = clients[0].dataHandler;
            handler.DidNotReceive().ReceiveMessage(Arg.Any<IConnection>(), Arg.Any<ArraySegment<byte>>());
        }



        private bool DebugSequenceEqual(byte[] inMsg, ArraySegment<byte> outMsg)
        {
            if (inMsg.Length != outMsg.Count) { return false; }

            for (int i = 0; i < inMsg.Length; i++)
            {
                if (inMsg[i] != outMsg.Array[outMsg.Offset + i]) { return false; }
            }

            return true;
        }
    }
}
