using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class AwakeStartNetworkStateTests
    {
        [Test]
        public async Task Positive_AccessInAllowedMethods()
        {
            var code = VerifyCS.LoadTestData("AwakeStart/Positive_AccessInAllowedMethods.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Positive_NonNetworkBehaviourClass()
        {
            var code = VerifyCS.LoadTestData("AwakeStart/Positive_NonNetworkBehaviourClass.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Negative_AccessIsServerInAwake()
        {
            var code = VerifyCS.LoadTestData("AwakeStart/Negative_AccessIsServerInAwake.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1401")
                .WithLocation(0)
                .WithArguments("IsServer", "Awake");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task Negative_AccessSyncVarPropertyInStart()
        {
            var code = VerifyCS.LoadTestData("AwakeStart/Negative_AccessSyncVarPropertyInStart.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1401")
                .WithLocation(0)
                .WithArguments("Health", "Start");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task Negative_AccessSyncVarFieldInAwake()
        {
            var code = VerifyCS.LoadTestData("AwakeStart/Negative_AccessSyncVarFieldInAwake.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1401")
                .WithLocation(0)
                .WithArguments("points", "Awake");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task Edge_NonSyncVarAccessInAwakeStart()
        {
            var code = VerifyCS.LoadTestData("AwakeStart/Edge_NonSyncVarAccessInAwakeStart.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
