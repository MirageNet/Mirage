using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class ServerRpcRateLimitMissingTests
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
        public async Task ServerRpcWithRateLimitDoesNotReportWarning()
        {
            // Verify that server RPCs with a rate limit are considered secure and do not warn
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    [RateLimit(Interval = 1f, Refill = 10, MaxTokens = 10)]
    public void CmdInteract()
    {
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ClientRpcWithoutRateLimitDoesNotReportWarning()
        {
            // Verify client-bound RPCs do not require rate limits since they run on trusted clients
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc]
    public void RpcUpdateInteract()
    {
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ServerRpcWithoutRateLimitReportsWarning()
        {
            // Verify that server RPCs without rate limits warn to prevent potential spam attacks
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    public void {|#0:CmdInteract|}()
    {
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1206")
                .WithLocation(0)
                .WithArguments("CmdInteract");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ServerRpcWithCustomRateLimitReportsWarning()
        {
            // Verify custom external rate limiting attributes are not recognized by Mirage's defense rule
            var code = @"
using Mirage;

namespace CustomNamespace
{
    public class RateLimitAttribute : System.Attribute {}
}

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    [CustomNamespace.RateLimit]
    public void {|#0:CmdInteract|}()
    {
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1206")
                .WithLocation(0)
                .WithArguments("CmdInteract");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
