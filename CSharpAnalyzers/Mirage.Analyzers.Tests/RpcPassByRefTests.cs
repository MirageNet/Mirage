using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class RpcPassByRefTests
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
        public async Task RpcWithValueAndRefParametersDoesNotReportWarning()
        {
            // Verify that normal RPCs with value parameters or in parameters do not trigger MIRAGE1202
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public void CmdDoSomething(int value, string message) {}

    [ClientRpc]
    public void RpcDoSomething(int value, string message) {}
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task RpcWithInParameterDoesNotReportWarning()
        {
            // Verify that 'in' parameters (which are read-only references) do not trigger MIRAGE1202
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public void CmdDoSomething(in int value) {}
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task NonRpcMethodWithRefOrOutDoesNotReportWarning()
        {
            // Verify that non-RPC methods can use ref/out parameters freely
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    public void LocalHelper(ref int value, out string result)
    {
        value += 1;
        result = ""done"";
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task FakeRpcWithRefParameterDoesNotReportWarning()
        {
            // Verify that a custom attribute with the same name in a different namespace does not trigger MIRAGE1202
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
    public void CmdFakeRpc(ref int value) {}
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ServerRpcWithRefParameterReportsError()
        {
            // Verify that a ServerRpc with a ref parameter triggers MIRAGE1202
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public void CmdDoSomething(ref int {|#0:value|}) {}
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1202").WithLocation(0).WithArguments("CmdDoSomething", "value");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ServerRpcWithOutParameterReportsError()
        {
            // Verify that a ServerRpc with an out parameter triggers MIRAGE1202
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public void CmdDoSomething(out int {|#0:value|})
    {
        value = 0;
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1202").WithLocation(0).WithArguments("CmdDoSomething", "value");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ClientRpcWithRefParameterReportsError()
        {
            // Verify that a ClientRpc with a ref parameter triggers MIRAGE1202
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc]
    public void RpcDoSomething(ref int {|#0:value|}) {}
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1202").WithLocation(0).WithArguments("RpcDoSomething", "value");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ClientRpcWithOutParameterReportsError()
        {
            // Verify that a ClientRpc with an out parameter triggers MIRAGE1202
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc]
    public void RpcDoSomething(out int {|#0:value|})
    {
        value = 0;
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1202").WithLocation(0).WithArguments("RpcDoSomething", "value");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
