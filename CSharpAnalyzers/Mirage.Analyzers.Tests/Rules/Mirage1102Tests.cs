using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1102Tests
    {
        [Test]
        public async Task NonRedundantAttributesDoNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1102Tests/Valid_NonRedundantAttributesDoNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task RedundantAttributesReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1102Tests/Invalid_RedundantAttributesReportWarning.cs");
            var expected0 = VerifyCS.Diagnostic("MIRAGE1102").WithLocation(0).WithArguments("Server", "ServerRpc", "CmdFireWeapon");
            var expected1 = VerifyCS.Diagnostic("MIRAGE1102").WithLocation(1).WithArguments("Client", "ClientRpc", "RpcPlayExplosion");
            await VerifyCS.VerifyAnalyzerAsync(code, expected0, expected1);
        }
    }
}
