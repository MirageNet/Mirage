using System;
using System.IO;
using System.Linq;
using Mirage.Serialization;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

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
                connection.Received(1).SendReliable(Arg.Is<ArraySegment<byte>>(arg => arg.SequenceEqual(message)));
            }
            else if (channel == Channel.Unreliable)
            {
                connection.Received(1).SendUnreliable(Arg.Is<ArraySegment<byte>>(arg => arg.SequenceEqual(message)));
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
        private NetworkReader reader;
        private SocketLayer.IConnection connection;

        [SetUp]
        public void SetUp()
        {
            connection = Substitute.For<SocketLayer.IConnection>();
            player = new NetworkPlayer(connection);
            // reader with some random data
            reader = new NetworkReader();
            reader.Reset(new byte[] { 1, 2, 3, 4 });
        }


        [Test]
        public void InvokesMessageHandler()
        {
            int invoked = 0;
            player.RegisterHandler<ReadyMessage>(_ => { invoked++; });

            int messageId = MessagePacker.GetId<ReadyMessage>();
            player.InvokeHandler(messageId, reader);

            Assert.That(invoked, Is.EqualTo(1), "Should have been invoked");
        }

        [Test]
        public void DisconnectsIfHandlerHasException()
        {
            int invoked = 0;
            player.RegisterHandler<ReadyMessage>(_ => { invoked++; throw new InvalidOperationException("Fun Exception"); });

            var packet = new ArraySegment<byte>(MessagePacker.Pack(new ReadyMessage()));
            LogAssert.ignoreFailingMessages = true;
            Assert.DoesNotThrow(() =>
            {
                ((IMessageHandler)player).HandleMessage(packet);
            });
            LogAssert.ignoreFailingMessages = false;

            Assert.That(invoked, Is.EqualTo(1), "Should have been invoked");

            // should disconnect after catching the execption
            connection.Received(1).Disconnect();
        }

        [Test]
        public void ThrowsWhenNoHandlerIsFound()
        {
            int messageId = MessagePacker.GetId<SceneMessage>();

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
            {
                player.InvokeHandler(messageId, reader);
            });

            Assert.That(exception.Message, Does.StartWith("Unexpected message Mirage.SceneMessage received"));
        }

        [Test]
        public void ThrowsWhenUnknownMessage()
        {
            _ = MessagePacker.GetId<SceneMessage>();
            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
            {
                // some random id with no message
                player.InvokeHandler(1234, reader);
            });

            Assert.That(exception.Message, Does.StartWith("Unexpected message ID 1234 received"));
        }
    }
}
