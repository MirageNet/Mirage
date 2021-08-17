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
        readonly Random random = new Random();
        readonly VariableIntPacker packer;
        readonly ulong max;

        public UintPackerTests(ulong smallValue, ulong mediumValue, ulong? largeValue)
        {
            if (largeValue.HasValue)
            {
                packer = new VariableIntPacker(smallValue, mediumValue, largeValue.Value, false);
                max = largeValue.Value;
            }
            else
            {
                packer = new VariableIntPacker(smallValue, mediumValue);
                max = ulong.MaxValue;
            }
        }


        ulong GetRandonUlongBias()
        {
            return (ulong)(Math.Abs(random.NextDouble() - random.NextDouble()) * max);
        }

        uint GetRandonUintBias()
        {
            return (uint)(Math.Abs(random.NextDouble() - random.NextDouble()) * Math.Min(max, uint.MaxValue));
        }

        ushort GetRandonUshortBias()
        {
            return (ushort)(Math.Abs(random.NextDouble() - random.NextDouble()) * Math.Min(max, ushort.MaxValue));
        }

        [Test]
        [Repeat(1000)]
        public void UnpacksCorrectUlongValue()
        {
            ulong start = GetRandonUlongBias();
            packer.PackUlong(writer, start);
            ulong unpacked = packer.UnpackUlong(GetReader());

            Assert.That(unpacked, Is.EqualTo(start));
        }

        [Test]
        [Repeat(1000)]
        public void UnpacksCorrectUintValue()
        {
            uint start = GetRandonUintBias();
            packer.PackUint(writer, start);
            uint unpacked = packer.UnpackUint(GetReader());

            Assert.That(unpacked, Is.EqualTo(start));
        }

        [Test]
        [Repeat(1000)]
        public void UnpacksCorrectUshortValue()
        {
            ushort start = GetRandonUshortBias();
            packer.PackUshort(writer, start);
            ushort unpacked = packer.UnpackUshort(GetReader());

            Assert.That(unpacked, Is.EqualTo(start));
        }
    }
}
