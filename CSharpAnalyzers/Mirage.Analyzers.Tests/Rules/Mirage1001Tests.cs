using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1001Tests
    {
        [Test]
        public async Task SafeTypeDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1001Tests/Valid_SafeType.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ClassTypeReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1001Tests/Invalid_ClassType.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1001").WithLocation(0).WithArguments("MySyncVar", "MyClass");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ClassLevelSuppressionDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1001Tests/Valid_ClassLevelSuppression.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task PropertyLevelSuppressionDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1001Tests/Valid_PropertyLevelSuppression.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
