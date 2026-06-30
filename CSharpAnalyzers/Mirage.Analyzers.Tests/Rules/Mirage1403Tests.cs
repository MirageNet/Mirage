using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1403Tests
    {
        [Test]
        public async Task CheckingActiveOrIsSpawnedDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1403Tests/CheckingActiveOrIsSpawnedDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task CheckingEnabledReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1403Tests/CheckingEnabledReportsWarning.cs");
            var expected0 = VerifyCS.Diagnostic("MIRAGE1403").WithLocation(0).WithArguments("NetworkServer");
            var expected1 = VerifyCS.Diagnostic("MIRAGE1403").WithLocation(1).WithArguments("NetworkClient");
            var expected2 = VerifyCS.Diagnostic("MIRAGE1403").WithLocation(2).WithArguments("NetworkIdentity");
            await VerifyCS.VerifyAnalyzerAsync(code, expected0, expected1, expected2);
        }
    }
}
