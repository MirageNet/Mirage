using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    [Category("SocketLayer"), Description("tests using multiple instances of peer to check they can connect to each other")]
    public class PeerTestConnecting
    {
        const int ClientCount = 4;
        PeerInstanceWithSocket server;
        PeerInstanceWithSocket[] clients;

        [SetUp]
        public void SetUp()
        {
            server = new PeerInstanceWithSocket(new Config { MaxConnections = ClientCount });
            clients = new PeerInstanceWithSocket[ClientCount];
            for (int i = 0; i < ClientCount; i++)
            {
                clients[i] = new PeerInstanceWithSocket();

                // add remotes so they can send message to each other
                server.socket.AddRemote(clients[i].socket);
                clients[i].socket.AddRemote(server.socket);
            }
        }

        [Test]
        public void ServerAcceptsAllClients()
        {
            server.peer.Bind(Substitute.For<EndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            server.peer.OnConnected += connectAction;

            for (int i = 0; i < ClientCount; i++)
            {
                // tell client i to connect
                clients[i].peer.Connect(server.endPoint);
                Action<IConnection> clientConnectAction = Substitute.For<Action<IConnection>>();
                clients[i].peer.OnConnected += clientConnectAction;

                // no change untill update
                Assert.That(server.socket.Sent.Count, Is.EqualTo(i));
                connectAction.ReceivedWithAnyArgs(i).Invoke(default);

                // run tick on server, should read packet from client i
                server.peer.Update();

                // server invokes connect event 
                connectAction.ReceivedWithAnyArgs(i + 1).Invoke(default);

                // sever send accept packet
                Assert.That(server.socket.Sent.Count, Is.EqualTo(i + 1));
                TestSocket.Packet lastSent = server.socket.Sent.Last();
                Assert.That(lastSent.endPoint, Is.EqualTo(clients[i].socket.endPoint));
                // check first 2 bytes of message
                Assert.That(ArgCollection.AreEquivalentIgnoringLength(lastSent.data, new byte[2] {
                    (byte)PacketType.Command,
                    (byte)Commands.ConnectionAccepted
                }));

                // no change on cleint till update
                clientConnectAction.ReceivedWithAnyArgs(0).Invoke(default);
                clients[i].peer.Update();
                clientConnectAction.ReceivedWithAnyArgs(1).Invoke(default);
            }
        }

        [Test]
        public void EachServerConnectionIsANewInstance()
        {
            server.peer.Bind(Substitute.For<EndPoint>());
            var serverConnections = new List<IConnection>();

            Action<IConnection> connectAction = (conn) =>
            {
                serverConnections.Add(conn);
            };
            server.peer.OnConnected += connectAction;

            for (int i = 0; i < ClientCount; i++)
            {
                // tell client i to connect
                clients[i].peer.Connect(server.endPoint);

                // run tick on server, should read packet from client i
                server.peer.Update();

                Assert.That(serverConnections, Is.Unique);
            }
        }
    }
}
