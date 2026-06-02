using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class ServerRpcRateLimitMissingTests
    {
        [Test]
        public async Task ServerRpcWithRateLimitDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("ServerRpcRateLimitMissing/ServerRpcWithRateLimitDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ClientRpcWithoutRateLimitDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("ServerRpcRateLimitMissing/ClientRpcWithoutRateLimitDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ServerRpcWithoutRateLimitReportsWarning()
        {
            var code = VerifyCS.LoadTestData("ServerRpcRateLimitMissing/ServerRpcWithoutRateLimitReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1207")
                .WithLocation(0)
                .WithArguments("CmdInteract");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ServerRpcWithCustomRateLimitReportsWarning()
        {
            var code = VerifyCS.LoadTestData("ServerRpcRateLimitMissing/ServerRpcWithCustomRateLimitReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1207")
                .WithLocation(0)
                .WithArguments("CmdInteract");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
