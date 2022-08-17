using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    [TestFixture]
    public class ArrayWriterTest
    {
        [Test]
        public void TestNullByterray()
        {
            byte[] array = null;

            var data = MessagePacker.Pack(array);

            var unpacked = MessagePacker.Unpack<byte[]>(data, null);

            Assert.IsNull(unpacked);
        }

        [Test]
        public void TestEmptyByteArray()
        {
            var array = new byte[] { };

            var data = MessagePacker.Pack(array);

            var unpacked = MessagePacker.Unpack<byte[]>(data, null);

            Assert.IsNotNull(unpacked);
            Assert.IsEmpty(unpacked);
            Assert.That(unpacked, Is.EquivalentTo(new int[] { }));
        }

        [Test]
        public void TestDataByteArray()
        {
            var array = new byte[] { 3, 4, 5 };

            var data = MessagePacker.Pack(array);

            var unpacked = MessagePacker.Unpack<byte[]>(data, null);

            Assert.IsNotNull(unpacked);
            Assert.IsNotEmpty(unpacked);
            Assert.That(unpacked, Is.EquivalentTo(new byte[] { 3, 4, 5 }));
        }

        [Test]
        public void TestNullIntArray()
        {
            int[] array = null;

            var data = MessagePacker.Pack(array);

            var unpacked = MessagePacker.Unpack<int[]>(data, null);

            Assert.That(unpacked, Is.Null);
        }

        [Test]
        public void TestEmptyIntArray()
        {
            var array = new int[] { };

            var data = MessagePacker.Pack(array);

            var unpacked = MessagePacker.Unpack<int[]>(data, null);

            Assert.That(unpacked, Is.EquivalentTo(new int[] { }));
        }

        [Test]
        public void TestDataIntArray()
        {
            var array = new[] { 3, 4, 5 };

            var data = MessagePacker.Pack(array);

            var unpacked = MessagePacker.Unpack<int[]>(data, null);

            Assert.That(unpacked, Is.EquivalentTo(new[] { 3, 4, 5 }));
        }
    }
}
