using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class UncompressedPrimitiveTests
    {
        private const string MockDefinitions = @"
namespace Mirage
{
    public class NetworkMessageAttribute : System.Attribute {}
    public class NetworkBehaviour {}
    public class SyncVarAttribute : System.Attribute {}
    public class ServerRpcAttribute : System.Attribute {}
    public class ClientRpcAttribute : System.Attribute {}
}
namespace Mirage.Serialization
{
    public class BitCountAttribute : System.Attribute
    {
        public BitCountAttribute(int bits) {}
    }
    public class BitCountFromRangeAttribute : System.Attribute {}
    public class VarIntAttribute : System.Attribute {}
    public class VarIntBlocksAttribute : System.Attribute {}
    public class FloatPackAttribute : System.Attribute {}
    public class Vector2PackAttribute : System.Attribute {}
    public class Vector3PackAttribute : System.Attribute {}
    public class QuaternionPackAttribute : System.Attribute {}
}
namespace UnityEngine
{
    public struct Vector2
    {
        public float x;
        public float y;
    }
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
        public async Task Positive_CompressedPrimitivesAndVectors()
        {
            // Verify that decorated primitive types and Unity vectors do not trigger uncompressed warning
            var code = @"
using Mirage;
using Mirage.Serialization;
using UnityEngine;

[NetworkMessage]
public struct ValidMessage
{
    [BitCount(8)]
    public int score;

    [FloatPack]
    public float temperature;

    [Vector2Pack]
    public Vector2 velocity;

    [Vector3Pack]
    public Vector3 position;

    [QuaternionPack]
    public Quaternion rotation;
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Positive_AllowedUncompressedTypes()
        {
            // Verify that small type primitives like bool, byte, sbyte, char, short, ushort do not require compression
            var code = @"
using Mirage;

[NetworkMessage]
public struct AllowedMessage
{
    public bool isReady;
    public byte health;
    public char category;
    public short level;
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Negative_UncompressedSyncVars()
        {
            // Verify that uncompressed SyncVar properties and fields trigger warning
            var code = @"
using Mirage;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public int {|#0:Health|} { get; set; }

    [SyncVar]
    public float {|#1:PlayerScale|};
}
" + MockDefinitions;

            var expectedProp = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(0).WithArguments("Health", "Int32");
            var expectedField = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(1).WithArguments("PlayerScale", "Single");

            await VerifyCS.VerifyAnalyzerAsync(code, expectedProp, expectedField);
        }

        [Test]
        public async Task Negative_UncompressedRpcParameters()
        {
            // Verify that uncompressed RPC parameters trigger warning
            var code = @"
using Mirage;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [ServerRpc]
    public void CmdUpdateStatus(int {|#0:score|}, float {|#1:val|}) {}

    [ClientRpc]
    public void RpcUpdatePhysics(Vector3 {|#2:pos|}, Quaternion {|#3:rot|}) {}
}
" + MockDefinitions;

            var expectedScore = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(0).WithArguments("score", "Int32");
            var expectedVal = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(1).WithArguments("val", "Single");
            var expectedPos = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(2).WithArguments("pos", "Vector3");
            var expectedRot = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(3).WithArguments("rot", "Quaternion");

            await VerifyCS.VerifyAnalyzerAsync(code, expectedScore, expectedVal, expectedPos, expectedRot);
        }

        [Test]
        public async Task Negative_UncompressedFieldsInMessage()
        {
            // Verify that uncompressed fields in a NetworkMessage trigger warning
            var code = @"
using Mirage;
using UnityEngine;

[NetworkMessage]
public struct StatusMessage
{
    public double {|#0:timestamp|};
    public Vector2 {|#1:offset|};
}
" + MockDefinitions;

            var expectedTimestamp = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(0).WithArguments("timestamp", "Double");
            var expectedOffset = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(1).WithArguments("offset", "Vector2");

            await VerifyCS.VerifyAnalyzerAsync(code, expectedTimestamp, expectedOffset);
        }
    }
}
