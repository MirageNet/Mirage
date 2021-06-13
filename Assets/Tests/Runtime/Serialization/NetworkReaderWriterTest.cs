using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    [TestFixture]
    public class NetworkReaderWriterTest
    {
        public struct MyType
        {
            public int id;
            public string name;
        }

        [Test]
        public void TestIntWriterNotNull()
        {
            Assert.That(Writer<int>.Write, Is.Not.Null);
        }

        [Test]
        public void TestIntReaderNotNull()
        {
            Assert.That(Reader<int>.Read, Is.Not.Null);
        }

        [Test]
        public void TestCustomWriterNotNull()
        {
            Assert.That(Writer<MyType>.Write, Is.Not.Null);
        }

        [Test]
        public void TestCustomReaderNotNull()
        {
            Assert.That(Reader<MyType>.Read, Is.Not.Null);
        }

        [Test]
        public void TestAccessingCustomWriterAndReader()
        {
            var data = new MyType
            {
                id = 10,
                name = "Yo Gaba Gaba"
            };
            var writer = new NetworkWriter(1300);
            writer.Write(data);
            var reader = new NetworkReader();
            reader.Reset(writer.ToArraySegment());
            MyType copy = reader.Read<MyType>();

            Assert.That(copy, Is.EqualTo(data));

            reader.Dispose();
        }
    }
}
