using System;
using System.IO;
using Mirage.Serialization;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime
{
    public class MessageHandlerTest
    {
        private NetworkPlayer player;
        private NetworkReader reader;
        private SocketLayer.IConnection connection;
        private MessageHandler messageHandler;

        [SetUp]
        public void SetUp()
        {
            connection = Substitute.For<SocketLayer.IConnection>();
            player = new NetworkPlayer(connection);
            // reader with some random data
            reader = new NetworkReader();
            reader.Reset(new byte[] { 1, 2, 3, 4 });

            messageHandler = new MessageHandler(true);
        }


        [Test]
        public void InvokesMessageHandler()
        {
            int invoked = 0;
            messageHandler.RegisterHandler<SceneReadyMessage>(_ => { invoked++; });

            int messageId = MessagePacker.GetId<SceneReadyMessage>();
            messageHandler.InvokeHandler(player, messageId, reader);

            Assert.That(invoked, Is.EqualTo(1), "Should have been invoked");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void DisconnectsIfHandlerHasException(bool disconnectOnThrow)
        {
            messageHandler = new MessageHandler(disconnectOnThrow);

            int invoked = 0;
            messageHandler.RegisterHandler<SceneReadyMessage>(_ => { invoked++; throw new InvalidOperationException("Fun Exception"); });

            var packet = new ArraySegment<byte>(MessagePacker.Pack(new SceneReadyMessage()));
            LogAssert.ignoreFailingMessages = true;
            Assert.DoesNotThrow(() =>
            {
                messageHandler.HandleMessage(player, packet);
            });
            LogAssert.ignoreFailingMessages = false;

            Assert.That(invoked, Is.EqualTo(1), "Should have been invoked");

            if (disconnectOnThrow)
            {
                connection.Received(1).Disconnect();
            }
            else
            {
                connection.DidNotReceive().Disconnect();
            }
        }

        [Test]
        public void ThrowsWhenNoHandlerIsFound()
        {
            int messageId = MessagePacker.GetId<SceneMessage>();

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
            {
                messageHandler.InvokeHandler(player, messageId, reader);
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
                messageHandler.InvokeHandler(player, 1234, reader);
            });

            Assert.That(exception.Message, Does.StartWith("Unexpected message ID 1234 received"));
        }
    }
}
