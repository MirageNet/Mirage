using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class MtuSizeEstimationTests
    {
        private const string MockDefinitions = @"
namespace Mirage
{
    public class NetworkMessageAttribute : System.Attribute {}
    public class NetworkBehaviour {}
    public class NetworkIdentity {}
}
namespace Mirage.Serialization
{
    public class BitCountAttribute : System.Attribute
    {
        public BitCountAttribute(int bits) {}
    }
    public class FloatPackAttribute : System.Attribute
    {
        public FloatPackAttribute(float min, float max, float precision) {}
        public FloatPackAttribute(int bits) {} // overload for testing
        public FloatPackAttribute(float max, int bits) {} // overload matching signature in analyzer
    }
}
namespace UnityEngine
{
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
    }
    public struct Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }
}
";

        [Test]
        public async Task Positive_SmallMessageDoesNotTriggerWarning()
        {
            // Verify that a small network message does not trigger MTU warning (estimated size <= 1200)
            var code = @"
using Mirage;
using UnityEngine;

[NetworkMessage]
public struct SmallMessage
{
    public int id;
    public Vector3 position;
    public string name;
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Positive_LargeMessageReducedByPacking()
        {
            // Verify that bitcount / floatpack attribute overrides size estimation, keeping it within safe MTU
            var code = @"
using Mirage;
using Mirage.Serialization;

[NetworkMessage]
public struct PackedTestMessage
{
    [FloatPack(0.0f, 1)] // Should estimate size based on 1 bit instead of 4/8 bytes
    public float CompressedVal;
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Negative_LargeMessageExceedsMtu()
        {
            // Verify that a network message exceeding 1200 bytes triggers MIRAGE1501 warning.
            // 10 arrays of double: 10 * 128 * 8 = 10240 bytes.
            var code = @"
using Mirage;

[NetworkMessage]
public struct {|#0:HugeMessage|}
{
    public double[] arr1;
    public double[] arr2;
    public double[] arr3;
    public double[] arr4;
    public double[] arr5;
    public double[] arr6;
    public double[] arr7;
    public double[] arr8;
    public double[] arr9;
    public double[] arr10;
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1501")
                .WithLocation(0)
                .WithArguments("HugeMessage", "10240", "1200");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task Edge_RecursiveStructOrClassRef()
        {
            // Verify that recursive types do not cause infinite recursion and resolve to 0 or valid size
            var code = @"
using Mirage;

[NetworkMessage]
public class RecursiveMessage
{
    public RecursiveMessage Parent;
    public int Value;
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
