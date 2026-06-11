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
        public async Task Negative_AccessUnsafePropertiesInAwake()
        {
            var code = VerifyCS.LoadTestData("AwakeStart/Negative_AccessUnsafePropertiesInAwake.cs");
            var expected0 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(0).WithArguments("Server", "Awake");
            var expected1 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(1).WithArguments("Client", "Awake");
            var expected2 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(2).WithArguments("World", "Awake");
            var expected3 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(3).WithArguments("ServerObjectManager", "Awake");
            var expected4 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(4).WithArguments("ClientObjectManager", "Awake");
            var expected5 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(5).WithArguments("Visibility", "Awake");
            var expected6 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(6).WithArguments("SyncVarSender", "Awake");
            var expected7 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(7).WithArguments("MyServerRpc", "Awake");
            var expected8 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(8).WithArguments("MyClientRpc", "Awake");
            var expected9 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(9).WithArguments("MyServerMethod", "Awake");
            var expected10 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(10).WithArguments("MyClientMethod", "Awake");
            var expected11 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(11).WithArguments("MyHasAuthorityMethod", "Awake");
            var expected12 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(12).WithArguments("MyLocalPlayerMethod", "Awake");
            var expected13 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(13).WithArguments("MyNetworkFlagsMethod", "Awake");

            await VerifyCS.VerifyAnalyzerAsync(code, expected0, expected1, expected2, expected3, expected4, expected5, expected6, expected7, expected8, expected9, expected10, expected11, expected12, expected13);
        }
        [Test]
        public async Task Positive_AccessSyncVarPropertyInStart()
        {
            var code = VerifyCS.LoadTestData("AwakeStart/Positive_AccessSyncVarPropertyInStart.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Positive_AccessSyncVarFieldInAwake()
        {
            var code = VerifyCS.LoadTestData("AwakeStart/Positive_AccessSyncVarFieldInAwake.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Edge_NonSyncVarAccessInAwakeStart()
        {
            var code = VerifyCS.LoadTestData("AwakeStart/Edge_NonSyncVarAccessInAwakeStart.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
