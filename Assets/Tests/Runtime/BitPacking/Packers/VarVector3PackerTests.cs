using System;
using System.Collections.Generic;
using Mirage.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    [TestFixtureSource(typeof(VarVector3PackerTests), nameof(VarVector3PackerTests.TestData))]
    public class VarVector3PackerTests : PackerTestBase
    {
        public static IEnumerable<object[]> TestData()
        {
            for (var i = 1; i < 8; i++)
            {
                yield return new object[] { Vector3.one * 0.1f, i };
                yield return new object[] { Vector3.one * 0.01f, i };
                yield return new object[] { Vector3.one * 0.5f, i };
                yield return new object[] { new Vector3(0.1f, 0.5f, 0.1f), i };
                yield return new object[] { new Vector3(1f, 0.1f, 1f), i };
            }
        }

        private readonly Vector3 _precision;
        private readonly int _blockSize;
        private readonly VarVector3Packer _packer;
        private readonly Vector3Int _maxExponent;

        public VarVector3PackerTests(Vector3 precsion, int blockSize)
        {
            _precision = precsion;
            _packer = new VarVector3Packer(precsion, blockSize);
            for (var i = 0; i < 3; i++)
            {
                _maxExponent[i] = (int)Math.Log(1 / _precision[i]);
            }
        }


        // weighed random
        private Vector3 GetRandomVector()
        {
            Vector3 vec = default;
            for (var i = 0; i < 3; i++)
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
            Assert.That(unpacked.z, Is.EqualTo(expected.z).Within(_precision.z));
        }

        [Test]
        public void ZeroUnpacksAsZero()
        {
            var zero = Vector3.zero;

            _packer.Pack(writer, zero);
            var unpacked = _packer.Unpack(GetReader());

            Assert.That(unpacked, Is.EqualTo(zero));
        }
    }
}
