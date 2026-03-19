using System;
using Mirage.Serialization;
using Mirage.Serialization.BrotliCompression;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    [TestFixture]
    public class StringStoreTest
    {
        [Test]
        public void BasicLookupTest()
        {
            var store = new StringStore();
            var index1 = store.GetKey("hello");
            var index2 = store.GetKey("world");
            var index3 = store.GetKey("hello");

            Assert.That(index1, Is.EqualTo(0));
            Assert.That(index2, Is.EqualTo(1));
            Assert.That(index3, Is.EqualTo(index1));
            Assert.That(store.Strings.Count, Is.EqualTo(2));
        }

        [Test]
        public void WriteReadStringTest()
        {
            var store = new StringStore();
            var writer = new NetworkWriter(1300);

            store.WriteString(writer, "apple");
            store.WriteString(writer, "banana");
            store.WriteString(writer, "apple"); // Duplicate
            store.WriteString(writer, null);

            var reader = new NetworkReader();
            reader.Reset(writer.ToArraySegment());

            Assert.That(store.ReadString(reader), Is.EqualTo("apple"));
            Assert.That(store.ReadString(reader), Is.EqualTo("banana"));
            Assert.That(store.ReadString(reader), Is.EqualTo("apple"));
            Assert.That(store.ReadString(reader), Is.Null);
        }

        [Test]
        public void NullShouldNotBeAddedToStrinStore()
        {
            var store = new StringStore();
            var writer = new NetworkWriter(1300);

            store.WriteString(writer, null);
            store.WriteString(writer, null);
            Assert.That(store.Strings, Is.Empty);

            var reader = new NetworkReader();
            reader.Reset(writer.ToArraySegment());

            Assert.That(store.ReadString(reader), Is.Null);
            Assert.That(store.ReadString(reader), Is.Null);
        }

        [Test]
        public void WriteReadStringStoreExtensionTest()
        {
            var store = new StringStore();
            store.GetKey("one");
            store.GetKey("two");
            store.GetKey("three");

            var writer = new NetworkWriter(1300);
            writer.WriteStringStore(store);

            var reader = new NetworkReader();
            reader.Reset(writer.ToArraySegment());
            var readStore = reader.ReadStringStore();

            Assert.That(readStore.Strings.Count, Is.EqualTo(3));
            Assert.That(readStore.Strings[0], Is.EqualTo("one"));
            Assert.That(readStore.Strings[1], Is.EqualTo("two"));
            Assert.That(readStore.Strings[2], Is.EqualTo("three"));
        }

        [Test]
        public void BrotliEncoderDecoderBasicTest()
        {
            // test setup
            var receiver = (IMessageReceiver)new MessageHandler(null, false, true);
            var player = Substitute.For<INetworkPlayer>();
            player.IsAuthenticated.Returns(true);

            player.When(p => p.Send(Arg.Any<ArraySegment<byte>>(), Arg.Any<Channel>()))
                .Do(info =>
                {
                    var seg = info.Arg<ArraySegment<byte>>();
                    receiver.HandleMessage(player, seg);
                });


            // create store
            var store = new StringStore();
            for (var i = 0; i < 100; i++)
            {
                store.GetKey($"String_{i % 10}"); // 10 unique strings repeated 10 times
            }

            // create encoder/decoder
            var encoder = StringStoreBrotliEncoder.Encode(store);
            var decoder = new StringStoreBrotliDecoder(receiver);

            // send data
            encoder.Send(player);

            // check decoder as data
            Assert.That(decoder.StringStore, Is.Not.Null);
            Assert.That(decoder.StringStore.Strings.Count, Is.EqualTo(store.Strings.Count));
            for (var i = 0; i < store.Strings.Count; i++)
            {
                Assert.That(decoder.StringStore.Strings[i], Is.EqualTo(store.Strings[i]));
            }
        }

        [Test]
        public void BrotliEncoderDecoderTest()
        {
            var store = new StringStore();
            for (var i = 0; i < 100; i++)
            {
                store.GetKey($"String_{i % 10}"); // 10 unique strings repeated 10 times
            }

            // Encode
            var encoder = StringStoreBrotliEncoder.Encode(store);

            // Mock player to capture sent messages
            var player = Substitute.For<INetworkPlayer>();
            ArraySegment<byte> lengthsSegment = default;
            ArraySegment<byte> stringsSegment = default;

            player.When(p => p.Send(Arg.Any<ArraySegment<byte>>(), Arg.Any<Channel>()))
                  .Do(info =>
                  {
                      var seg = info.Arg<ArraySegment<byte>>();
                      // We expect lengths first, then strings
                      if (lengthsSegment == default)
                          lengthsSegment = seg;
                      else
                          stringsSegment = seg;
                  });

            encoder.Send(player);

            Assert.That(lengthsSegment, Is.Not.EqualTo(default(ArraySegment<byte>)));
            Assert.That(stringsSegment, Is.Not.EqualTo(default(ArraySegment<byte>)));

            // Decode
            var receiver = Substitute.For<IMessageReceiver>();

            Action<StringStoreLengthsMessage> lengthsHandler = null;
            Action<StringStoreStringsMessage> stringsHandler = null;

            receiver.When(r => r.RegisterHandler(Arg.Any<MessageDelegateWithPlayer<StringStoreLengthsMessage>>(), Arg.Any<bool>()))
                    .Do(info =>
                    {
                        var del = info.Arg<MessageDelegateWithPlayer<StringStoreLengthsMessage>>();
                        lengthsHandler = (msg) => del(null, msg);
                    });
            receiver.When(r => r.RegisterHandler(Arg.Any<MessageDelegateWithPlayer<StringStoreStringsMessage>>(), Arg.Any<bool>()))
                    .Do(info =>
                    {
                        var del = info.Arg<MessageDelegateWithPlayer<StringStoreStringsMessage>>();
                        stringsHandler = (msg) => del(null, msg);
                    });

            var decoder = new StringStoreBrotliDecoder(receiver);
            // Decoder registers lengths handler on init
            Assert.That(lengthsHandler, Is.Not.Null);

            // Simulate receiving lengths
            var lengthsReader = new NetworkReader();
            lengthsReader.Reset(lengthsSegment);
            lengthsReader.ReadUInt16(); // skip id
            var lengthsMsg = lengthsReader.ReadStringStoreLengthsMessage();
            lengthsHandler(lengthsMsg);

            // Decoder should have unregistered lengths and registered strings
            receiver.Received().UnregisterHandler<StringStoreLengthsMessage>();
            Assert.That(stringsHandler, Is.Not.Null);

            // Simulate receiving strings
            var stringsReader = new NetworkReader();
            stringsReader.Reset(stringsSegment);
            stringsReader.ReadUInt16(); // skip id
            var stringsMsg = stringsReader.ReadStringStoreStringsMessage();

            var receivedEventCalled = false;
            decoder.OnReceived += () => receivedEventCalled = true;

            stringsHandler(stringsMsg);

            // Verify
            Assert.That(receivedEventCalled, Is.True);
            Assert.That(decoder.StringStore, Is.Not.Null);
            Assert.That(decoder.StringStore.Strings.Count, Is.EqualTo(store.Strings.Count));
            for (var i = 0; i < store.Strings.Count; i++)
            {
                Assert.That(decoder.StringStore.Strings[i], Is.EqualTo(store.Strings[i]));
            }
        }

        [Test]
        public void WriterAndReaderExtensionsUseAssignedStringStore()
        {
            var store = new StringStore();
            var writer = new NetworkWriter(1300);
            writer.StringStore = store;

            // These should use StringStore instead of raw encoding
            writer.WriteString("apple");
            writer.WriteString("banana");
            writer.WriteString("apple"); // Duplicate
            writer.WriteString(null);

            // Verify StringStore captured them
            Assert.That(store.Strings.Count, Is.EqualTo(2));

            var reader = new NetworkReader();
            reader.Reset(writer.ToArraySegment());
            reader.StringStore = store;

            // These should read using StringStore
            Assert.That(reader.ReadString(), Is.EqualTo("apple"));
            Assert.That(reader.ReadString(), Is.EqualTo("banana"));
            Assert.That(reader.ReadString(), Is.EqualTo("apple"));
            Assert.That(reader.ReadString(), Is.Null);
        }

        [Test]
        public void PooledNetworkWriterReleasesStringStore()
        {
            var store = new StringStore();
            var writer = NetworkWriterPool.GetWriter();
            writer.StringStore = store;

            writer.Release();

            Assert.That(writer.StringStore, Is.Null, "StringStore should be cleared when putting the writer back in the pool");
        }

        [Test]
        public void PooledNetworkReaderReleasesStringStore()
        {
            var store = new StringStore();
            byte[] dummyBytes = { 0x00 };
            var reader = NetworkReaderPool.GetReader(dummyBytes, null);
            reader.StringStore = store;

            reader.Dispose();

            Assert.That(reader.StringStore, Is.Null, "StringStore should be cleared when putting the reader back in the pool");
        }
    }
}
