using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public void ThrowIfSocketIsNull()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            {
                new Peer(null, Substitute.For<IDataHandler>(), new Config(), Substitute.For<ILogger>());
            });
            var expected = new ArgumentNullException("socket");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
        [Test]
        public void ThrowIfDataHandlerIsNull()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            {
                new Peer(Substitute.For<ISocket>(), null, new Config(), Substitute.For<ILogger>());
            });
            var expected = new ArgumentNullException("dataHandler");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
        [Test]
        public void DoesNotThrowIfConfigIsNull()
        {
            Assert.DoesNotThrow(() =>
            {
                new Peer(Substitute.For<ISocket>(), Substitute.For<IDataHandler>(), null, Substitute.For<ILogger>());
            });
        }
        [Test]
        public void DoesNotThrowIfLoggerIsNull()
        {
            Assert.DoesNotThrow(() =>
            {
                new Peer(Substitute.For<ISocket>(), Substitute.For<IDataHandler>(), new Config(), null);
            });
        }

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

        [Test]
        public void AcceptsForValidMessage()
        {
            peer.Bind(Substitute.For<EndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            var validator = new ConnectKeyValidator();
            byte[] valid = new byte[3]
            {
                (byte)PacketType.Command,
                (byte)Commands.ConnectRequest,
                0
            };
            validator.CopyTo(valid);
            EndPoint endPoint = Substitute.For<EndPoint>();
            SetupRecieveCall(socket, valid, endPoint);
            peer.Update();

            // server sends accept and invokes event locally
            socket.Received(1).Send(endPoint, Arg.Is<byte[]>(x =>
                x.Length >= 2 &&
                x[0] == (byte)PacketType.Command &&
                x[1] == (byte)Commands.ConnectionAccepted
            ), 2);
            connectAction.ReceivedWithAnyArgs(1).Invoke(default);
        }

        [Test]
        public void AcceptsUpToMaxConnections()
        {
            peer.Bind(Substitute.For<EndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            var validator = new ConnectKeyValidator();
            byte[] valid = new byte[3]
            {
                (byte)PacketType.Command,
                (byte)Commands.ConnectRequest,
                0
            };
            validator.CopyTo(valid);
            const int maxConnections = 5;
            var endPoints = new EndPoint[maxConnections];
            for (int i = 0; i < maxConnections; i++)
            {
                endPoints[i] = Substitute.For<EndPoint>();

                SetupRecieveCall(socket, valid, endPoints[i]);
                peer.Update();
            }


            // server sends accept and invokes event locally
            connectAction.ReceivedWithAnyArgs(maxConnections).Invoke(default);
            for (int i = 0; i < maxConnections; i++)
            {
                socket.Received(1).Send(endPoints[i], Arg.Is<byte[]>(x =>
                    x.Length >= 2 &&
                    x[0] == (byte)PacketType.Command &&
                    x[1] == (byte)Commands.ConnectionAccepted
                ), 2);
            }
        }

        [Test]
        public void RejectsConnectionOverMax()
        {
            peer.Bind(Substitute.For<EndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            var validator = new ConnectKeyValidator();
            byte[] valid = new byte[3]
            {
                (byte)PacketType.Command,
                (byte)Commands.ConnectRequest,
                validator.GetKey(),
            };


            const int maxConnections = 5;
            for (int i = 0; i < maxConnections; i++)
            {
                SetupRecieveCall(socket, valid);
                peer.Update();
            }

            // clear calls from valid connections
            socket.ClearReceivedCalls();
            connectAction.ClearReceivedCalls();

            EndPoint overMaxEndpoint = Substitute.For<EndPoint>();
            SetupRecieveCall(socket, valid, overMaxEndpoint);


            byte[] received = null;
            socket.WhenForAnyArgs(x => x.Send(default, default, default)).Do(x =>
            {
                received = (byte[])x[1];
            });

            peer.Update();

            Debug.Log($"Length:{received.Length} [{received[0]},{received[1]},{received[2]}]");
            const int length = 3;
            socket.Received(1).Send(overMaxEndpoint, Arg.Is<byte[]>(x =>
                x.Length >= length &&
                x[0] == (byte)PacketType.Command &&
                x[1] == (byte)Commands.ConnectionRejected &&
                x[2] == (byte)RejectReason.ServerFull
            ), length);
            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test, Description("Should reject with no reason given")]
        public void IgnoresMessageThatIsInvalid()
        {
            peer.Bind(Substitute.For<EndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            SetupRecieveCall(socket, new byte[2] {
                (byte)PacketType.Command,
                (byte)Commands.ConnectRequest
            });
            peer.Update();

            // server does nothing for invalid
            socket.DidNotReceiveWithAnyArgs().Send(default, default, default);
            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test]
        public void IgnoresMessageThatIsTooShort()
        {
            peer.Bind(Substitute.For<EndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            SetupRecieveCall(socket, new byte[1] {
                (byte)UnityEngine.Random.Range(0, 255),
            });

            peer.Update();

            // server does nothing for invalid
            socket.DidNotReceiveWithAnyArgs().Send(default, default, default);
            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test]
        public void IgnoresMessageThatIsTooLong()
        {
            peer.Bind(Substitute.For<EndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            const int aboveMTU = 5000;
            SetupRecieveCall(socket, new byte[aboveMTU]);

            peer.Update();

            // server does nothing for invalid
            socket.DidNotReceiveWithAnyArgs().Send(default, default, default);
            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test]
        [Repeat(10)]
        public void IgnoresRandomData()
        {
            peer.Bind(Substitute.For<EndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            EndPoint endPoint = Substitute.For<EndPoint>();

            // 2 is min length of a message
            byte[] randomData = new byte[UnityEngine.Random.Range(2, 20)];
            for (int i = 0; i < randomData.Length; i++)
            {
                randomData[i] = (byte)UnityEngine.Random.Range(0, 255);
            }
            SetupRecieveCall(socket, randomData);

            peer.Update();

            // server does nothing for invalid
            socket.DidNotReceiveWithAnyArgs().Send(default, default, default);
            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        void SetupRecieveCall(ISocket socket, byte[] data, EndPoint endPoint = null)
        {
            socket.Poll().Returns(true, false);
            socket
               // when any call
               .When(x => x.Receive(Arg.Any<byte[]>(), ref Arg.Any<EndPoint>(), out Arg.Any<int>()))
               // return the data from endpoint
               .Do(x =>
               {
                   byte[] dataArg = (byte[])x[0];
                   for (int i = 0; i < data.Length; i++)
                   {
                       dataArg[i] = data[i];
                   }
                   x[1] = endPoint ?? Substitute.For<EndPoint>();
                   x[2] = data.Length;
               });
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

    public class TestSocket : ISocket
    {
        public struct Packet
        {
            public EndPoint endPoint;
            public byte[] data;
            public int? length;
        }

        public readonly EndPoint endPoint;
        Dictionary<EndPoint, TestSocket> remoteSockets = new Dictionary<EndPoint, TestSocket>();
        Queue<Packet> received = new Queue<Packet>();
        public List<Packet> Sent = new List<Packet>();

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

        void ISocket.Send(EndPoint remoteEndPoint, byte[] data, int? length)
        {
            TestSocket other = remoteSockets[remoteEndPoint];
            Sent.Add(new Packet
            {
                endPoint = remoteEndPoint,
                data = data,
                length = length
            });
            other.received.Enqueue(new Packet
            {
                endPoint = endPoint,
                data = data,
                length = length
            });
        }
    }


    public class PeerInstanceWithSocket : PeerInstance
    {
        public new TestSocket socket;
        /// <summary>
        /// endpoint that other sockets use to send to this
        /// </summary>
        public EndPoint endPoint;

        public PeerInstanceWithSocket(Config config = null) : base(config, socket: new TestSocket())
        {
            socket = (TestSocket)base.socket;
            endPoint = socket.endPoint;
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
                MaxConnections = 5,
                // 1 second before "failed to connect"
                MaxConnectAttempts = 5,
                ConnectAttemptInterval = 0.2f,
            };
            logger = Substitute.For<ILogger>();
            peer = new Peer(this.socket, dataHandler, this.config, logger);
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
