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
            var code = VerifyCS.LoadTestData("Mirage1202Tests/Positive_GenericRpc.cs");
            var expected1 = VerifyCS.Diagnostic("MIRAGE1202")
                .WithLocation(0)
                .WithArguments("CmdGeneric", "cannot have generic parameters");

            var expected2 = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(1)
                .WithArguments("T", "RPC parameter");

            await VerifyCS.VerifyAnalyzerAsync(code, expected1, expected2);
        }

        [Test]
        public async Task RpcWithInvalidReturnTypeReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1202Tests/Positive_RpcWithInvalidReturnType.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1202")
                .WithLocation(0)
                .WithArguments("CmdReturnsInt", "cannot return 'int' (must return void or UniTask<T>)");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task UniTaskRpcReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1202Tests/Positive_UniTaskRpc.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1202")
                .WithLocation(0)
                .WithArguments("CmdReturnsUniTask", "cannot return 'Cysharp.Threading.Tasks.UniTask' (must return void or UniTask<T>)");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ValidVoidRpcDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1202Tests/Negative_ValidVoidRpc.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task GenericBehaviourWithRpcDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1202Tests/Negative_GenericBehaviourWithRpc.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ValidGenericUniTaskRpcDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1202Tests/Negative_ValidGenericUniTaskRpc.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
