using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1302Tests
    {
        [Test]
        public async Task PublicFieldInNetworkMessageDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1302Tests/Negative_PublicFieldInNetworkMessageDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task PrivateFieldInNetworkMessageReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1302Tests/Positive_PrivateFieldInNetworkMessageReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1302")
                .WithLocation(0)
                .WithArguments("secretCode", "MyMessage");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task StaticPrivateFieldDoesNotWarn()
        {
            var code = VerifyCS.LoadTestData("Mirage1302Tests/Negative_StaticPrivateFieldDoesNotWarn.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ExplicitNonSerializedPrivateFieldDoesNotWarn()
        {
            var code = VerifyCS.LoadTestData("Mirage1302Tests/Negative_ExplicitNonSerializedPrivateFieldDoesNotWarn.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
