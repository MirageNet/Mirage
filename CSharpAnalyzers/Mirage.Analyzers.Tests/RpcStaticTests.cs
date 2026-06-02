using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class RpcStaticTests
    {
        [Test]
        public async Task InstanceRpcDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("RpcStatic/InstanceRpcDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task StaticNonRpcMethodDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("RpcStatic/StaticNonRpcMethodDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task FakeStaticRpcDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("RpcStatic/FakeStaticRpcDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task StaticServerRpcReportsError()
        {
            var code = VerifyCS.LoadTestData("RpcStatic/StaticServerRpcReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1204").WithLocation(0).WithArguments("CmdDoSomething");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task StaticClientRpcReportsError()
        {
            var code = VerifyCS.LoadTestData("RpcStatic/StaticClientRpcReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1204").WithLocation(0).WithArguments("RpcDoSomething");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
