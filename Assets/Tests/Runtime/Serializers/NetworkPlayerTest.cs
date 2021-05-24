using System;
using System.IO;
using System.Linq;
using Mirage.Serialization;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.Tests.Runtime
{
    public class NetworkPlayerMessageSendingTest
    {
        private NetworkPlayer player;
        private SocketLayer.IConnection connection;

        [SetUp]
        public void SetUp()
        {
            connection = Substitute.For<SocketLayer.IConnection>();
            player = new NetworkPlayer(connection);
        }

        [Test]
        [TestCase(Channel.Reliable)]
        [TestCase(Channel.Unreliable)]
        public void SendCallsSendOnConnection(int channel)
        {
            byte[] message = new byte[] { 0, 1, 2 };
            player.Send(new ArraySegment<byte>(message), channel);
            if (channel == Channel.Reliable)
            {
                connection.Received(1).SendReliable(Arg.Is<byte[]>(arg => arg.SequenceEqual(message)));
            }
            else if (channel == Channel.Unreliable)
            {
                connection.Received(1).SendUnreliable(Arg.Is<byte[]>(arg => arg.SequenceEqual(message)));
            }
        }

        [Test]
        public void DisconnectCallsDisconnectOnConnection()
        {
            player.Disconnect();
            connection.Received(1).Disconnect();
        }

        [Test]
        public void DisconnectStopsMessagesBeingSentToConnection()
        {
            player.Disconnect();
            player.Send(new ArraySegment<byte>(new byte[] { 0, 1, 2 }));
            connection.DidNotReceive().SendReliable(Arg.Any<byte[]>());
            connection.DidNotReceive().SendUnreliable(Arg.Any<byte[]>());
        }
        [Test]
        public void MarkAsDisconnectedStopsMessagesBeingSentToConnection()
        {
            player.MarkAsDisconnected();
            player.Send(new ArraySegment<byte>(new byte[] { 0, 1, 2 }));
            connection.DidNotReceive().SendReliable(Arg.Any<byte[]>());
            connection.DidNotReceive().SendUnreliable(Arg.Any<byte[]>());
        }
    }
    public class NetworkPlayerMessageHandlingTest
    {
        private NetworkPlayer player;
        private SocketLayer.IConnection connection;

        [SetUp]
        public void SetUp()
        {
            connection = Substitute.For<SocketLayer.IConnection>();
            player = new NetworkPlayer(connection);
        }


        [Test]
        public void DisconnectsIfHandlerHasException()
        {
            // todo add handler that throws, cause it to be invoked, and see if disconnect is called
            Assert.Ignore("NotImplemented");
        }
        [Test]
        public void InvokesMessageHandler()
        {
            // todo add handler, cause it to be invoked, check it was invoked
            Assert.Ignore("NotImplemented");
        }

        [Test]
        public void ThrowsWhenNoHandlerIsFound()
        {
            int messageId = MessagePacker.GetId<SceneMessage>();
            var reader = new NetworkReader(new byte[] { 1, 2, 3, 4 });
            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
            {
                player.InvokeHandler(messageId, reader, 0);
            });

            Assert.That(exception.Message, Does.StartWith("Unexpected message Mirage.SceneMessage received"));
        }

        [Test]
        public void ThrowsWhenUnknownMessage()
        {
            _ = MessagePacker.GetId<SceneMessage>();
            var reader = new NetworkReader(new byte[] { 1, 2, 3, 4 });
            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
            {
                // some random id with no message
                player.InvokeHandler(1234, reader, 0);
            });

            Assert.That(exception.Message, Does.StartWith("Unexpected message ID 1234 received"));
        }
    }
}
