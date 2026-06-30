using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1004Tests
    {
        [Test]
        public async Task ValidHookSignatureDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1004Tests/ValidHookSignatureDoesNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task HookSignatureWithSingleParameterDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1004Tests/HookSignatureWithSingleParameterDoesNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task HookSignatureMismatchReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1004Tests/HookSignatureMismatchReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1004")
                .WithLocation(0)
                .WithArguments("OnHealthChanged", "parameter type mismatch (must be 'Int32' or 'Int32, Int32')");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task HookMethodNotFoundReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1004Tests/HookMethodNotFoundReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1004")
                .WithLocation(0)
                .WithArguments("SomeNonExistentMethod", "could not find hook method with name 'SomeNonExistentMethod'");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task HookMethodIsStaticReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1004Tests/HookMethodIsStaticReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1004")
                .WithLocation(0)
                .WithArguments("OnHealthChanged", "hook method cannot be static");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
