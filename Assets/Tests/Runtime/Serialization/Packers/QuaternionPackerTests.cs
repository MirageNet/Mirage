using System.Collections;
using System.Collections.Generic;
using Mirage.Serialization;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEngine;
using Random = UnityEngine.Random;
using Range = NUnit.Framework.RangeAttribute;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    public class QuaternionPackerTests : PackerTestBase
    {
        static Quaternion GetRandomQuaternion()
        {
            return Random.rotationUniform.normalized;
        }
        static Quaternion GetRandomQuaternionNotNormalized()
        {
            return new Quaternion(
                Random.Range(-1, 1),
                Random.Range(-1, 1),
                Random.Range(-1, 1),
                Random.Range(-1, 1)
                );
        }


        [Test]
        public void IdentityIsUnpackedAsIdentity()
        {
            var packer = new QuaternionPacker(10);
            packer.Pack(writer, Quaternion.identity);
            Quaternion unpack = packer.Unpack(GetReader());

            Assert.That(unpack, Is.EqualTo(Quaternion.identity));
        }


        static IEnumerable ReturnsCorrectIndexCases()
        {
            var values = new List<float>() { 0.1f, 0.2f, 0.3f, 0.4f };
            // abcd are index
            // testing all permutation, index can only be used once
            for (int a = 0; a < 4; a++)
            {
                for (int b = 0; b < 4; b++)
                {
                    if (b == a) { continue; }

                    for (int c = 0; c < 4; c++)
                    {
                        if (c == a || c == b) { continue; }

                        for (int d = 0; d < 4; d++)
                        {
                            if (d == a || d == b || d == c) { continue; }

                            uint largest = 0;
                            // index 3 is the largest, 
                            if (a == 3) { largest = 0; }
                            if (b == 3) { largest = 1; }
                            if (c == 3) { largest = 2; }
                            if (d == 3) { largest = 3; }
                            yield return new TestCaseData(new Quaternion(values[a], values[b], values[c], values[d]).normalized)
                                .Returns(largest);
                        }
                    }
                }
            }
        }

        [Test]
        [TestCaseSource(nameof(ReturnsCorrectIndexCases))]
        public uint ReturnsCorrectIndex(Quaternion q)
        {
            QuaternionPacker.FindLargestIndex(ref q, out uint index);
            return index;
        }


        static IEnumerable CompressesAndDecompressesCases()
        {
            for (int i = 8; i < 12; i++)
            {
                yield return new TestCaseData(i, Quaternion.identity);
                yield return new TestCaseData(i, Quaternion.Euler(25, 30, 0));
                yield return new TestCaseData(i, Quaternion.Euler(-50, 30, 90));
                yield return new TestCaseData(i, Quaternion.Euler(90, 90, 180));
                yield return new TestCaseData(i, Quaternion.Euler(-20, 0, 45));
                yield return new TestCaseData(i, Quaternion.Euler(80, 30, -45));
            }
        }

        [Test]
        [TestCaseSource(nameof(CompressesAndDecompressesCases))]
#if !UNITY_EDITOR
        [Ignore("Quaternion.Euler Requires unity engine to run")]
#endif
        public void PackAndUnpack(int bits, Quaternion inValue)
        {
            RunPackAndUnpack(bits, inValue);
        }


        [Test]
        [Repeat(1000)]
#if !UNITY_EDITOR
        [Ignore("Quaternion.Euler Requires unity engine to run")]
#endif
        public void PackAndUnpackRepeat([Range(8, 12)] int bits)
        {
            Quaternion inValue = GetRandomQuaternion();

            RunPackAndUnpack(bits, inValue);
        }

        [Test]
        [Repeat(1000)]
#if !UNITY_EDITOR
        [Ignore("Quaternion.Euler Requires unity engine to run")]
#endif
        public void PackAndUnpackRepeatNotNormalized([Range(8, 12)] int bits)
        {
            Quaternion inValueNotNormalized = GetRandomQuaternionNotNormalized();

            RunPackAndUnpack(bits, inValueNotNormalized);
        }

        private void RunPackAndUnpack(int bits, Quaternion inValueNotNormalized)
        {
            Quaternion inValue = inValueNotNormalized.normalized;

            // precision for 1 element
            float max = (1 / Mathf.Sqrt(2));
            float precision = 2 * max / ((1 << bits) - 1);
            // allow extra precision because largest is caculated using the other 3 values so may be out side of precision
            precision *= 2;

            var packer = new QuaternionPacker(bits);

            packer.Pack(writer, inValueNotNormalized);

            Quaternion outValue = packer.Unpack(GetReader());
            //Debug.Log($"Packed: ({inValue.x:0.000},{inValue.y:0.000},{inValue.z:0.000},{inValue.w:0.000}) " +
            //          $"UnPacked: ({outValue.x:0.000},{outValue.y:0.000},{outValue.z:0.000},{outValue.w:0.000})");

            Assert.That(outValue.x, Is.Not.NaN, "x was NaN");
            Assert.That(outValue.y, Is.Not.NaN, "y was NaN");
            Assert.That(outValue.z, Is.Not.NaN, "z was NaN");
            Assert.That(outValue.w, Is.Not.NaN, "w was NaN");

            float assertSign = getAssertSign(inValue, outValue);

            Assert.That(outValue.x, IsUnSignedEqualWithIn(inValue.x), $"x off by {Mathf.Abs(assertSign * inValue.x - outValue.x)}");
            Assert.That(outValue.y, IsUnSignedEqualWithIn(inValue.y), $"y off by {Mathf.Abs(assertSign * inValue.y - outValue.y)}");
            Assert.That(outValue.z, IsUnSignedEqualWithIn(inValue.z), $"z off by {Mathf.Abs(assertSign * inValue.z - outValue.z)}");
            Assert.That(outValue.w, IsUnSignedEqualWithIn(inValue.w), $"w off by {Mathf.Abs(assertSign * inValue.w - outValue.w)}");

            Vector3 inVec = inValue * Vector3.forward;
            Vector3 outVec = outValue * Vector3.forward;

            // allow for extra precision when rotating vector
            Assert.AreEqual(inVec.x, outVec.x, precision * 2, $"vx off by {Mathf.Abs(inVec.x - outVec.x)}");
            Assert.AreEqual(inVec.y, outVec.y, precision * 2, $"vy off by {Mathf.Abs(inVec.y - outVec.y)}");
            Assert.AreEqual(inVec.z, outVec.z, precision * 2, $"vz off by {Mathf.Abs(inVec.z - outVec.z)}");


            EqualConstraint IsUnSignedEqualWithIn(float v)
            {
                return Is.EqualTo(v).Within(precision).Or.EqualTo(assertSign * v).Within(precision);
            }
        }


        [Test]
        [Repeat(1000)]
        public void FastNormalize()
        {
            Quaternion q1 = GetRandomQuaternionNotNormalized();
            // create copy here so it can be used in ref without chanigng q1
            Quaternion q2 = q1;
            QuaternionPacker.QuickNormalize(ref q2);

            Assert.That(q2, Is.EqualTo(q1.normalized));
        }

        /// <summary>
        /// sign used to validate values (in/out are different, then flip values
        /// </summary>
        /// <param name="inValue"></param>
        /// <param name="outValue"></param>
        /// <returns></returns>
        static float getAssertSign(Quaternion inValue, Quaternion outValue)
        {
            // keep same index for in/out because largest *might* have chagned if all elements are equal
            QuaternionPacker.FindLargestIndex(ref inValue, out uint index);

            float inLargest = inValue[(int)index];
            float outLargest = outValue[(int)index];
            // flip sign of A if largest is is negative
            // Q == (-Q)
            float inSign = Mathf.Sign(inLargest);
            float outSign = Mathf.Sign(outLargest);

            float assertSign = inSign == outSign ? 1 : -1;
            return assertSign;
        }
    }
}
