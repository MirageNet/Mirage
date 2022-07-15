using System;
using Mirage.Logging;
using Mirage.Serialization;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
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

            messageHandler = new MessageHandler(null, true);
        }


        [Test]
        public void InvokesMessageHandler()
        {
            var invoked = 0;
            messageHandler.RegisterHandler<SceneReadyMessage>(_ => { invoked++; });

            var messageId = MessagePacker.GetId<SceneReadyMessage>();
            messageHandler.InvokeHandler(player, messageId, reader);

            Assert.That(invoked, Is.EqualTo(1), "Should have been invoked");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void DisconnectsIfHandlerHasException(bool disconnectOnThrow)
        {
            messageHandler = new MessageHandler(null, disconnectOnThrow);

            var invoked = 0;
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
        public void LogsWhenNoHandlerIsFound()
        {
            ExpectLog(() =>
            {
                var messageId = MessagePacker.GetId<SceneMessage>();
                messageHandler.InvokeHandler(player, messageId, reader);
            }
            , LogType.Warning, $"Unexpected message {typeof(SceneMessage)} received from {player}. Did you register a handler for it?");
        }

        [Test]
        public void LogsWhenUnknownMessage()
        {
            const int id = 1234;
            ExpectLog(() =>
            {
                messageHandler.InvokeHandler(player, id, reader);
            }
            , LogType.Log, $"Unexpected message ID {id} received from {player}. May be due to no existing RegisterHandler for this message.");
        }

        private void ExpectLog(Action action, LogType type, string log)
        {
            var logger = LogFactory.GetLogger(typeof(MessageHandler));
            var existing = logger.logHandler;
            var existingLevel = logger.filterLogType;
            try
            {
                var handler = Substitute.For<ILogHandler>();
                logger.logHandler = handler;
                logger.filterLogType = LogType.Log;

                action.Invoke();

                handler.Received(1).LogFormat(type, null, "{0}", log);
            }
            finally
            {
                logger.logHandler = existing;
                logger.filterLogType = existingLevel;
            }
        }
    }
}
