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
            var code = VerifyCS.LoadTestData("Mirage1004Tests/Negative_ValidHookSignatureDoesNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task HookSignatureWithSingleParameterDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1004Tests/Negative_HookSignatureWithSingleParameterDoesNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task HookSignatureMismatchReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1004Tests/Positive_HookSignatureMismatchReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1004")
                .WithLocation(0)
                .WithArguments("OnHealthChanged", "parameter type mismatch (must be 'Int32' or 'Int32, Int32')");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task HookMethodNotFoundReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1004Tests/Positive_HookMethodNotFoundReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1004")
                .WithLocation(0)
                .WithArguments("SomeNonExistentMethod", "could not find hook method with name 'SomeNonExistentMethod'");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task HookEventCanBeStaticDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1004Tests/Negative_HookEventCanBeStaticDoesNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task HookMethodCanBeStaticDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1004Tests/Negative_HookMethodCanBeStaticDoesNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task EventHookSignatureDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1004Tests/Negative_EventHookSignatureDoesNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
