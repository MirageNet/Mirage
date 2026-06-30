using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1401Tests
    {
        [Test]
        public async Task Invalid_AccessInAllowedMethods()
        {
            var code = VerifyCS.LoadTestData("Mirage1401Tests/Invalid_AccessInAllowedMethods.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Invalid_NonNetworkBehaviourClass()
        {
            var code = VerifyCS.LoadTestData("Mirage1401Tests/Invalid_NonNetworkBehaviourClass.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Valid_AccessIsServerInAwake()
        {
            var code = VerifyCS.LoadTestData("Mirage1401Tests/Valid_AccessIsServerInAwake.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1401")
                .WithLocation(0)
                .WithArguments("IsServer", "Awake");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task Valid_AccessUnsafePropertiesInAwake()
        {
            var code = VerifyCS.LoadTestData("Mirage1401Tests/Valid_AccessUnsafePropertiesInAwake.cs");
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
            var expected14 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(14).WithArguments("Owner", "Awake");
            var expected15 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(15).WithArguments("IsHost", "Awake");
            var expected16 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(16).WithArguments("IsLocalClient", "Awake");
            var expected17 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(17).WithArguments("IsServerOnly", "Awake");
            var expected18 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(18).WithArguments("IsClientOnly", "Awake");
            var expected19 = VerifyCS.Diagnostic("MIRAGE1401").WithLocation(19).WithArguments("HasAuthority", "Awake");

            await VerifyCS.VerifyAnalyzerAsync(code, expected0, expected1, expected2, expected3, expected4, expected5, expected6, expected7, expected8, expected9, expected10, expected11, expected12, expected13, expected14, expected15, expected16, expected17, expected18, expected19);
        }
        [Test]
        public async Task Invalid_AccessSyncVarPropertyInStart()
        {
            var code = VerifyCS.LoadTestData("Mirage1401Tests/Invalid_AccessSyncVarPropertyInStart.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Invalid_AccessSyncVarFieldInAwake()
        {
            var code = VerifyCS.LoadTestData("Mirage1401Tests/Invalid_AccessSyncVarFieldInAwake.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Edge_NonSyncVarAccessInAwakeStart()
        {
            var code = VerifyCS.LoadTestData("Mirage1401Tests/Valid_Edge_NonSyncVarAccessInAwakeStart.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
