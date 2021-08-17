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
        readonly Random random = new Random();
        readonly int blockSize;

        public UintBlockPackerTests(int blockSize)
        {
            this.blockSize = blockSize;
        }


        ulong GetRandonUlongBias()
        {
            return (ulong)(Math.Abs(random.NextDouble() - random.NextDouble()) * ulong.MaxValue);
        }

        uint GetRandonUintBias()
        {
            return (uint)(Math.Abs(random.NextDouble() - random.NextDouble()) * uint.MaxValue);
        }

        ushort GetRandonUshortBias()
        {
            return (ushort)(Math.Abs(random.NextDouble() - random.NextDouble()) * ushort.MaxValue);
        }

        [Test]
        [Repeat(1000)]
        public void UnpacksCorrectUlongValue()
        {
            ulong start = GetRandonUlongBias();
            VariableBlockPacker.Pack(writer, start, blockSize);
            ulong unpacked = VariableBlockPacker.Unpack(GetReader(), blockSize);

            Assert.That(unpacked, Is.EqualTo(start));
        }

        [Test]
        [Repeat(1000)]
        public void UnpacksCorrectUintValue()
        {
            uint start = GetRandonUintBias();
            VariableBlockPacker.Pack(writer, start, blockSize);
            ulong unpacked = VariableBlockPacker.Unpack(GetReader(), blockSize);

            Assert.That(unpacked, Is.EqualTo(start));
        }

        [Test]
        [Repeat(1000)]
        public void UnpacksCorrectUshortValue()
        {
            ushort start = GetRandonUshortBias();
            VariableBlockPacker.Pack(writer, start, blockSize);
            ulong unpacked = VariableBlockPacker.Unpack(GetReader(), blockSize);

            Assert.That(unpacked, Is.EqualTo(start));
        }

        [Test]
        public void WritesNplus1BitsPerBlock()
        {
            uint zero = 0u;
            VariableBlockPacker.Pack(writer, zero, blockSize);
            Assert.That(writer.BitPosition, Is.EqualTo(blockSize + 1));

            ulong unpacked = VariableBlockPacker.Unpack(GetReader(), blockSize);
            Assert.That(unpacked, Is.EqualTo(zero));
        }

        [Test]
        public void WritesNplus1BitsPerBlock_bigger()
        {
            uint aboveBlockSize = (1u << blockSize) + 1u;
            VariableBlockPacker.Pack(writer, aboveBlockSize, blockSize);
            Assert.That(writer.BitPosition, Is.EqualTo(2 * (blockSize + 1)));

            ulong unpacked = VariableBlockPacker.Unpack(GetReader(), blockSize);
            Assert.That(unpacked, Is.EqualTo(aboveBlockSize));
        }
    }
}
