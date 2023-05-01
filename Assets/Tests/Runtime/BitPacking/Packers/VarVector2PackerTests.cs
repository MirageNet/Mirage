using System;
using System.Collections.Generic;
using Mirage.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    [TestFixtureSource(typeof(VarVector2PackerTests), nameof(VarVector2PackerTests.TestData))]
    public class VarVector2PackerTests : PackerTestBase
    {
        public static IEnumerable<object[]> TestData()
        {
            for (var i = 1; i < 8; i++)
            {
                yield return new object[] { Vector2.one * 0.1f, i };
                yield return new object[] { Vector2.one * 0.01f, i };
                yield return new object[] { Vector2.one * 0.5f, i };
                yield return new object[] { new Vector2(0.1f, 0.5f), i };
                yield return new object[] { new Vector2(1f, 0.1f), i };
            }
        }

        private readonly Vector2 _precision;
        private readonly int _blockSize;
        private readonly VarVector2Packer _packer;
        private readonly Vector2Int _maxExponent;

        public VarVector2PackerTests(Vector2 precsion, int blockSize)
        {
            _precision = precsion;
            _packer = new VarVector2Packer(precsion, blockSize);
            for (var i = 0; i < 2; i++)
            {
                _maxExponent[i] = (int)Math.Log(1 / _precision[i]);
            }
        }


        // weighed random
        private Vector2 GetRandomVector()
        {
            Vector2 vec = default;
            for (var i = 0; i < 2; i++)
            {
                vec[i] = VarFloatPackerTests.GetRandomFloat(_maxExponent[i]);
            }
            return vec;
        }


        [Test]
        [Repeat(100)]
        public void UnpacksToSameValue()
        {
            var expected = GetRandomVector();

            _packer.Pack(writer, expected);
            var unpacked = _packer.Unpack(GetReader());

            Assert.That(unpacked.x, Is.EqualTo(expected.x).Within(_precision.x));
            Assert.That(unpacked.y, Is.EqualTo(expected.y).Within(_precision.y));
        }

        [Test]
        public void ZeroUnpacksAsZero()
        {
            var zero = Vector2.zero;

            _packer.Pack(writer, zero);
            var unpacked = _packer.Unpack(GetReader());

            Assert.That(unpacked, Is.EqualTo(zero));
        }
    }
}
