using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class RpcStaticTests
    {
        private const string MockDefinitions = @"
namespace Mirage
{
    public class NetworkBehaviour {}
    public class ServerRpcAttribute : System.Attribute {}
    public class ClientRpcAttribute : System.Attribute {}
    public class RateLimitAttribute : System.Attribute {}
}
";

        [Test]
        public async Task InstanceRpcDoesNotReportWarning()
        {
            // Verify that instance-level RPC methods (both ServerRpc and ClientRpc) do not trigger MIRAGE1203
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public void CmdDoSomething() {}

    [ClientRpc]
    public void RpcDoSomething() {}
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task StaticNonRpcMethodDoesNotReportWarning()
        {
            // Verify that static methods that are not RPCs do not trigger MIRAGE1203
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    public static void LocalHelper() {}
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task FakeStaticRpcDoesNotReportWarning()
        {
            // Verify that a fake ServerRpc attribute in a different namespace does not trigger MIRAGE1203 on static methods
            var code = @"
using System;
using Mirage;

namespace Custom
{
    public class ServerRpcAttribute : Attribute {}
}

public class MyBehaviour : NetworkBehaviour
{
    [Custom.ServerRpc]
    public static void CmdFakeRpc() {}
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task StaticServerRpcReportsError()
        {
            // Verify that a static ServerRpc method triggers MIRAGE1203
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public static void {|#0:CmdDoSomething|}() {}
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1203").WithLocation(0).WithArguments("CmdDoSomething");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task StaticClientRpcReportsError()
        {
            // Verify that a static ClientRpc method triggers MIRAGE1203
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc]
    public static void {|#0:RpcDoSomething|}() {}
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1203").WithLocation(0).WithArguments("RpcDoSomething");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
