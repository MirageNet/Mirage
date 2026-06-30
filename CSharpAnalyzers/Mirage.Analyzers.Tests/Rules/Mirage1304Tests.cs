using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1304Tests
    {
        [Test]
        public async Task NetworkBehaviourInMessageFieldDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1304Tests/Negative_NetworkBehaviourInMessageFieldDoesNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task NetworkBehaviourInRpcParameterDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1304Tests/Negative_NetworkBehaviourInRpcParameterDoesNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task MonoBehaviourInMessageFieldReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1304Tests/Positive_MonoBehaviourInMessageFieldReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1304")
                .WithLocation(0)
                .WithArguments("PlainMonoBehaviour", "NetworkMessage field");

            var expectedClassWarning = VerifyCS.Diagnostic("MIRAGE1201")
                .WithLocation(0)
                .WithArguments("NetworkMessage field", "myMonoBehaviour", "PlainMonoBehaviour");

            await VerifyCS.VerifyAnalyzerAsync(code, expected, expectedClassWarning);
        }

        [Test]
        public async Task MonoBehaviourInRpcParameterReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1304Tests/Positive_MonoBehaviourInRpcParameterReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1304")
                .WithLocation(0)
                .WithArguments("PlainMonoBehaviour", "RPC parameter");

            var expectedClassWarning = VerifyCS.Diagnostic("MIRAGE1201")
                .WithLocation(0)
                .WithArguments("RPC parameter", "target", "PlainMonoBehaviour");

            await VerifyCS.VerifyAnalyzerAsync(code, expected, expectedClassWarning);
        }
    }
}
