using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1202Tests
    {
        [Test]
        public async Task GenericRpcReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1202Tests/GenericRpc.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1202")
                .WithLocation(0)
                .WithArguments("CmdGeneric", "cannot have generic parameters");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task RpcWithInvalidReturnTypeReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1202Tests/RpcWithInvalidReturnType.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1202")
                .WithLocation(0)
                .WithArguments("CmdReturnsInt", "cannot return 'int' (must return void or UniTask)");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ValidVoidRpcDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1202Tests/ValidVoidRpc.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ValidUniTaskRpcDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1202Tests/ValidUniTaskRpc.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ValidGenericUniTaskRpcDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1202Tests/ValidGenericUniTaskRpc.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
