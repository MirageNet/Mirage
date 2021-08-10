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
        [TestCase(1, 8)]
        [TestCase(0.1f, 11)]
        [TestCase(0.01f, 15)]
        public void CreateUsingPrecsion(float precision, int expectedBitCount)
        {
            var packer = new FloatPacker(100, precision);

            packer.Pack(writer, 1f);

            Assert.That(writer.BitPosition, Is.EqualTo(expectedBitCount));
        }

        [Test]
        public void PackFromBitCountPacksToCorrectCount([Range(1, 30)] int bitCount)
        {
            var packer = FloatPacker.FromBitCount(100, bitCount);

            packer.Pack(writer, 1f);

            Assert.That(writer.BitPosition, Is.EqualTo(bitCount));
        }

        [Test]
        public void ThrowsIfBitCountIsLessThan1([Range(-10, 0)] int bitCount)
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                FloatPacker.FromBitCount(10, bitCount);
            });

            var expected = new ArgumentException("Bit count is too low, bit count should be between 1 and 30", "bitCount");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        public void ThrowsIfBitCountIsGreaterThan30([Range(31, 40)] int bitCount)
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                FloatPacker.FromBitCount(10, bitCount);
            });

            var expected = new ArgumentException("Bit count is too high, bit count should be between 1 and 30", "bitCount");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        public void ThrowsIfMaxIsZero()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                FloatPacker.FromBitCount(0, 1);
            });

            var expected = new ArgumentException("Max can not be 0", "max");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
    }
}
