using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class RateLimitSettingsTests
    {
        [Test]
        public async Task ValidRateLimitSettings()
        {
            var code = VerifyCS.LoadTestData("RateLimitSettings/ValidRateLimitSettings.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ValidRateLimitDefaultSettings()
        {
            var code = VerifyCS.LoadTestData("RateLimitSettings/ValidRateLimitDefaultSettings.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task InvalidRateLimitInterval()
        {
            var code = VerifyCS.LoadTestData("RateLimitSettings/InvalidRateLimitInterval.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1205")
                .WithLocation(0)
                .WithArguments("CmdFire", "Interval must be greater than zero");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task InvalidRateLimitRefillAndMaxTokens()
        {
            var code = VerifyCS.LoadTestData("RateLimitSettings/InvalidRateLimitRefillAndMaxTokens.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1205")
                .WithLocation(0)
                .WithArguments("CmdFire", "Refill must be greater than zero, MaxTokens must be greater than zero");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task InvalidRateLimitMaxTokensLessThanRefill()
        {
            var code = VerifyCS.LoadTestData("RateLimitSettings/InvalidRateLimitMaxTokensLessThanRefill.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1205")
                .WithLocation(0)
                .WithArguments("CmdFire", "MaxTokens must be greater than or equal to Refill");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task RateLimitOnNonRpcMethodIgnored()
        {
            var code = VerifyCS.LoadTestData("RateLimitSettings/RateLimitOnNonRpcMethodIgnored.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task CustomRateLimitAttributeIgnored()
        {
            var code = VerifyCS.LoadTestData("RateLimitSettings/CustomRateLimitAttributeIgnored.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1206")
                .WithLocation(0)
                .WithArguments("CmdFire");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
