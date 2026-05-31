using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class RateLimitSettingsTests
    {
        private const string MockDefinitions = @"
namespace Mirage
{
    public class NetworkBehaviour {}

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ServerRpcAttribute : System.Attribute {}

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ClientRpcAttribute : System.Attribute {}

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class RateLimitAttribute : System.Attribute
    {
        public float Interval { get; set; }
        public int Refill { get; set; }
        public int MaxTokens { get; set; }
    }
}
";

        [Test]
        public async Task ValidRateLimitSettings()
        {
            // Verify rate limit settings with positive values and MaxTokens >= Refill compile and analyze cleanly
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    [RateLimit(Interval = 0.5f, Refill = 5, MaxTokens = 10)]
    public void CmdFire()
    {
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ValidRateLimitDefaultSettings()
        {
            // Verify default rate limit settings are automatically treated as valid
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    [RateLimit]
    public void CmdFire()
    {
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task InvalidRateLimitInterval()
        {
            // Verify an interval of zero or lower triggers the appropriate validation warning
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    [RateLimit(Interval = 0f)]
    public void {|#0:CmdFire|}()
    {
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1205")
                .WithLocation(0)
                .WithArguments("CmdFire", "Interval must be greater than zero");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task InvalidRateLimitRefillAndMaxTokens()
        {
            // Verify non-positive refills or tokens trigger warnings
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    [RateLimit(Refill = -5, MaxTokens = 0)]
    public void {|#0:CmdFire|}()
    {
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1205")
                .WithLocation(0)
                .WithArguments("CmdFire", "Refill must be greater than zero, MaxTokens must be greater than zero");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task InvalidRateLimitMaxTokensLessThanRefill()
        {
            // Verify that maximum allowed tokens cannot be configured below the refill amount
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    [RateLimit(Refill = 10, MaxTokens = 5)]
    public void {|#0:CmdFire|}()
    {
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1205")
                .WithLocation(0)
                .WithArguments("CmdFire", "MaxTokens must be greater than or equal to Refill");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task RateLimitOnNonRpcMethodIgnored()
        {
            // Verify that rate limit constraints are only enforced on network RPC methods
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [RateLimit(Interval = 0f, Refill = 0, MaxTokens = 0)]
    public void LocalFire()
    {
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task CustomRateLimitAttributeIgnored()
        {
            // Verify external rate limiting attributes do not trigger Mirage-specific validations
            var code = @"
using Mirage;

namespace CustomNamespace
{
    public class RateLimitAttribute : System.Attribute
    {
        public float Interval { get; set; }
        public int Refill { get; set; }
        public int MaxTokens { get; set; }
    }
}

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    [CustomNamespace.RateLimit(Interval = -1f)]
    public void CmdFire()
    {
    }
}
" + MockDefinitions;

            // Note: Since Mirage's [RateLimit] is missing on ServerRpc here, it will trigger MIRAGE1206
            var expected = VerifyCS.Diagnostic("MIRAGE1206")
                .WithLocation(0)
                .WithArguments("CmdFire");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
