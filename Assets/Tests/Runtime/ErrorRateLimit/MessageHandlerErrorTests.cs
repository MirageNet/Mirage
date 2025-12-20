using System;
using System.Text.RegularExpressions;
using Mirage.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ErrorRateLimit
{
    [TestFixture]
    public class MessageHandlerErrorTests : ClientServerSetup
    {
        protected override void ExtraServerSetup()
        {
            server.RethrowException = false;
        }

        [Test]
        public void HandleExceptionInReaderTest()
        {
            // register our new type that has a reader that throws
            var writer = new NetworkWriter(1300);
            var reader = new NetworkReader();
            Reader<ReadThrowMessage>.Read = (r) => { throw new DeserializeFailedException("test reason"); };
            MessagePacker.RegisterMessage<ReadThrowMessage>();


            var invoked = 0;
            server.MessageHandler.RegisterHandler<ReadThrowMessage>((player, msg) => { invoked++; });

            var message = new ReadThrowMessage();
            var packed = MessagePacker.Pack(message);

            // throw will log it twice
            LogAssert.Expect(LogType.Error, new Regex(".*Mirage.DeserializeFailedException in NetworkReader.*"));
            server.MessageHandler.HandleMessage(serverPlayer, packed);

            Assert.That(invoked, Is.EqualTo(0));
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.DeserializationException));
        }

        [Test]
        public void HandleExceptionInMessageTest()
        {
            var invoked = 0;
            server.MessageHandler.RegisterHandler<SceneReadyMessage>((player, msg) =>
            {
                invoked++;
                throw new Exception("Test Exception");
            });

            var packed = MessagePacker.Pack(new SceneReadyMessage());

            LogAssert.Expect(LogType.Error, new Regex(".*System.Exception in Message handler.*"));
            server.MessageHandler.HandleMessage(serverPlayer, packed);

            Assert.That(invoked, Is.EqualTo(1));
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.RpcException));
        }

        [Test]
        public void ShortMessagePayloadTest()
        {
            var message = new ArraySegment<byte>(new byte[] { 1 });

            server.MessageHandler.HandleMessage(serverPlayer, message);
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.DeserializationException));
        }

        [Test]
        public void UnregisteredMessageTest()
        {
            var packed = MessagePacker.Pack(new AddCharacterMessage());

            LogAssert.Expect(LogType.Warning, new Regex(".*Unexpected message Mirage.AddCharacterMessage.*"));
            server.MessageHandler.HandleMessage(serverPlayer, packed);

            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.RpcSync));
        }

        [NetworkMessage]
        public struct ReadThrowMessage { public int value; }
    }
}
