using System;
using System.Collections.Generic;
using Mirage.Serialization;
using NUnit.Framework;
using Mirage;

namespace Mirage.Tests.Runtime.Serialization
{
    public struct MyCustomStructWithWriter
    {
        public string Name;
        public int Value;
    }

    public static class MyCustomStructWithWriterExtensions
    {
        // Extension methods must explain why they are defined (to register length-limited serialization)
        public static void WriteMyStruct(this NetworkWriter writer, MyCustomStructWithWriter value, int maxLength)
        {
            writer.WriteString(value.Name, maxLength);
            writer.WriteInt32(value.Value);
        }

        public static MyCustomStructWithWriter ReadMyStruct(this NetworkReader reader, int maxLength)
        {
            return new MyCustomStructWithWriter
            {
                Name = reader.ReadString(maxLength),
                Value = reader.ReadInt32()
            };
        }
    }

    [TestFixture]
    public class NetworkReaderWriterTest
    {
        // This dummy message forces the Weaver to generate both standard and length-limited
        // serializers for these collection types so they are populated and tested successfully.
        [NetworkMessage]
        public struct WeaverGenericCollectionUsages
        {
            [MaxLength(10)] public List<int> listInt;
            [MaxLength(10)] public int[] intArray;
            [MaxLength(10)] public ArraySegment<int> arraySegmentInt;
            [MaxLength(10)] public Dictionary<int, string> dictionaryIntString;
            [MaxLength(10)] public byte[] byteArray;
            [MaxLength(10)] public ArraySegment<byte> arraySegmentByte;
            [MaxLength(10)] public MyCustomStructWithWriter customWithLength;

            // These non-MaxLength fields force standard writer/reader generation
            public List<int> listIntNormal;
            public int[] intArrayNormal;
            public ArraySegment<int> arraySegmentIntNormal;
            public Dictionary<int, string> dictionaryIntStringNormal;
            public byte[] byteArrayNormal;
            public ArraySegment<byte> arraySegmentByteNormal;
            public MyCustomStructWithWriter customNormal;
        }

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
            var copy = reader.Read<MyType>();

            Assert.That(copy, Is.EqualTo(data));

            reader.Dispose();
        }

        [Test]
        // Weaver must initialize generic delegates at runtime so list collections can be serialized without manual registration.
        public void TestListIntReaderWriterNotNull()
        {
            Assert.That(Writer<List<int>>.Write, Is.Not.Null);
            Assert.That(Writer<List<int>>.WriteWithLength, Is.Not.Null);
            Assert.That(Reader<List<int>>.Read, Is.Not.Null);
            Assert.That(Reader<List<int>>.ReadWithLength, Is.Not.Null);
        }

        [Test]
        // Weaver must initialize generic delegates at runtime so arrays can be serialized without manual registration.
        public void TestIntArrayReaderWriterNotNull()
        {
            Assert.That(Writer<int[]>.Write, Is.Not.Null);
            Assert.That(Writer<int[]>.WriteWithLength, Is.Not.Null);
            Assert.That(Reader<int[]>.Read, Is.Not.Null);
            Assert.That(Reader<int[]>.ReadWithLength, Is.Not.Null);
        }

        [Test]
        // Weaver must initialize generic delegates at runtime so array segments can be serialized without manual registration.
        public void TestArraySegmentIntReaderWriterNotNull()
        {
            Assert.That(Writer<ArraySegment<int>>.Write, Is.Not.Null);
            Assert.That(Writer<ArraySegment<int>>.WriteWithLength, Is.Not.Null);
            Assert.That(Reader<ArraySegment<int>>.Read, Is.Not.Null);
            Assert.That(Reader<ArraySegment<int>>.ReadWithLength, Is.Not.Null);
        }

        [Test]
        // Weaver must initialize generic delegates at runtime so dictionaries can be serialized without manual registration.
        public void TestDictionaryReaderWriterNotNull()
        {
            Assert.That(Writer<Dictionary<int, string>>.Write, Is.Not.Null);
            Assert.That(Writer<Dictionary<int, string>>.WriteWithLength, Is.Not.Null);
            Assert.That(Reader<Dictionary<int, string>>.Read, Is.Not.Null);
            Assert.That(Reader<Dictionary<int, string>>.ReadWithLength, Is.Not.Null);
        }

        [Test]
        // Weaver must initialize generic delegates at runtime so byte arrays can be serialized without manual registration.
        public void TestByteArrayReaderWriterNotNull()
        {
            Assert.That(Writer<byte[]>.Write, Is.Not.Null);
            Assert.That(Writer<byte[]>.WriteWithLength, Is.Not.Null);
            Assert.That(Reader<byte[]>.Read, Is.Not.Null);
            Assert.That(Reader<byte[]>.ReadWithLength, Is.Not.Null);
        }

        [Test]
        // Weaver must initialize generic delegates at runtime so byte array segments can be serialized without manual registration.
        public void TestArraySegmentByteReaderWriterNotNull()
        {
            Assert.That(Writer<ArraySegment<byte>>.Write, Is.Not.Null);
            Assert.That(Writer<ArraySegment<byte>>.WriteWithLength, Is.Not.Null);
            Assert.That(Reader<ArraySegment<byte>>.Read, Is.Not.Null);
            Assert.That(Reader<ArraySegment<byte>>.ReadWithLength, Is.Not.Null);
        }

        [Test]
        // Weaver must wire the length-limited reader and writer for our custom struct with extensions.
        public void TestCustomStructWithLengthReaderWriterNotNull()
        {
            Assert.That(Writer<MyCustomStructWithWriter>.Write, Is.Not.Null);
            Assert.That(Writer<MyCustomStructWithWriter>.WriteWithLength, Is.Not.Null);
            Assert.That(Reader<MyCustomStructWithWriter>.Read, Is.Not.Null);
            Assert.That(Reader<MyCustomStructWithWriter>.ReadWithLength, Is.Not.Null);
        }

        [Test]
        // Verify that the woven length-limited delegates correctly encode, decode, and enforce limit violations.
        public void TestCustomStructWithLengthEnforcesLimits()
        {
            var data = new MyCustomStructWithWriter
            {
                Name = "abc",
                Value = 42
            };
            var writer = new NetworkWriter(1300);
            writer.WriteWithLength(data, 5);
            var reader = new NetworkReader();
            reader.Reset(writer.ToArraySegment());
            var copy = reader.ReadWithLength<MyCustomStructWithWriter>(5);

            Assert.That(copy.Name, Is.EqualTo("abc"));
            Assert.That(copy.Value, Is.EqualTo(42));

            // Verify limit is enforced during writing
            var writerExceeded = new NetworkWriter(1300);
            Assert.Throws<SerializationLimitException>(() =>
            {
                writerExceeded.WriteWithLength(data, 2);
            });

            reader.Dispose();
        }
    }
}

