using System;
using System.Collections.Generic;
using System.Linq;
using Mirage.Logging;
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
        public const int MAX_PACKET_SIZE = 1300;

        protected byte[] connectRequest;
        private PeerInstance instance;
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

            CreateConnectPacket();
        }

        private void CreateConnectPacket()
        {
            var keyValidator = new ConnectKeyValidator(instance.config.key);
            connectRequest = new byte[2 + keyValidator.KeyLength];
            connectRequest[0] = (byte)PacketType.Command;
            connectRequest[1] = (byte)Commands.ConnectRequest;
            keyValidator.CopyTo(connectRequest);
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

        private static ISocket CreateMock()
        {
            var mock = Substitute.ForPartsOf<MockISocket>();
            mock.Connect(Arg.Any<IConnectEndPoint>()).Returns(x => x.Args()[0]);
            return mock;
        }

        public PeerInstance(Config config = null, ISocket socket = null, int? maxPacketSize = null)
        {
            this.socket = socket ?? CreateMock();
            dataHandler = Substitute.For<IDataHandler>();

            this.config = config ?? new Config()
            {
                MaxConnections = PeerTestBase.maxConnections,
                // 1 second before "failed to connect"
                MaxConnectAttempts = 5,
                ConnectAttemptInterval = 0.2f,
            };
            logger = LogFactory.GetLogger<PeerInstance>();
            peer = new Peer(this.socket, maxPacketSize ?? PeerTestBase.MAX_PACKET_SIZE, dataHandler, this.config, logger);
        }
    }

    public abstract class MockISocket : ISocket
    {
        public readonly List<(IConnectionHandle handle, byte[] packet)> SendReceiveCalls = new List<(IConnectionHandle handle, byte[] packet)>();
        public readonly Queue<(IConnectionHandle handle, byte[] data)> ReceiveQueue = new();

        // can't use NUnit for Span calls, so custom mock for this method
        public void Send(IConnectionHandle handle, ReadOnlySpan<byte> packet)
        {
            SendReceiveCalls.Add((handle, packet.ToArray()));
        }
        public int Receive(Span<byte> outBuffer, out IConnectionHandle handle)
        {
            var (h, data) = ReceiveQueue.Dequeue();
            handle = h;
            var copyLength = Math.Min(outBuffer.Length, data.Length);
            data.AsSpan(0, copyLength).CopyTo(outBuffer);
            return data.Length;
        }
        public bool Poll() => ReceiveQueue.Count > 0;

        public abstract void Bind(IBindEndPoint endPoint);
        public abstract void Close();
        public abstract IConnectionHandle Connect(IConnectEndPoint endPoint);
        public abstract void SetTickEvents(int maxPacketSize, OnData onData, OnDisconnect onDisconnect);
        public abstract void Tick();
    }
    public static class MockISocketExtension
    {
        public static MockISocket AsMock(this ISocket socket) => (MockISocket)socket;
        public static byte[] GetLastSendArray(this MockISocket socket)
        {
            return GetLastSend(socket).packet;
        }
        public static (IConnectionHandle handle, byte[] packet) GetLastSend(this MockISocket socket)
        {
            Assert.Greater(socket.SendReceiveCalls.Count, 0, "No Send calls were recorded.");
            // Return the most recent call
            return socket.SendReceiveCalls.Last();
        }

        public static void AssertSendDidNotReceive(this MockISocket socket)
        {
            Assert.AreEqual(0, socket.SendReceiveCalls.Count, $"Expected no Send calls.");
        }

        public static void AssertSendCall(this MockISocket socket, int? expectedCount = default, IConnectionHandle handle = null, int? packetLength = null, Func<byte[], bool> validateLast = null)
        {
            var matchingCount = 0;
            foreach (var call in socket.SendReceiveCalls)
            {
                if (handle != null && call.handle != handle)
                    continue;

                if (packetLength != null && call.packet.Length != packetLength.Value)
                    continue;

                if (validateLast != null && !validateLast.Invoke(call.packet))
                    continue;

                matchingCount++;
            }

            if (expectedCount.HasValue)
            {
                // Equivalent to Received(n) -> Exact match
                Assert.AreEqual(expectedCount.Value, matchingCount,
                    $"Expected exactly {expectedCount.Value} matching Send calls, but found {matchingCount}.");
            }
            else
            {
                // Equivalent to Received() -> At least one
                Assert.GreaterOrEqual(matchingCount, 1,
                    $"Expected at least 1 matching Send call, but found {matchingCount}.");
            }
        }

        public static void ClearSendAndReceivedCalls(this ISocket socket)
        {
            socket.ClearReceivedCalls();
            if (socket is MockISocket mock)
                mock.SendReceiveCalls.Clear();
        }

        public static void QueueReceiveCall(this MockISocket socket, byte[] data, IConnectionHandle endPoint, int? length = null)
        {
            Debug.Assert(endPoint != null, "QueueReceiveCall needs endpoint, use TestEndPoint.CreateSubstitute() to create mock");
            var queueEndpoint = endPoint;
            var queueArray = new byte[length ?? data.Length];
            data.AsSpan().CopyTo(queueArray.AsSpan());
            socket.ReceiveQueue.Enqueue((queueEndpoint, queueArray));
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
        public IConnectionHandle endPoint;

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

            for (var i = 0; i < expected.Length; i++)
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
