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
            Assert.That(Writer<MyIgnoredType>.Write.Method.Name, Is.Not.EqualTo(new Action<NetworkWriter, MyIgnoredType>(MyCustomTypeExtension.WriteOnlyPartOfIgnore).Method.Name));
            Assert.That(Reader<MyIgnoredType>.Read.Method.Name, Is.Not.EqualTo(new Func<NetworkReader, MyIgnoredType>(MyCustomTypeExtension.ReadOnlyPartOfIgnore).Method.Name));

            // check writing and reading
            var data = new MyIgnoredType
            {
                first = 10,
                second = 20,
            };
            var writer = new NetworkWriter(1300);
            writer.Write(data);
            var reader = new NetworkReader();
            reader.Reset(writer.ToArraySegment());
            MyIgnoredType copy = reader.Read<MyIgnoredType>();

            // should have copied both fields,
            // if it uses custom extension methods it will only write first
            Assert.That(copy.first, Is.EqualTo(data.first));
            Assert.That(copy.second, Is.EqualTo(data.second));
            reader.Dispose();
        }

    }
    public class SerializeExtensionTest
    {
        [Test]
        public void UsesWriterWithHigherPriority()
        {
            // check method names
            Assert.That(Writer<MyPriorityType>.Write.Method.Name,
                Is.EqualTo(new Action<NetworkWriter, MyPriorityType>(MyCustomTypeExtension.WriteOnlyPartOfPriority_high).Method.Name));
            Assert.That(Reader<MyPriorityType>.Read.Method.Name,
                Is.EqualTo(new Func<NetworkReader, MyPriorityType>(MyCustomTypeExtension.ReadOnlyPartOfPriority_high).Method.Name));

            // check writing and reading
            var data = new MyPriorityType
            {
                low = 10,
                high = 20,
            };
            var writer = new NetworkWriter(1300);
            writer.Write(data);
            var reader = new NetworkReader();
            reader.Reset(writer.ToArraySegment());
            MyPriorityType copy = reader.Read<MyPriorityType>();

            // should have only written 1 field, check that is is using high priority writer
            Assert.That(copy.low, Is.Zero, "Should not be set by writer with higher priority");
            Assert.That(copy.high, Is.EqualTo(data.high));
            reader.Dispose();
        }
    }


    public struct MyIgnoredType
    {
        public int first;
        public int second;
    }
    public struct MyPriorityType
    {
        public int low;
        public int high;
    }
    public static class MyCustomTypeExtension
    {
        [WeaverIgnore]
        public static void WriteOnlyPartOfIgnore(this NetworkWriter writer, MyIgnoredType value)
        {
            writer.WriteInt32(value.first);
        }
        [WeaverIgnore]
        public static MyIgnoredType ReadOnlyPartOfIgnore(this NetworkReader reader)
        {
            return new MyIgnoredType { first = reader.ReadInt32() };
        }


        [SerializeExtension(Priority = 1)]
        public static void WriteOnlyPartOfPriority_low(this NetworkWriter writer, MyPriorityType value)
        {
            writer.WriteInt32(value.low);
        }


        [SerializeExtension(Priority = 10)]
        public static void WriteOnlyPartOfPriority_high(this NetworkWriter writer, MyPriorityType value)
        {
            writer.WriteInt32(value.high);
        }


        [SerializeExtension(Priority = 10)]
        public static MyPriorityType ReadOnlyPartOfPriority_high(this NetworkReader reader)
        {
            return new MyPriorityType { high = reader.ReadInt32() };
        }

        // define this reader after other, so that we test bother define orders
        [SerializeExtension(Priority = 1)]
        public static MyPriorityType ReadOnlyPartOfPriority_low(this NetworkReader reader)
        {
            return new MyPriorityType { low = reader.ReadInt32() };
        }
    }
}
