using System;
using System.Net;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.SocketLayer.Tests
{
    [Category("SocketLayer")]
    public class PeerTest
    {
        private ISocket socket;
        private IDataHandler dataHandler;
        private Config config;
        private ILogger logger;
        private Peer peer;

        [SetUp]
        public void SetUp()
        {
            socket = Substitute.For<ISocket>();
            dataHandler = Substitute.For<IDataHandler>();
            config = new Config();
            logger = Substitute.For<ILogger>();
            peer = new Peer(socket, dataHandler, config, logger);
        }

        [Test]
        public void BindShoudlCallSocketBind()
        {
            EndPoint endPoint = Substitute.For<EndPoint>();
            peer.Bind(endPoint);

            socket.Received(1).Bind(Arg.Is(endPoint));
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



        [Test]
        public void CloseShouldCallSocketClose()
        {
            // activate peer
            peer.Bind(default);
            // close peer
            peer.Close();
            socket.Received(1).Close();
        }

        [Test]
        public void CloseShouldSendDisconnectMessage()
        {
            // todo set up connected connections,
            // todo test as server with multiple connections
            // todo test as client with 1 connection
            throw new NotImplementedException("What should happen if close/disconnect is called while still connecting");

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
