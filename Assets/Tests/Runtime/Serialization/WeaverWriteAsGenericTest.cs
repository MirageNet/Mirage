using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    public class WeaverWriteAsGenericTest
    {
        [Test]
        public void WeaverShouldWriteUsingGenericButNotCreateThem()
        {
            Assert.That(Writer<IMyInterface>.Write, Is.Null, "Weaver should not have set writer");
            Assert.That(Reader<IMyInterface>.Read, Is.Null, "Weaver should not have set read");

            var writeCalled = 0;
            var readCalled = 0;

            Writer<IMyInterface>.Write = (writer, value) => writeCalled++;
            Reader<IMyInterface>.Read = (reader) => { readCalled++; return null; };

            try
            {
                Assert.That(writeCalled, Is.EqualTo(0));
                Assert.That(readCalled, Is.EqualTo(0));

                var writer = new NetworkWriter(1300);
                writer.Write(new MessageWithInterface());

                Assert.That(writeCalled, Is.EqualTo(1));
                Assert.That(readCalled, Is.EqualTo(0));

                var reader = new NetworkReader();
                reader.Reset(writer.ToArraySegment());
                var copy = reader.Read<MessageWithInterface>();

                Assert.That(writeCalled, Is.EqualTo(1));
                Assert.That(readCalled, Is.EqualTo(1));
            }
            finally
            {
                // they started null, so we need to reset them to null
                // if they dont start null, then the assert checks that the start will throw and stop us from setting them in test
                Writer<IMyInterface>.Write = null;
                Reader<IMyInterface>.Read = null;
            }
        }

        // a type that weaver can not create a writer for, it should instead write it as a generic
        [WeaverWriteAsGeneric]
        public interface IMyInterface
        {
            int Id { get; set; }
        }

        // use the interface in a message so we can check what methods it calls when writing
        [NetworkMessage]
        public struct MessageWithInterface
        {
            public IMyInterface data;
        }
    }
}
