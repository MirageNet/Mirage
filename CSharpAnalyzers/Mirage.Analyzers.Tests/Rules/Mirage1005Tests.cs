using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1005Tests
    {
        [Test]
        public async Task MutableSyncVarDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1005Tests/Negative_MutableSyncVarDoesNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ReadonlySyncVarReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1005Tests/Positive_ReadonlySyncVarReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1005")
                .WithLocation(0)
                .WithArguments("health");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
