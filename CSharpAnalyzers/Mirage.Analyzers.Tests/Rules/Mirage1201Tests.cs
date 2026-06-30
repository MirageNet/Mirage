using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1201Tests
    {
        [Test]
        public async Task StructTypeDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1201Tests/StructTypeDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ClassTypeReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1201Tests/ClassTypeReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1201")
                .WithLocation(0)
                .WithArguments("NetworkMessage field", "classField", "MyClassData");

            var expectedSerializationError = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("MyClassData", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected, expectedSerializationError);
        }

        [Test]
        public async Task WeaverSafeClassSuppressed()
        {
            var code = VerifyCS.LoadTestData("Mirage1201Tests/WeaverSafeClassSuppressed.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task UnityTypesAllowed()
        {
            var code = VerifyCS.LoadTestData("Mirage1201Tests/UnityTypesAllowed.cs");
            // Since custom serializers exist for GameObject, Transform, NetworkIdentity in Mirage, 
            // they don't warn about either serialization or class usage.
            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
