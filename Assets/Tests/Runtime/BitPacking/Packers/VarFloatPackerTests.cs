using System;
using System.Collections.Generic;
using Mirage.Serialization;
using NUnit.Framework;
using Random = Mirage.Tests.BitPacking.TestRandom;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    [TestFixtureSource(typeof(VarFloatPackerTests), nameof(VarFloatPackerTests.TestData))]
    public class VarFloatPackerTests : PackerTestBase
    {
        public static IEnumerable<object[]> TestData()
        {
            for (var i = 1; i < 8; i++)
            {
                yield return new object[] { 0.1f, i };
                yield return new object[] { 0.02f, i };
                yield return new object[] { 0.05f, i };
                yield return new object[] { 0.01f, i };
                yield return new object[] { 30, i };
            }
        }

        private readonly float _precsion;
        private readonly VarFloatPacker _packer;
        private readonly int _maxExponent;

        public VarFloatPackerTests(float precsion, int blockSize)
        {
            _precsion = precsion;
            _packer = new VarFloatPacker(precsion, blockSize);
            _maxExponent = (int)Math.Log(1 / _precsion);
        }

        // weighed random
        private float GetRandomFloat()
        {
            return GetRandomFloat(_maxExponent);
        }

        public static float GetRandomFloat(int maxExponent)
        {
            // scale random number with _maxExponent calcualted from _precsion
            // we dont want the random number to bee too big, or it will loss precsion just from being a float
            var exponent = Random.Range(0, 20 - maxExponent);
            var mutlipler = Random.Range(-1f, 1f);
            return (2 << exponent) * mutlipler;
        }


        [Test]
        [Repeat(100)]
        public void UnpackedValueIsWithinPrecision()
        {
            var start = GetRandomFloat();
            _packer.Pack(writer, start);
            var unpacked = _packer.Unpack(GetReader());

            Assert.That(unpacked, Is.EqualTo(start).Within(_precsion));
        }


        [Test]
        public void ZeroUnpackToExactlyZero()
        {
            const float zero = 0;
            _packer.Pack(writer, zero);
            var unpacked = _packer.Unpack(GetReader());

            Assert.That(unpacked, Is.EqualTo(zero));
        }
    }
}
