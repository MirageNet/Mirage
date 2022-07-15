using System;
using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    public class UintPackerCreateTest : PackerTestBase
    {
        [Test]
        [TestCase(0ul, 50ul, 1000ul, ExpectedResult = 6 + 1)]
        [TestCase(60ul, 50ul, 1000ul, ExpectedResult = 6 + 1, Description = "50 rounds up to 64, so v=60 is still small")]
        [TestCase(100ul, 50ul, 1000ul, ExpectedResult = 10 + 2)]
        [TestCase(100ul, 500ul, 100000ul, ExpectedResult = 9 + 1)]
        [TestCase(501ul, 500ul, 100000ul, ExpectedResult = 9 + 1)]
        [TestCase(5_000ul, 500ul, 100000ul, ExpectedResult = 17 + 2)]
        [TestCase(5_000_000ul, 500ul, 100000ul, ExpectedResult = 64 + 2)]
        public int CreatesUsing2Values(ulong inValue, ulong smallValue, ulong mediumValue)
        {
            var packer = new VarIntPacker(smallValue, mediumValue);
            packer.PackUlong(writer, inValue);
            return writer.BitPosition;
        }


        [Test]
        [TestCase(0ul, 50ul, 1000ul, 50_000ul, ExpectedResult = 6 + 1)]
        [TestCase(60ul, 50ul, 1000ul, 50_000ul, ExpectedResult = 6 + 1, Description = "50 rounds up to 64, so v=60 is still small")]
        [TestCase(100ul, 50ul, 1000ul, 50_000ul, ExpectedResult = 10 + 2)]
        [TestCase(5000ul, 50ul, 1000ul, 50_000ul, ExpectedResult = 16 + 2)]
        [TestCase(100ul, 500ul, 100_000ul, 50_000_000ul, ExpectedResult = 9 + 1)]
        [TestCase(501ul, 500ul, 100_000ul, 50_000_000ul, ExpectedResult = 9 + 1)]
        [TestCase(5_000ul, 500ul, 100_000ul, 50_000_000ul, ExpectedResult = 17 + 2)]
        [TestCase(5_000_000ul, 500ul, 100_000ul, 50_000_000ul, ExpectedResult = 26 + 2)]
        public int CreatesUsing3Values(ulong inValue, ulong smallValue, ulong mediumValue, ulong largeValue)
        {
            var packer = new VarIntPacker(smallValue, mediumValue, largeValue);
            packer.PackUlong(writer, inValue);
            return writer.BitPosition;
        }


        [Test]
        public void ThrowsIfSmallBitIsZero()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                _ = VarIntPacker.FromBitCount(0, 10);
            });
            var expected = new ArgumentException("Small value can not be zero", "smallBits");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
        [Test]
        public void ThrowsIfMediumLessThanSmall()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                _ = VarIntPacker.FromBitCount(6, 5);
            });
            var expected = new ArgumentException("Medium value must be greater than small value", "mediumBits");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
        [Test]
        public void ThrowsIfLargeLessThanMedium()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                _ = VarIntPacker.FromBitCount(4, 10, 8);
            });
            var expected = new ArgumentException("Large value must be greater than medium value", "largeBits");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
        [Test]
        public void ThrowsIfLargeIsOver64()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                _ = VarIntPacker.FromBitCount(5, 10, 65);
            });
            var expected = new ArgumentException("Large bits must be 64 or less", "largeBits");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
        [Test]
        public void ThrowsIfMediumIsOver62()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                _ = VarIntPacker.FromBitCount(5, 63);
            });
            var expected = new ArgumentException("Medium bits must be 62 or less", "mediumBits");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        public void ThrowsWhenValueIsOverLargeValue()
        {
            var packer = VarIntPacker.FromBitCount(1, 2, 3, true);
            var exception1 = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                packer.PackUlong(writer, 20);
            });
            var exception2 = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                packer.PackUint(writer, 20);
            });
            var exception3 = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                packer.PackUlong(writer, 20);
            });
            var expected = new ArgumentOutOfRangeException("value", 20, $"Value is over max of {7}");
            Assert.That(exception1, Has.Message.EqualTo(expected.Message));
            Assert.That(exception2, Has.Message.EqualTo(expected.Message));
            Assert.That(exception3, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        [TestCase(20ul, 3)]
        [TestCase(260ul, 8)]
        [TestCase(50_000ul, 10)]
        public void WritesMaxIfOverLargeValue(ulong inValue, int largeBits)
        {
            var max = BitMask.Mask(largeBits);
            var packer = VarIntPacker.FromBitCount(1, 2, largeBits, false);
            Assert.DoesNotThrow(() =>
            {
                packer.PackUlong(writer, inValue);
                packer.PackUint(writer, (uint)inValue);
                packer.PackUlong(writer, (ushort)inValue);
            });
            var reader = GetReader();
            var unpack1 = packer.UnpackUlong(reader);
            var unpack2 = packer.UnpackUint(reader);
            var unpack3 = packer.UnpackUshort(reader);

            Assert.That(unpack1, Is.EqualTo(max));
            Assert.That(unpack2, Is.EqualTo(max));
            Assert.That(unpack3, Is.EqualTo(max));
        }
    }
}
