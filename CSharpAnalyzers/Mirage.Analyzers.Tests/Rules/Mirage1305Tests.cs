using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1305Tests
    {
        [Test]
        public async Task MessageWithAttributeDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1305Tests/MessageWithAttributeDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task MessageWithoutAttributeReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1305Tests/MessageWithoutAttributeReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1305")
                .WithLocation(0)
                .WithArguments("UnattributedMessage");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task BuiltInTypesAllowed()
        {
            var code = VerifyCS.LoadTestData("Mirage1305Tests/BuiltInTypesAllowed.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task MessageWithoutAttributeInRegisterHandlerReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1305Tests/MessageWithoutAttributeInRegisterHandlerReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1305")
                .WithLocation(0)
                .WithArguments("UnattributedMessage");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
