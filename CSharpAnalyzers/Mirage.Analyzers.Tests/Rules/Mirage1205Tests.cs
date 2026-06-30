using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1205Tests
    {
        [Test]
        public async Task ValidClientRpcObserversReturnsVoid()
        {
            var code = VerifyCS.LoadTestData("Mirage1205Tests/Negative_ValidClientRpcObserversReturnsVoid.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ValidClientRpcPlayerWithNetworkPlayer()
        {
            var code = VerifyCS.LoadTestData("Mirage1205Tests/Negative_ValidClientRpcPlayerWithNetworkPlayer.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ValidClientRpcPlayerWithNetworkConnection()
        {
            var code = VerifyCS.LoadTestData("Mirage1205Tests/Negative_ValidClientRpcPlayerWithNetworkConnection.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ValidClientRpcOwnerWithUniTask()
        {
            var code = VerifyCS.LoadTestData("Mirage1205Tests/Negative_ValidClientRpcOwnerWithUniTask.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task InvalidClientRpcObserversWithUniTask()
        {
            var code = VerifyCS.LoadTestData("Mirage1205Tests/Positive_InvalidClientRpcObserversWithUniTask.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1205")
                .WithLocation(0)
                .WithArguments("RpcCalculate", "must return void when target is Observers");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task InvalidClientRpcPlayerWithoutConnectionParameter()
        {
            var code = VerifyCS.LoadTestData("Mirage1205Tests/Positive_InvalidClientRpcPlayerWithoutConnectionParameter.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1205")
                .WithLocation(0)
                .WithArguments("RpcMessage", "method with target = Player requires first parameter to be INetworkPlayer or NetworkConnection");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task InvalidClientRpcPlayerWithWrongFirstParameter()
        {
            var code = VerifyCS.LoadTestData("Mirage1205Tests/Positive_InvalidClientRpcPlayerWithWrongFirstParameter.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1205")
                .WithLocation(0)
                .WithArguments("RpcMessage", "method with target = Player requires first parameter to be INetworkPlayer or NetworkConnection");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task CustomClientRpcAttributeIgnored()
        {
            var code = VerifyCS.LoadTestData("Mirage1205Tests/Negative_CustomClientRpcAttributeIgnored.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
