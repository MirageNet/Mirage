using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1207Tests
    {
        [Test]
        public async Task ServerRpcWithRateLimitDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1207Tests/Negative_ServerRpcWithRateLimitDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ClientRpcWithoutRateLimitDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1207Tests/Negative_ClientRpcWithoutRateLimitDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ServerRpcWithoutRateLimitReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1207Tests/Positive_ServerRpcWithoutRateLimitReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1207")
                .WithLocation(0)
                .WithArguments("CmdInteract");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ServerRpcWithCustomRateLimitReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1207Tests/Positive_ServerRpcWithCustomRateLimitReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1207")
                .WithLocation(0)
                .WithArguments("CmdInteract");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
