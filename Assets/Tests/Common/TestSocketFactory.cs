using System;
using System.Collections.Generic;
using Mirage.SocketLayer;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.Tests
{
    public enum SocketBehavior { PollReceive, TickEvent }
    public enum ConnectionHandleBehavior { Stateful, Stateless }

    /// <summary>
    /// Socket that can send message to other sockets
    /// </summary>
    public class TestSocket : ISocket
    {
        /// <summary>
        /// this static dictionary will act as the internet
        /// </summary>
        public static Dictionary<IConnectionHandle, TestSocket> allSockets = new Dictionary<IConnectionHandle, TestSocket>();

        /// <summary>
        /// Can be useful to fake timeouts or dropped messages
        /// </summary>
        public static bool StopAllMessages;

        public static bool EndpointInUse(IConnectionHandle endPoint) => allSockets.ContainsKey(endPoint);

        /// <summary>
        /// adds this socket as an option to receive data
        /// </summary>
        private void AddThisSocket()
        {
            if (allSockets.TryGetValue(endPoint, out var value))
            {
                if (value != this)
                {
                    throw new Exception($"endpoint [{endPoint}] already exist in socket dictionary with different value");
                }
            }
            else
            {
                allSockets[endPoint] = this;
            }
        }


        /// <summary>
        /// what other instances can use too send message to the socket
        /// </summary>
        public readonly IConnectionHandle endPoint;
        private readonly Queue<Packet> received = new Queue<Packet>();
        public List<Packet> Sent = new List<Packet>();

        public readonly string name;
        private readonly SocketBehavior _behavior;
        private OnData _onData;

        public TestSocket(string name, SocketBehavior behavior, IConnectionHandle endPoint = null)
        {
            this.name = name;
            _behavior = behavior;
            this.endPoint = endPoint ?? TestEndPoint.CreateSubstitute();
        }


        void ISocket.Bind(IBindEndPoint endPoint)
        {
            AddThisSocket();
        }

        IConnectionHandle ISocket.Connect(IConnectEndPoint endPoint)
        {
            AddThisSocket();
            // return server's endpoint
            return (IConnectionHandle)endPoint;
        }

        void ISocket.Close()
        {
            allSockets.Remove(endPoint);
        }

        void ISocket.Tick()
        {
            if (_behavior == SocketBehavior.TickEvent)
            {
                while (received.TryDequeue(out var next))
                {
                    _onData.Invoke(next.endPoint, next.AsSpan());
                }
            }

        }
        void ISocket.SetTickEvents(int maxPacketSize, OnData onData, OnDisconnect onDisconnect)
        {
            if (_behavior == SocketBehavior.TickEvent)
                _onData = onData;
        }

        bool ISocket.Poll()
        {
            if (_behavior == SocketBehavior.PollReceive)
                return received.Count > 0;
            else
                return false;
        }

        int ISocket.Receive(Span<byte> outBuffer, out IConnectionHandle handle)
        {
            if (_behavior != SocketBehavior.PollReceive)
                Assert.Fail("Receive should only be called in PollReceive mode");

            var next = received.Dequeue();
            handle = next.endPoint;

            next.AsSpan().CopyTo(outBuffer);
            return next.length;
        }

        void ISocket.Send(IConnectionHandle handle, ReadOnlySpan<byte> packet)
        {
            AddThisSocket();

            if (!allSockets.TryGetValue(handle, out var other))
            {
                // other socket might have been closed
                return;
            }

            // create copy because data is from buffer
            var clone = packet.ToArray();
            Sent.Add(new Packet
            {
                endPoint = handle,
                data = clone,
                length = clone.Length
            });

            // mark as sent, but not as received
            if (StopAllMessages)
                return;

            other.received.Enqueue(new Packet
            {
                endPoint = endPoint,
                data = clone,
                length = clone.Length
            });
        }

        public struct Packet
        {
            public IConnectionHandle endPoint;
            public byte[] data;
            public int length;

            public Span<byte> AsSpan() => data.AsSpan(0, length);
        }
    }

    public abstract class MockIConnectionHandle : IConnectionHandle, IBindEndPoint, IConnectEndPoint
    {
        public bool IsStateful => true;
        public ISocketLayerConnection SocketLayerConnection { get; set; }

        public abstract bool SupportsGracefulDisconnect { get; }
        public abstract IConnectionHandle CreateCopy();
        public abstract void Disconnect(string gracefulDisconnectReason);
    }


    public static class TestEndPoint
    {
        public static IConnectionHandle CreateSubstitute(ConnectionHandleBehavior handleBehavior = ConnectionHandleBehavior.Stateless)
        {
            if (handleBehavior == ConnectionHandleBehavior.Stateful)
            {
                var endpoint = Substitute.ForPartsOf<MockIConnectionHandle>();
                return endpoint;
            }
            else
            {
                var endpoint = Substitute.For<IConnectionHandle, IBindEndPoint, IConnectEndPoint>();
                endpoint.CreateCopy().Returns(endpoint);
                return endpoint;
            }
        }
    }

    public class TestSocketFactory : SocketFactory
    {
        public SocketBehavior Behavior = SocketBehavior.TickEvent;
        public IConnectionHandle serverEndpoint = TestEndPoint.CreateSubstitute();
        private int clientNameIndex;
        private int serverNameIndex;
        public override int MaxPacketSize => 1300;
        public override ISocket CreateClientSocket()
        {
            return new TestSocket($"Client {clientNameIndex++}", Behavior);
        }

        public override ISocket CreateServerSocket()
        {
            if (TestSocket.EndpointInUse(serverEndpoint))
            {
                throw new InvalidOperationException("TestSocketFactory can only create 1 server, see comment");
                // Clients use Server endpoint to connect, so a 2nd server can't be started from the same TestSocketFactory
                // if multiple server are needed then it would require multiple instances of TestSocketFactory
            }
            return new TestSocket($"Server {serverNameIndex++}", Behavior, serverEndpoint);
        }

        public override IBindEndPoint GetBindEndPoint()
        {
            return default;
        }

        public override IConnectEndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            return (IConnectEndPoint)serverEndpoint;
        }
    }
}
