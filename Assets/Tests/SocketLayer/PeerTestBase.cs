using System;
using Mirage.Tests;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    /// <summary>
    /// base class of PeerTests that has setup
    /// </summary>
    public class PeerTestBase
    {
        public const int maxConnections = 5;
        protected static readonly byte[] connectRequest = new byte[3]
        {
            (byte)PacketType.Command,
            (byte)Commands.ConnectRequest,
            new ConnectKeyValidator().GetKey(),
        };

        PeerInstance instance;
        protected Action<IConnection> connectAction;
        protected Action<IConnection, RejectReason> connectFailedAction;
        protected Action<IConnection, DisconnectReason> disconnectAction;

        // helper properties to access instance
        protected ISocket socket => instance.socket;
        protected IDataHandler dataHandler => instance.dataHandler;
        protected Config config => instance.config;
        protected ILogger logger => instance.logger;
        protected Peer peer => instance.peer;

        internal readonly Time time = new Time();

        [SetUp]
        public void SetUp()
        {
            instance = new PeerInstance();

            connectAction = Substitute.For<Action<IConnection>>();
            connectFailedAction = Substitute.For<Action<IConnection, RejectReason>>();
            disconnectAction = Substitute.For<Action<IConnection, DisconnectReason>>();
            peer.OnConnected += connectAction;
            peer.OnConnectionFailed += connectFailedAction;
            peer.OnDisconnected += disconnectAction;
        }
    }

    /// <summary>
    /// Peer and Substitutes for test
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
                MaxConnections = PeerTestBase.maxConnections,
                // 1 second before "failed to connect"
                MaxConnectAttempts = 5,
                ConnectAttemptInterval = 0.2f,
            };
            logger = Debug.unityLogger;
            peer = new Peer(this.socket, dataHandler, this.config, logger);
        }
    }

    /// <summary>
    /// Peer and Substitutes for testing but with TestSocket
    /// </summary>
    public class PeerInstanceWithSocket : PeerInstance
    {
        public new TestSocket socket;
        /// <summary>
        /// endpoint that other sockets use to send to this
        /// </summary>
        public IEndPoint endPoint;

        public PeerInstanceWithSocket(Config config = null) : base(config, socket: new TestSocket("TestInstance"))
        {
            socket = (TestSocket)base.socket;
            endPoint = socket.endPoint;
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

        public static void SetupReceiveCall(this ISocket socket, byte[] data, IEndPoint endPoint = null, int? length = null)
        {
            socket.Poll().Returns(true, false);
            socket
               // when any call
               .When(x => x.Receive(Arg.Any<byte[]>(), out Arg.Any<IEndPoint>()))
               // return the data from endpoint
               .Do(x =>
               {
                   byte[] dataArg = (byte[])x[0];
                   for (int i = 0; i < data.Length; i++)
                   {
                       dataArg[i] = data[i];
                   }
                   x[1] = endPoint ?? Substitute.For<IEndPoint>();
               });
            socket.Receive(Arg.Any<byte[]>(), out Arg.Any<IEndPoint>()).Returns(length ?? data.Length);
        }
    }
}
