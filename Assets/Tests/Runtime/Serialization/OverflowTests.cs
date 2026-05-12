using System;
using System.IO;
using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    [TestFixture]
    public class OverflowTests
    {
        [Test]
        public void CanReadBitsOverflowTest()
        {
            var reader = new NetworkReader();
            reader.Reset(new byte[100]);

            // int.MaxValue bits will overflow when added to _bitPosition
            reader.MoveBitPosition(100);
            Assert.Throws<OverflowException>(() =>
            {
                reader.CanReadBits(int.MaxValue);
            });
        }

        [Test]
        public void CanReadBytesOverflowTest()
        {
            var reader = new NetworkReader();
            reader.Reset(new byte[100]);

            // int.MaxValue bytes will overflow when multiplied by 8
            Assert.Throws<OverflowException>(() =>
            {
                reader.CanReadBytes(int.MaxValue);
            });
        }

        [Test]
        public void NegativeReadBitsTest()
        {
            var reader = new NetworkReader();
            reader.Reset(new byte[100]);

            Assert.Throws<EndOfStreamException>(() =>
            {
                reader.CanReadBits(-1);
            });
        }

        [Test]
        public void NegativeReadBytesTest()
        {
            var reader = new NetworkReader();
            reader.Reset(new byte[100]);

            Assert.Throws<EndOfStreamException>(() =>
            {
                reader.CanReadBytes(-1);
            });
        }

        [Test]
        public void ReadBytesSegmentOverflowTest()
        {
            var reader = new NetworkReader();
            reader.Reset(new byte[100]);

            Assert.Throws<OverflowException>(() =>
            {
                reader.ReadBytesSegment(int.MaxValue);
            });
        }

        [Test]
        public void ReadBytesSegmentNegativeTest()
        {
            var reader = new NetworkReader();
            reader.Reset(new byte[100]);

            Assert.Throws<EndOfStreamException>(() =>
            {
                reader.ReadBytesSegment(-1);
            });
        }

        [Test]
        public void ReadBytesOverflowTest()
        {
            var reader = new NetworkReader();
            reader.Reset(new byte[100]);

            // 0x10000000 * 8 = 0x80000000 which is int.MinValue (overflow)
            Assert.Throws<OverflowException>(() =>
            {
                reader.ReadBytes(0x10000000);
            });
        }

        [Test]
        public void ResetOverflowTest()
        {
            var reader = new NetworkReader();
            // length*8 will overflow, Reset should throw because it uses checked block
            Assert.Throws<OverflowException>(() =>
            {
                reader.Reset(new byte[100], 0, int.MaxValue / 4);
            });
        }

        [Test]
        public void SkipOverflowTest()
        {
            var reader = new NetworkReader();
            reader.Reset(new byte[100], 0, 10);
            Assert.Throws<EndOfStreamException>(() =>
            {
                reader.Skip(int.MaxValue);
            });
        }

        [Test]
        public void ReadAtPositionOverflowTest()
        {
            var reader = new NetworkReader();
            reader.Reset(new byte[100], 0, 10);
            Assert.Throws<EndOfStreamException>(() =>
            {
                // bitPosition + bits will overflow
                reader.ReadAtPosition(10, int.MaxValue);
            });
        }

        [Test]
        public void ReadBooleanWrapOverflowTest()
        {
            var reader = new NetworkReader();
            // Max safe length for Reset is int.MaxValue / 8
            var maxSafeLength = int.MaxValue / 8;
            reader.Reset(new byte[maxSafeLength + 1], 0, maxSafeLength);

            // Now bitLength is 2147483640.
            // Move position to the end.
            reader.MoveBitPosition(2147483640);

            // ReadBoolean adds 1. 2147483640 + 1 = 2147483641. 
            // This is > bitLength, so it should throw EndOfStreamException.
            Assert.Throws<EndOfStreamException>(() =>
            {
                reader.ReadBoolean();
            });
        }

        [Test]
        public void ReadUInt64WrapOverflowTest()
        {
            var reader = new NetworkReader();
            var maxSafeLength = int.MaxValue / 8;
            reader.Reset(new byte[maxSafeLength + 1], 0, maxSafeLength);

            reader.MoveBitPosition(2147483640);

            // 2147483640 + 64 overflows to negative.
            // CheckNewLength should catch the negative value.
            Assert.Throws<EndOfStreamException>(() =>
            {
                reader.ReadUInt64();
            });
        }
    }
}
