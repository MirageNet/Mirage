using System;
using System.Collections.Generic;
using System.Linq;
using Mirage.Tests;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    [Category("SocketLayer"), Description("tests using multiple instances of peer to check they can connect to each other")]
    [TestFixture(SocketBehavior.PollReceive)]
    [TestFixture(SocketBehavior.TickEvent)]
    public class PeerTestConnecting
    {
        private const int ClientCount = 4;
        private PeerInstanceWithSocket server;
        private PeerInstanceWithSocket[] clients;
        private SocketBehavior _behavior;

        public PeerTestConnecting(SocketBehavior behavior)
        {
            _behavior = behavior;
        }

        [SetUp]
        public void SetUp()
        {
            server = new PeerInstanceWithSocket(_behavior, new Config { MaxConnections = ClientCount });
            clients = new PeerInstanceWithSocket[ClientCount];
            for (var i = 0; i < ClientCount; i++)
            {
                clients[i] = new PeerInstanceWithSocket(_behavior);
            }
        }

        [Test]
        public void ServerAcceptsAllClients()
        {
            server.peer.Bind(Substitute.For<IBindEndPoint>());

            var connectAction = Substitute.For<Action<IConnection>>();
            server.peer.OnConnected += connectAction;

            for (var i = 0; i < ClientCount; i++)
            {
                // tell client i to connect
                clients[i].peer.Connect((IConnectEndPoint)server.endPoint);
                var clientConnectAction = Substitute.For<Action<IConnection>>();
                clients[i].peer.OnConnected += clientConnectAction;

                // no change untill update
                Assert.That(server.socket.Sent.Count, Is.EqualTo(i));
                connectAction.ReceivedWithAnyArgs(i).Invoke(default);

                // run tick on server, should read packet from client i
                server.peer.UpdateTest();

                // server invokes connect event 
                connectAction.ReceivedWithAnyArgs(i + 1).Invoke(default);

                // sever send accept packet
                Assert.That(server.socket.Sent.Count, Is.EqualTo(i + 1));
                var lastSent = server.socket.Sent.Last();
                Assert.That(lastSent.endPoint, Is.EqualTo(clients[i].socket.endPoint));
                // check first 2 bytes of message
                Assert.That(ArgCollection.AreEquivalentIgnoringLength(lastSent.data, new byte[2] {
                    (byte)PacketType.Command,
                    (byte)Commands.ConnectionAccepted
                }));

                // no change on cleint till update
                clientConnectAction.ReceivedWithAnyArgs(0).Invoke(default);
                clients[i].peer.UpdateTest();
                clientConnectAction.ReceivedWithAnyArgs(1).Invoke(default);
            }
        }

        [Test]
        public void EachServerConnectionIsANewInstance()
        {
            server.peer.Bind(Substitute.For<IBindEndPoint>());
            var serverConnections = new List<IConnection>();

            Action<IConnection> connectAction = (conn) =>
            {
                serverConnections.Add(conn);
            };
            server.peer.OnConnected += connectAction;

            for (var i = 0; i < ClientCount; i++)
            {
                // tell client i to connect
                clients[i].peer.Connect((IConnectEndPoint)server.endPoint);

                // run tick on server, should read packet from client i
                server.peer.UpdateTest();

                Assert.That(serverConnections, Is.Unique);
            }
        }
    }
}
