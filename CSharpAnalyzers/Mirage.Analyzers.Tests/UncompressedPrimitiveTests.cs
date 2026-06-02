using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class UncompressedPrimitiveTests
    {
        [Test]
        public async Task Positive_CompressedPrimitivesAndVectors()
        {
            var code = VerifyCS.LoadTestData("UncompressedPrimitive/Positive_CompressedPrimitivesAndVectors.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Positive_AllowedUncompressedTypes()
        {
            var code = VerifyCS.LoadTestData("UncompressedPrimitive/Positive_AllowedUncompressedTypes.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Negative_UncompressedSyncVars()
        {
            var code = VerifyCS.LoadTestData("UncompressedPrimitive/Negative_UncompressedSyncVars.cs");
            var expectedProp = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(0).WithArguments("Health", "Int32");
            var expectedField = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(1).WithArguments("PlayerScale", "Single");

            await VerifyCS.VerifyAnalyzerAsync(code, expectedProp, expectedField);
        }

        [Test]
        public async Task Negative_UncompressedRpcParameters()
        {
            var code = VerifyCS.LoadTestData("UncompressedPrimitive/Negative_UncompressedRpcParameters.cs");
            var expectedScore = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(0).WithArguments("score", "Int32");
            var expectedVal = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(1).WithArguments("val", "Single");
            var expectedPos = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(2).WithArguments("pos", "Vector3");
            var expectedRot = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(3).WithArguments("rot", "Quaternion");

            await VerifyCS.VerifyAnalyzerAsync(code, expectedScore, expectedVal, expectedPos, expectedRot);
        }

        [Test]
        public async Task Negative_UncompressedFieldsInMessage()
        {
            var code = VerifyCS.LoadTestData("UncompressedPrimitive/Negative_UncompressedFieldsInMessage.cs");
            var expectedTimestamp = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(0).WithArguments("timestamp", "Double");
            var expectedOffset = VerifyCS.Diagnostic("MIRAGE1503").WithLocation(1).WithArguments("offset", "Vector2");

            await VerifyCS.VerifyAnalyzerAsync(code, expectedTimestamp, expectedOffset);
        }
    }
}
