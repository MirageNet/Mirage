using System;
using Mirage.Serialization;
using NUnit.Framework;
using Random = System.Random;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    [TestFixture(50ul, 1_000ul, null)]
    [TestFixture(250ul, 10_000ul, null)]
    [TestFixture(500ul, 100_000ul, null)]
    [TestFixture(50ul, 1_000ul, 10_000_000ul)]
    [TestFixture(250ul, 10_000ul, 10_000_000ul)]
    [TestFixture(500ul, 100_000ul, 10_000_000ul)]
    public class UintPackerTests : PackerTestBase
    {
        private readonly Random random = new Random();
        private readonly VarIntPacker packer;
        private readonly ulong max;

        public UintPackerTests(ulong smallValue, ulong mediumValue, ulong? largeValue)
        {
            if (largeValue.HasValue)
            {
                packer = new VarIntPacker(smallValue, mediumValue, largeValue.Value, false);
                max = largeValue.Value;
            }
            else
            {
                packer = new VarIntPacker(smallValue, mediumValue);
                max = ulong.MaxValue;
            }
        }

        private ulong GetRandonUlongBias()
        {
            return (ulong)(Math.Abs(random.NextDouble() - random.NextDouble()) * max);
        }

        private uint GetRandonUintBias()
        {
            return (uint)(Math.Abs(random.NextDouble() - random.NextDouble()) * Math.Min(max, uint.MaxValue));
        }

        private ushort GetRandonUshortBias()
        {
            return (ushort)(Math.Abs(random.NextDouble() - random.NextDouble()) * Math.Min(max, ushort.MaxValue));
        }

        [Test]
        [Repeat(1000)]
        public void UnpacksCorrectUlongValue()
        {
            var start = GetRandonUlongBias();
            packer.PackUlong(writer, start);
            var unpacked = packer.UnpackUlong(GetReader());

            Assert.That(unpacked, Is.EqualTo(start));
        }

        [Test]
        [Repeat(1000)]
        public void UnpacksCorrectUintValue()
        {
            var start = GetRandonUintBias();
            packer.PackUint(writer, start);
            var unpacked = packer.UnpackUint(GetReader());

            Assert.That(unpacked, Is.EqualTo(start));
        }

        [Test]
        [Repeat(1000)]
        public void UnpacksCorrectUshortValue()
        {
            var start = GetRandonUshortBias();
            packer.PackUshort(writer, start);
            var unpacked = packer.UnpackUshort(GetReader());

            Assert.That(unpacked, Is.EqualTo(start));
        }
    }
}
