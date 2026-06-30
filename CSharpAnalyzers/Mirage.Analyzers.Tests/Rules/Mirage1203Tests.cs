using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1203Tests
    {
        [Test]
        public async Task RpcWithValueAndRefParametersDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1203Tests/Negative_RpcWithValueAndRefParametersDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task RpcWithInParameterDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1203Tests/Negative_RpcWithInParameterDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task NonRpcMethodWithRefOrOutDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1203Tests/Negative_NonRpcMethodWithRefOrOutDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task FakeRpcWithRefParameterDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1203Tests/Negative_FakeRpcWithRefParameterDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ServerRpcWithRefParameterReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1203Tests/Positive_ServerRpcWithRefParameterReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1203").WithLocation(0).WithArguments("CmdDoSomething", "value");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ServerRpcWithOutParameterReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1203Tests/Positive_ServerRpcWithOutParameterReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1203").WithLocation(0).WithArguments("CmdDoSomething", "value");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ClientRpcWithRefParameterReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1203Tests/Positive_ClientRpcWithRefParameterReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1203").WithLocation(0).WithArguments("RpcDoSomething", "value");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ClientRpcWithOutParameterReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1203Tests/Positive_ClientRpcWithOutParameterReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1203").WithLocation(0).WithArguments("RpcDoSomething", "value");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
