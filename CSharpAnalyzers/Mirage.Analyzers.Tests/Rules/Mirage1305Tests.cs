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
            var code = VerifyCS.LoadTestData("Mirage1305Tests/Valid_MessageWithAttributeDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task MessageWithoutAttributeReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1305Tests/Invalid_MessageWithoutAttributeReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1305")
                .WithLocation(0)
                .WithArguments("UnattributedMessage");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task BuiltInTypesAllowed()
        {
            var code = VerifyCS.LoadTestData("Mirage1305Tests/Valid_BuiltInTypesAllowed.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task MessageWithoutAttributeInRegisterHandlerReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1305Tests/Invalid_MessageWithoutAttributeInRegisterHandlerReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1305")
                .WithLocation(0)
                .WithArguments("UnattributedMessage");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task OtherGenericMethodsWithoutAttributeReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1305Tests/Invalid_OtherGenericMethodsWithoutAttributeReportsWarning.cs");
            var expected0 = VerifyCS.Diagnostic("MIRAGE1305").WithLocation(0).WithArguments("UnattributedMessage");
            var expected1 = VerifyCS.Diagnostic("MIRAGE1305").WithLocation(1).WithArguments("UnattributedMessage");
            var expected2 = VerifyCS.Diagnostic("MIRAGE1305").WithLocation(2).WithArguments("UnattributedMessage");
            var expected3 = VerifyCS.Diagnostic("MIRAGE1305").WithLocation(3).WithArguments("UnattributedMessage");
            var expected4 = VerifyCS.Diagnostic("MIRAGE1305").WithLocation(4).WithArguments("UnattributedMessage");
            var expected5 = VerifyCS.Diagnostic("MIRAGE1305").WithLocation(5).WithArguments("UnattributedMessage");

            await VerifyCS.VerifyAnalyzerAsync(code, expected0, expected1, expected2, expected3, expected4, expected5);
        }
    }
}
