using System;
using Mirage.Serialization;
using NUnit.Framework;
using UnityEngine;
using Range = NUnit.Framework.RangeAttribute;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    public class FloatPackerCreateTest : PackerTestBase
    {
        [Test]
        [TestCase(1, ExpectedResult = 8)]
        [TestCase(0.1f, ExpectedResult = 11)]
        [TestCase(0.01f, ExpectedResult = 15)]
        public int CreateUsingPrecsion(float precision)
        {
            var packer = new FloatPacker(100, precision);

            packer.Pack(writer, 1f);
            return writer.BitPosition;
        }

        [Test]
        [TestCase(1, ExpectedResult = 7)]
        [TestCase(0.1f, ExpectedResult = 10)]
        [TestCase(0.01f, ExpectedResult = 14)]
        public int BitCountIsLessForUnSigned(float precision)
        {
            var packer = new FloatPacker(100, precision, false);

            packer.Pack(writer, 1f);
            return writer.BitPosition;
        }

        [Test]
        public void PackFromBitCountPacksToCorrectCount([Range(1, 30)] int bitCount, [Values(true, false)] bool signed)
        {
            var packer = new FloatPacker(100, bitCount, signed);

            packer.Pack(writer, 1f);

            Assert.That(writer.BitPosition, Is.EqualTo(bitCount));
        }

        [Test]
        public void ThrowsIfBitCountIsLessThan1([Range(-10, 0)] int bitCount, [Values(true, false)] bool signed)
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                _ = new FloatPacker(10, bitCount, signed);
            });

            var expected = new ArgumentException("Bit count is too low, bit count should be between 1 and 30", "bitCount");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        public void ThrowsIfBitCountIsGreaterThan30([Range(31, 40)] int bitCount, [Values(true, false)] bool signed)
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                _ = new FloatPacker(10, bitCount, signed);
            });

            var expected = new ArgumentException("Bit count is too high, bit count should be between 1 and 30", "bitCount");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        public void ThrowsIfMaxIsZero([Values(true, false)] bool signed)
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                _ = new FloatPacker(0, 1, signed);
            });

            var expected = new ArgumentException("Max can not be 0", "max");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
    }
}
