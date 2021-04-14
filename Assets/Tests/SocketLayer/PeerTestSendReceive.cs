using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    [Category("SocketLayer"), Description("integration test to make sure that send and receiving works as a whole")]
    public class PeerTestSendReceive
    {
        const int ClientCount = 4;
        PeerInstanceWithSocket server;
        PeerInstanceWithSocket[] clients;

        List<IConnection> clientConnections = new List<IConnection>();
        List<IConnection> serverConnections = new List<IConnection>();

        [SetUp]
        public void SetUp()
        {
            server = new PeerInstanceWithSocket(new Config { MaxConnections = ClientCount });
            clients = new PeerInstanceWithSocket[ClientCount];
            Action<IConnection> serverConnect = (conn) => serverConnections.Add(conn);
            server.peer.OnConnected += serverConnect;

            server.peer.Bind(Substitute.For<EndPoint>());
            for (int i = 0; i < ClientCount; i++)
            {
                clients[i] = new PeerInstanceWithSocket();

                // add remotes so they can send message to each other
                server.socket.AddRemote(clients[i].socket);
                clients[i].socket.AddRemote(server.socket);

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

        [Test]
        public void SeverUnreliableSend()
        {
            byte[] packet = Enumerable.Range(10, 20).Select(x => (byte)x).ToArray();

            // send 1 message to each client
            for (int i = 0; i < ClientCount; i++)
            {
                serverConnections[i].SendUnreliable(packet);
            }

            UpdateAll();

            // check each client got packet once
            for (int i = 0; i < ClientCount; i++)
            {
                IDataHandler handler = clients[i].dataHandler;

                handler.Received(1).ReceiveData(clientConnections[i], Arg.Is<ArraySegment<byte>>(x => x.SequenceEqual(packet)));
            }
        }

        [Test]
        public void ClientUnreliableSend()
        {
            Assert.Ignore("not implemented");
        }

        [Test]
        public void ServerNotifySend()
        {
            Assert.Ignore("not implemented");
        }

        [Test]
        public void ClientNofitySend()
        {
            Assert.Ignore("not implemented");
        }

        [Test]
        public void ServerNotifyAck()
        {
            Assert.Ignore("not implemented");
        }

        [Test]
        public void ClientNofityAck()
        {
            Assert.Ignore("not implemented");
        }
    }
}
