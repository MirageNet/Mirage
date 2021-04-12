using System;
using System.Collections.Generic;
using System.Net;
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

    /// <summary>
    /// Socket that can send message to other sockets
    /// </summary>
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
                MaxConnections = 5,
                // 1 second before "failed to connect"
                MaxConnectAttempts = 5,
                ConnectAttemptInterval = 0.2f,
            };
            logger = Substitute.For<ILogger>();
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
        public EndPoint endPoint;

        public PeerInstanceWithSocket(Config config = null) : base(config, socket: new TestSocket())
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

        public static void SetupRecieveCall(this ISocket socket, byte[] data, EndPoint endPoint = null)
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
}
