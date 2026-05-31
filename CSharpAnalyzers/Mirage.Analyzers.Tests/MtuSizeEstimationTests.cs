using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class MtuSizeEstimationTests
    {
        [Test]
        public async Task Positive_SmallMessageDoesNotTriggerWarning()
        {
            var code = VerifyCS.LoadTestData("MtuSizeEstimation/Positive_SmallMessageDoesNotTriggerWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Positive_LargeMessageReducedByPacking()
        {
            var code = VerifyCS.LoadTestData("MtuSizeEstimation/Positive_LargeMessageReducedByPacking.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Negative_LargeMessageExceedsMtu()
        {
            var code = VerifyCS.LoadTestData("MtuSizeEstimation/Negative_LargeMessageExceedsMtu.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1501")
                .WithLocation(0)
                .WithArguments("HugeMessage", "10240", "1200");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task Edge_RecursiveStructOrClassRef()
        {
            var code = VerifyCS.LoadTestData("MtuSizeEstimation/Edge_RecursiveStructOrClassRef.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
