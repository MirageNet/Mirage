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
    public abstract class PeerTestBase
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
        protected readonly SocketBehavior _behavior;

        public PeerTestBase(SocketBehavior behavior)
        {
            _behavior = behavior;
        }

        [SetUp]
        public virtual void SetUp()
        {
            instance = new PeerInstance(_behavior);

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

        private static ISocket CreateMock(SocketBehavior behavior)
        {
            var mock = Substitute.ForPartsOf<MockISocket>(behavior);
            mock.Connect(Arg.Any<IConnectEndPoint>()).Returns(x => x.Args()[0]);
            return mock;
        }

        public PeerInstance(SocketBehavior behavior, Config config = null, int? maxPacketSize = null)
            : this(CreateMock(behavior), config, maxPacketSize)
        {
        }
        public PeerInstance(ISocket socket, Config config = null, int? maxPacketSize = null)
        {
            this.socket = socket;
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
        public readonly Queue<(IConnectionHandle handle, bool isDisconnect, byte[] data)> ReceiveQueue = new();
        public readonly SocketBehavior Behavior;
        private OnData _onData;
        private OnDisconnect _onDisconnect;

        public MockISocket(SocketBehavior behavior)
        {
            Behavior = behavior;
        }

        public void Send(IConnectionHandle handle, ReadOnlySpan<byte> packet)
        {
            SendReceiveCalls.Add((handle, packet.ToArray()));
        }
        public int Receive(Span<byte> outBuffer, out IConnectionHandle outHandle)
        {
            if (Behavior != SocketBehavior.PollReceive)
                Assert.Fail("Receive should only be called in PollReceive mode");

            // dont care if it is disconnect or not, just return the data, it should be the disconnect command
            var (handle, _, data) = ReceiveQueue.Dequeue();
            outHandle = handle;
            var copyLength = Math.Min(outBuffer.Length, data.Length);
            data.AsSpan(0, copyLength).CopyTo(outBuffer);
            return data.Length;
        }
        public bool Poll()
        {
            if (Behavior == SocketBehavior.PollReceive)
                return ReceiveQueue.Count > 0;
            else
                return false;
        }

        public void SetTickEvents(int maxPacketSize, OnData onData, OnDisconnect onDisconnect)
        {
            if (Behavior == SocketBehavior.TickEvent)
            {
                _onData = onData;
                _onDisconnect = onDisconnect;
            }
        }
        public void Tick()
        {
            if (Behavior == SocketBehavior.TickEvent)
            {
                while (ReceiveQueue.TryDequeue(out var next))
                {
                    var (handle, isDisconnect, data) = next;
                    if (isDisconnect)
                        _onDisconnect.Invoke(handle, data, null);
                    else
                        _onData.Invoke(handle, data);
                }
            }
        }

        public abstract void Bind(IBindEndPoint endPoint);
        public abstract void Close();
        public abstract IConnectionHandle Connect(IConnectEndPoint endPoint);
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
            socket.ReceiveQueue.Enqueue((queueEndpoint, false, queueArray));
        }
        public static void QueueDisconnectCall(this MockISocket socket, IConnectionHandle endPoint, DisconnectReason? disconnectReason = null)
        {
            Debug.Assert(endPoint != null, "QueueReceiveCall needs endpoint, use TestEndPoint.CreateSubstitute() to create mock");
            var queueEndpoint = endPoint;
            var queueArray = new byte[disconnectReason.HasValue ? 3 : 2];
            queueArray[0] = (byte)PacketType.Command;
            queueArray[1] = (byte)Commands.Disconnect;
            if (disconnectReason.HasValue)
                queueArray[2] = (byte)disconnectReason.Value;
            socket.ReceiveQueue.Enqueue((queueEndpoint, true, queueArray));
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

        public PeerInstanceWithSocket(SocketBehavior behavior, Config config = null) : base(new TestSocket("TestInstance", behavior), config)
        {
            socket = (TestSocket)base.socket;
            endPoint = socket.endPoint;
        }
    }

    public static class ArgCollection
    {
        public static bool AreEquivalentIgnoringLength<T>(this T[] actual, T[] expected) where T : IEquatable<T>
        {
            return AreEquivalentIgnoringLength<T>(actual.AsSpan(), expected.AsSpan());
        }

        public static bool AreEquivalentIgnoringLength<T>(this ArraySegment<T> actual, ArraySegment<T> expected) where T : IEquatable<T>
        {
            return AreEquivalentIgnoringLength<T>(actual.AsSpan(), expected.AsSpan());
        }

        public static bool AreEquivalentIgnoringLength<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> expected) where T : IEquatable<T>
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
