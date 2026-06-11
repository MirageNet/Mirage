using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class MtuSizeEstimationTests
    {
        [Test]
        public async Task SmallMessageTriggersInfo()
        {
            var code = VerifyCS.LoadTestData("MtuSizeEstimation/Positive_SmallMessageDoesNotTriggerWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1501")
                .WithLocation(0)
                .WithArguments("SmallMessage", "14+ (Average ~15 + dynamic content)");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task LargeMessageReducedByPackingTriggersInfo()
        {
            var code = VerifyCS.LoadTestData("MtuSizeEstimation/Positive_LargeMessageReducedByPacking.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1501")
                .WithLocation(0)
                .WithArguments("PackedTestMessage", "1");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task LargeMessageTriggersInfo()
        {
            var code = VerifyCS.LoadTestData("MtuSizeEstimation/Negative_LargeMessageExceedsMtu.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1501")
                .WithLocation(0)
                .WithArguments("HugeMessage", "10+ (Average ~10 + dynamic content)");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task RecursiveStructOrClassRefTriggersInfo()
        {
            var code = VerifyCS.LoadTestData("MtuSizeEstimation/Edge_RecursiveStructOrClassRef.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1501")
                .WithLocation(0)
                .WithArguments("RecursiveMessage", "2 to 6 (Average ~3)");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
