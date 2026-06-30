using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1101Tests
    {
        [Test]
        public async Task SyncVarOnNonNetworkBehaviourReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1101Tests/Positive_SyncVarOnNonNetworkBehaviour.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1101")
                .WithLocation(0)
                .WithArguments("SyncVarAttribute", "MySyncVar");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ServerOnNonNetworkBehaviourReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1101Tests/Positive_ServerOnNonNetworkBehaviour.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1101")
                .WithLocation(0)
                .WithArguments("ServerAttribute", "MyMethod");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ClientOnNonNetworkBehaviourReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1101Tests/Positive_ClientOnNonNetworkBehaviour.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1101")
                .WithLocation(0)
                .WithArguments("ClientAttribute", "MyMethod");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ServerRpcOnNonNetworkBehaviourReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1101Tests/Positive_ServerRpcOnNonNetworkBehaviour.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1101")
                .WithLocation(0)
                .WithArguments("ServerRpcAttribute", "MyMethod");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ClientRpcOnNonNetworkBehaviourReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1101Tests/Positive_ClientRpcOnNonNetworkBehaviour.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1101")
                .WithLocation(0)
                .WithArguments("ClientRpcAttribute", "MyMethod");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
