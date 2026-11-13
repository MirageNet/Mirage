using System.Collections.Generic;
using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    public class EnumBitCountTests
    {
        [BitCount(2)]
        public enum MyEnum
        {
            A = 0,
            B = 1,
            C = 3,
        }

        [BitCount(4), ZigZagEncode]
        public enum MyZigZagEnum
        {
            Min = -8,
            Zero = 0,
            Max = 7,
        }

        [BitCountFromRange(-2, 2)]
        public enum MyRangeEnum
        {
            Min = -2,
            Zero = 0,
            Max = 2,
        }

        [NetworkMessage]
        public struct MyMessage
        {
            public MyEnum value;

            // field attribute overrides enum attribute
            [BitCount(8)]
            public MyEnum overrideValue;
        }

        private readonly NetworkWriter writer = new NetworkWriter(1300);
        private readonly NetworkReader reader = new NetworkReader();

        [TearDown]
        public void TearDown()
        {
            writer.Reset();
            reader.Dispose();
        }

        // note: Write and Read are called with concrete types in each test,
        // because weaver only generates functions for types it sees at call sites

        [Test]
        [TestCase(MyEnum.A)]
        [TestCase(MyEnum.B)]
        [TestCase(MyEnum.C)]
        public void EnumIsBitPacked(MyEnum value)
        {
            writer.Write(value);
            AssertWriterBits(2);

            var result = reader.Read<MyEnum>();
            AssertReaderBits(2);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(MyZigZagEnum.Min)]
        [TestCase(MyZigZagEnum.Zero)]
        [TestCase(MyZigZagEnum.Max)]
        public void ZigZagEnumIsBitPacked(MyZigZagEnum value)
        {
            writer.Write(value);
            AssertWriterBits(4);

            var result = reader.Read<MyZigZagEnum>();
            AssertReaderBits(4);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(MyRangeEnum.Min)]
        [TestCase(MyRangeEnum.Zero)]
        [TestCase(MyRangeEnum.Max)]
        public void RangeEnumIsBitPacked(MyRangeEnum value)
        {
            // range of 4 needs 3 bits
            writer.Write(value);
            AssertWriterBits(3);

            var result = reader.Read<MyRangeEnum>();
            AssertReaderBits(3);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        public void MessageFieldUsesEnumAndOverrideBitCount()
        {
            var message = new MyMessage
            {
                value = MyEnum.C,
                overrideValue = MyEnum.B,
            };

            writer.Write(message);
            AssertWriterBits(2 + 8);

            var result = reader.Read<MyMessage>();
            AssertReaderBits(2 + 8);
            Assert.That(result, Is.EqualTo(message));
        }

        [Test]
        public void ListElementsAreBitPacked()
        {
            // compare with byte list so that the length header is the same
            var byteList = new List<byte> { 0, 1, 3 };
            writer.Write(byteList);
            var expectedBits = writer.BitPosition - (byteList.Count * (8 - 2));
            writer.Reset();

            var list = new List<MyEnum> { MyEnum.A, MyEnum.B, MyEnum.C };
            writer.Write(list);
            AssertWriterBits(expectedBits);

            var result = reader.Read<List<MyEnum>>();
            AssertReaderBits(expectedBits);
            Assert.That(result, Is.EqualTo(list));
        }

        [Test]
        public void NullableIsBitPacked()
        {
            // compare with nullable byte so that the has value header is the same
            byte? nullableByte = 3;
            writer.Write(nullableByte);
            var expectedBits = writer.BitPosition - (8 - 2);
            writer.Reset();

            MyEnum? value = MyEnum.C;
            writer.Write(value);
            AssertWriterBits(expectedBits);

            var result = reader.Read<MyEnum?>();
            AssertReaderBits(expectedBits);
            Assert.That(result, Is.EqualTo(value));
        }

        /// <summary>
        /// Checks writer bit count, then resets reader so it is ready to read the written value
        /// </summary>
        private void AssertWriterBits(int expectedBits)
        {
            Assert.That(writer.BitPosition, Is.EqualTo(expectedBits));
            reader.Reset(writer.ToArraySegment());
        }

        private void AssertReaderBits(int expectedBits)
        {
            Assert.That(reader.BitPosition, Is.EqualTo(expectedBits));
        }
    }
}
