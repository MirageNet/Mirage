using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1206Tests
    {
        [Test]
        public async Task ValidRateLimitSettings()
        {
            var code = VerifyCS.LoadTestData("Mirage1206Tests/ValidRateLimitSettings.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ValidRateLimitDefaultSettings()
        {
            var code = VerifyCS.LoadTestData("Mirage1206Tests/ValidRateLimitDefaultSettings.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task InvalidRateLimitInterval()
        {
            var code = VerifyCS.LoadTestData("Mirage1206Tests/InvalidRateLimitInterval.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1206")
                .WithLocation(0)
                .WithArguments("CmdFire", "Interval must be greater than zero");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task InvalidRateLimitRefillAndMaxTokens()
        {
            var code = VerifyCS.LoadTestData("Mirage1206Tests/InvalidRateLimitRefillAndMaxTokens.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1206")
                .WithLocation(0)
                .WithArguments("CmdFire", "Refill must be greater than zero, MaxTokens must be greater than zero");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task InvalidRateLimitMaxTokensLessThanRefill()
        {
            var code = VerifyCS.LoadTestData("Mirage1206Tests/InvalidRateLimitMaxTokensLessThanRefill.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1206")
                .WithLocation(0)
                .WithArguments("CmdFire", "MaxTokens must be greater than or equal to Refill");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task RateLimitOnNonRpcMethodIgnored()
        {
            var code = VerifyCS.LoadTestData("Mirage1206Tests/RateLimitOnNonRpcMethodIgnored.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task CustomRateLimitAttributeIgnored()
        {
            var code = VerifyCS.LoadTestData("Mirage1206Tests/CustomRateLimitAttributeIgnored.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1207")
                .WithLocation(0)
                .WithArguments("CmdFire");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
