using System;
using Mirage.Serialization;
using NUnit.Framework;
using Random = System.Random;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    [TestFixture(4)]
    [TestFixture(7)]
    [TestFixture(8)]
    [TestFixture(12)]
    [TestFixture(16)]
    public class UintBlockPackerTests : PackerTestBase
    {
        private readonly Random random = new Random();
        private readonly int blockSize;

        public UintBlockPackerTests(int blockSize)
        {
            this.blockSize = blockSize;
        }

        private ulong GetRandonUlongBias()
        {
            return (ulong)(Math.Abs(random.NextDouble() - random.NextDouble()) * ulong.MaxValue);
        }

        private uint GetRandonUintBias()
        {
            return (uint)(Math.Abs(random.NextDouble() - random.NextDouble()) * uint.MaxValue);
        }

        private ushort GetRandonUshortBias()
        {
            return (ushort)(Math.Abs(random.NextDouble() - random.NextDouble()) * ushort.MaxValue);
        }

        [Test]
        [Repeat(1000)]
        public void UnpacksCorrectUlongValue()
        {
            var start = GetRandonUlongBias();
            VarIntBlocksPacker.Pack(writer, start, blockSize);
            var unpacked = VarIntBlocksPacker.Unpack(GetReader(), blockSize);

            Assert.That(unpacked, Is.EqualTo(start));
        }

        [Test]
        [Repeat(1000)]
        public void UnpacksCorrectUintValue()
        {
            var start = GetRandonUintBias();
            VarIntBlocksPacker.Pack(writer, start, blockSize);
            var unpacked = VarIntBlocksPacker.Unpack(GetReader(), blockSize);

            Assert.That(unpacked, Is.EqualTo(start));
        }

        [Test]
        [Repeat(1000)]
        public void UnpacksCorrectUshortValue()
        {
            var start = GetRandonUshortBias();
            VarIntBlocksPacker.Pack(writer, start, blockSize);
            var unpacked = VarIntBlocksPacker.Unpack(GetReader(), blockSize);

            Assert.That(unpacked, Is.EqualTo(start));
        }

        [Test]
        public void WritesNplus1BitsPerBlock()
        {
            var zero = 0u;
            VarIntBlocksPacker.Pack(writer, zero, blockSize);
            Assert.That(writer.BitPosition, Is.EqualTo(blockSize + 1));

            var unpacked = VarIntBlocksPacker.Unpack(GetReader(), blockSize);
            Assert.That(unpacked, Is.EqualTo(zero));
        }

        [Test]
        public void WritesNplus1BitsPerBlock_bigger()
        {
            var aboveBlockSize = (1u << blockSize) + 1u;
            VarIntBlocksPacker.Pack(writer, aboveBlockSize, blockSize);
            Assert.That(writer.BitPosition, Is.EqualTo(2 * (blockSize + 1)));

            var unpacked = VarIntBlocksPacker.Unpack(GetReader(), blockSize);
            Assert.That(unpacked, Is.EqualTo(aboveBlockSize));
        }
    }
}
