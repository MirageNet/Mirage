using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class SyncVarClassTests
    {
        [Test]
        public async Task SafeTypeDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("SyncVars/SafeTypeDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ClassTypeReportsWarning()
        {
            var code = VerifyCS.LoadTestData("SyncVars/ClassTypeReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1001").WithLocation(0).WithArguments("MySyncVar", "MyClass");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ClassLevelSuppressionDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("SyncVars/ClassLevelSuppression.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task PropertyLevelSuppressionDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("SyncVars/PropertyLevelSuppression.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
