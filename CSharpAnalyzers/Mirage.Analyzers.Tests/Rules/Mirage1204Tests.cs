using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1204Tests
    {
        [Test]
        public async Task InstanceRpcDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1204Tests/Valid_InstanceRpcDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task StaticNonRpcMethodDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1204Tests/Valid_StaticNonRpcMethodDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task FakeStaticRpcDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1204Tests/Valid_FakeStaticRpcDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task StaticServerRpcReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1204Tests/Invalid_StaticServerRpcReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1204").WithLocation(0).WithArguments("CmdDoSomething");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task StaticClientRpcReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1204Tests/Invalid_StaticClientRpcReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1204").WithLocation(0).WithArguments("RpcDoSomething");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
