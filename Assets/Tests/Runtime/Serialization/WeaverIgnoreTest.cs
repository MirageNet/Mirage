using System;
using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    public class WeaverIgnoreTest
    {
        [Test]
        public void DoesNotUseCustomWriter()
        {
            // check method names
            Assert.That(Writer<MyCustomType>.Write.Method.Name, Is.Not.EqualTo(new Action<NetworkWriter, MyCustomType>(MyCustomTypeExtension.WriteOnlyPartOfCustom).Method.Name));
            Assert.That(Reader<MyCustomType>.Read.Method.Name, Is.Not.EqualTo(new Func<NetworkReader, MyCustomType>(MyCustomTypeExtension.ReadOnlyPartOfCustom).Method.Name));

            // check writing and reading
            var data = new MyCustomType
            {
                first = 10,
                second = 20,
            };
            var writer = new NetworkWriter(1300);
            writer.Write(data);
            var reader = new NetworkReader();
            reader.Reset(writer.ToArraySegment());
            MyCustomType copy = reader.Read<MyCustomType>();

            // should have copied both fields,
            // if it uses custom extension methods it will only write first
            Assert.That(copy.first, Is.EqualTo(data.first));
            Assert.That(copy.second, Is.EqualTo(data.second));
            reader.Dispose();
        }

    }
    public struct MyCustomType
    {
        public int first;
        public int second;
    }
    public static class MyCustomTypeExtension
    {
        [WeaverIgnore]
        public static void WriteOnlyPartOfCustom(this NetworkWriter writer, MyCustomType value)
        {
            writer.WriteInt32(value.first);
        }
        [WeaverIgnore]

        public static MyCustomType ReadOnlyPartOfCustom(this NetworkReader reader)
        {
            return new MyCustomType { first = reader.ReadInt32() };
        }
    }
}
