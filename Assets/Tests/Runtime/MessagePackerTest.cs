using System;
using System.Collections.Generic;
using System.IO;
using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    [TestFixture]
    public class MessagePackerTest
    {
        NetworkReader reader;
        [SetUp]
        public void Setup()
        {
            reader = new NetworkReader();
        }
        [TearDown]
        public void TearDown()
        {
            reader.Dispose();
            reader = null;
        }


        [Test]
        public void TestPacking()
        {
            var message = new SceneMessage
            {
                MainActivateScene = "Hello world",
                SceneOperation = SceneOperation.LoadAdditive
            };

            byte[] data = MessagePacker.Pack(message);

            SceneMessage unpacked = MessagePacker.Unpack<SceneMessage>(data);

            Assert.That(unpacked.MainActivateScene, Is.EqualTo("Hello world"));
            Assert.That(unpacked.SceneOperation, Is.EqualTo(SceneOperation.LoadAdditive));
        }

        [Test]
        public void UnpackWrongMessage()
        {
            var message = new SceneReadyMessage();

            byte[] data = MessagePacker.Pack(message);

            Assert.Throws<FormatException>(() =>
            {
                _ = MessagePacker.Unpack<AddCharacterMessage>(data);
            });
        }

        [Test]
        public void TestUnpackIdMismatch()
        {
            // Unpack<T> has a id != msgType case that throws a FormatException.
            // let's try to trigger it.

            var message = new SceneMessage
            {
                MainActivateScene = "Hello world",
                SceneOperation = SceneOperation.LoadAdditive
            };

            byte[] data = MessagePacker.Pack(message);

            // overwrite the id
            data[0] = 0x01;
            data[1] = 0x02;

            Assert.Throws<FormatException>(delegate
            {
                _ = MessagePacker.Unpack<SceneMessage>(data);

            });
        }

        [Test]
        public void TestUnpackMessageNonGeneric()
        {
            // try a regular message
            var message = new SceneMessage
            {
                MainActivateScene = "Hello world",
                SceneOperation = SceneOperation.LoadAdditive
            };

            byte[] data = MessagePacker.Pack(message);
            reader.Reset(data);

            int msgType = MessagePacker.UnpackId(reader);
            Assert.That(msgType, Is.EqualTo(BitConverter.ToUInt16(data, 0)));
        }

        [Test]
        public void UnpackInvalidMessage()
        {
            // try an invalid message
            Assert.Throws<EndOfStreamException>(() =>
            {
                reader.Reset(new byte[0]);
                _ = MessagePacker.UnpackId(reader);
            });
        }

        struct SomeRandomMessage { };

        [Test]
        public void RegisterMessage()
        {
            MessagePacker.RegisterMessage<SomeRandomMessage>();

            int id = MessagePacker.GetId<SomeRandomMessage>();

            Type type = MessagePacker.GetMessageType(id);

            Assert.That(type, Is.EqualTo(typeof(SomeRandomMessage)));
        }

        // these 2 messages have a colliding message id
        struct SomeRandomMessage2121143 { };
        struct SomeRandomMessage2133122 { };

        [Test]
        public void RegisterMessage2()
        {
            MessagePacker.RegisterMessage<SomeRandomMessage2121143>();
            Assert.Throws<ArgumentException>(() =>
            {
                MessagePacker.RegisterMessage<SomeRandomMessage2133122>();
            });
        }

        [Test]
        public void FindSystemMessage()
        {
            int id = MessagePacker.GetId<SceneMessage>();
            Type type = MessagePacker.GetMessageType(id);
            Assert.That(type, Is.EqualTo(typeof(SceneMessage)));
        }

        struct SomeRandomMessageNotRegistered { };
        [Test]
        public void FindUnknownMessage()
        {
            // note that GetId<> will cause the weaver to register it
            // but GetId() will not
            int id = MessagePacker.GetId(typeof(SomeRandomMessageNotRegistered));
            Assert.Throws<KeyNotFoundException>(() =>
            {
                _ = MessagePacker.GetMessageType(id);
            });
        }
    }
}
