using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    public class PeerTestBase
    {
        PeerInstance instance;

        // helper properties to access instance
        protected ISocket socket => instance.socket;
        protected IDataHandler dataHandler => instance.dataHandler;
        protected Config config => instance.config;
        protected ILogger logger => instance.logger;
        protected Peer peer => instance.peer;

        [SetUp]
        public void SetUp()
        {
            instance = new PeerInstance();
        }
    }
    [Category("SocketLayer"), Description("tests for Peer that apply to both server and client")]
    public class PeerTest : PeerTestBase
    {
        [Test]
        public void CloseShouldThrowIfNoActive()
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                peer.Close();
            });
            Assert.That(exception, Has.Message.EqualTo("Peer is not active"));
        }



        [Test]
        public void CloseShouldCallSocketClose()
        {
            // activate peer
            peer.Bind(default);
            // close peer
            peer.Close();
            socket.Received(1).Close();
        }
    }

    [Category("SocketLayer"), Description("tests for Peer that only apply to server")]
    public class PeerTestAsServer : PeerTestBase
    {
        [Test]
        public void BindShoudlCallSocketBind()
        {
            EndPoint endPoint = Substitute.For<EndPoint>();
            peer.Bind(endPoint);

            socket.Received(1).Bind(Arg.Is(endPoint));
        }

        [Test]
        public void CloseSendsDisconnectMessageToAllConnections()
        {
            EndPoint endPoint = Substitute.For<EndPoint>();
            peer.Bind(endPoint);

            Assert.Ignore("NotImplemented");
            // todo add connections

            var clientEndPoints = new EndPoint[] {
                Substitute.For<EndPoint>(),
                Substitute.For<EndPoint>() };

            peer.Close();
            byte[] expected = new byte[2]
            {
                (byte)PacketType.Command,
                (byte)Commands.Disconnect,
            };
            socket.Received(1).Send(
                Arg.Is(clientEndPoints[0]),
                Arg.Is<byte[]>(actual => actual.AreEquivalentIgnoringLength(expected)),
                Arg.Is(expected.Length)
            );
            socket.Received(1).Send(
                Arg.Is(clientEndPoints[1]),
                Arg.Is<byte[]>(actual => actual.AreEquivalentIgnoringLength(expected)),
                Arg.Is(expected.Length)
            );
        }
    }
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
            int disconnectedCalled = 0;
            peer.OnDisconnected += (conn, reason) =>
            {
                disconnectedCalled++;
            };

            EndPoint endPoint = Substitute.For<EndPoint>();
            IConnection conn = peer.Connect(endPoint);

            peer.Close();

            // wait enough time so that OnDisconnected would have been called
            yield return new WaitForSeconds(1.5f);

            Assert.That(disconnectedCalled, Is.Zero, "Disconnect should not have been called");
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
                server.testSocket.AddRemote(clients[i].testSocket);
                clients[i].testSocket.AddRemote(server.testSocket);
            }
        }

        [Test]
        public void ServerAcceptsAllClients()
        {
            server.peer.Bind(Substitute.For<EndPoint>());

            for (int i = 0; i < ClientCount; i++)
            {
                // tell client i to connect
                clients[i].peer.Connect(server.endPoint);

                server.peer.OnConnected +=;
                // run tick on server, should read packet from client i
                server.peer.Update();

                // server should then reply with connection accept
                // server should then......
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }
    }

    public class TestSocket : ISocket
    {
        struct Packet
        {
            public EndPoint endPoint;
            public byte[] data;
            public int? length;
        }

        public readonly EndPoint endPoint;
        Dictionary<EndPoint, TestSocket> remoteSockets = new Dictionary<EndPoint, TestSocket>();
        Queue<Packet> received;

        public TestSocket(EndPoint endPoint = null)
        {
            this.endPoint = endPoint ?? Substitute.For<EndPoint>();
        }
        public void AddRemote(EndPoint endPoint, TestSocket socket)
        {
            remoteSockets.Add(endPoint, socket);
        }
        public void AddRemote(TestSocket socket)
        {
            remoteSockets.Add(socket.endPoint, socket);
        }

        void ISocket.Bind(EndPoint endPoint)
        {
            //
        }

        void ISocket.Close()
        {
            //
        }

        bool ISocket.Poll()
        {
            return received.Count > 0;
        }

        void ISocket.Receive(byte[] data, ref EndPoint endPoint, out int bytesReceived)
        {
            Packet next = received.Dequeue();
            endPoint = next.endPoint;
            int length = next.length ?? next.data.Length;
            bytesReceived = length;

            Buffer.BlockCopy(next.data, 0, data, 0, length);
        }

        void ISocket.Send(EndPoint endPoint, byte[] data, int? length)
        {
            TestSocket other = remoteSockets[endPoint];
            other.received.Enqueue(new Packet
            {
                endPoint = this.endPoint,
                data = data,
                length = length
            });
        }
    }


    public class PeerInstanceWithSocket : PeerInstance
    {
        public TestSocket testSocket;
        /// <summary>
        /// endpoint that other sockets use to send to this
        /// </summary>
        public EndPoint endPoint;

        public PeerInstanceWithSocket(Config config = null) : base(config, socket: new TestSocket())
        {
            testSocket = (TestSocket)base.socket;
            endPoint = testSocket.endPoint;
        }
    }
    /// <summary>
    /// Peer and Substitute for test
    /// </summary>
    public class PeerInstance
    {
        public ISocket socket;
        public IDataHandler dataHandler;
        public Config config;
        public ILogger logger;
        public Peer peer;

        public PeerInstance(Config config = null, ISocket socket = null)
        {
            this.socket = socket ?? Substitute.For<ISocket>();
            dataHandler = Substitute.For<IDataHandler>();

            this.config = config ?? new Config()
            {
                // 1 second before "failed to connect"
                MaxConnectAttempts = 5,
                ConnectAttemptInterval = 0.2f,
            };
            logger = Substitute.For<ILogger>();
            peer = new Peer(socket, dataHandler, config, logger);
        }
    }

    public static class ArgCollection
    {
        public static bool AreEquivalentIgnoringLength<T>(this T[] actual, T[] expected) where T : IEquatable<T>
        {
            // atleast same length
            if (actual.Length < expected.Length)
            {
                Debug.LogError($"length of actual was less than expected\n" +
                    $"  actual length:{actual.Length}\n" +
                    $"  expected length:{expected.Length}");
                return false;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                if (!actual[i].Equals(expected[i]))
                {
                    Debug.LogError($"element {i} in actual was not equal to expected\n" +
                        $"  actual[{i}]:{actual[i]}\n" +
                        $"  expected[{i}]:{expected[i]}");
                    return false;
                }
            }

            return true;
        }
    }
}
