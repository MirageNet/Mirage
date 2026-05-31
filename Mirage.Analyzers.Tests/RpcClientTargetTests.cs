using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class RpcClientTargetTests
    {
        private const string MockDefinitions = @"
namespace Mirage
{
    public class NetworkBehaviour {}
    
    public enum RpcTarget
    {
        Owner = 0,
        Observers = 1,
        Player = 2
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ClientRpcAttribute : System.Attribute
    {
        public RpcTarget target { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ServerRpcAttribute : System.Attribute {}

    public interface INetworkPlayer {}
    public class NetworkPlayer : INetworkPlayer {}
    public class NetworkConnection {}
}

namespace Cysharp.Threading.Tasks
{
    public struct UniTask {}
    public struct UniTask<T> {}
}
";

        [Test]
        public async Task ValidClientRpcObserversReturnsVoid()
        {
            // Verify client RPCs default to Observers and allow void returns safely
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc]
    public void RpcUpdate(int value)
    {
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ValidClientRpcPlayerWithNetworkPlayer()
        {
            // Verify targeted client RPCs allow INetworkPlayer as target parameters
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc(target = RpcTarget.Player)]
    public void RpcMessage(INetworkPlayer player, string msg)
    {
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ValidClientRpcPlayerWithNetworkConnection()
        {
            // Verify targeted client RPCs allow NetworkConnection as target parameters
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc(target = RpcTarget.Player)]
    public void RpcMessage(NetworkConnection conn, string msg)
    {
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ValidClientRpcOwnerWithUniTask()
        {
            // Verify targeted client RPCs to the owner can return UniTask values
            var code = @"
using Mirage;
using Cysharp.Threading.Tasks;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc(target = RpcTarget.Owner)]
    public UniTask RpcCalculate()
    {
        return default;
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task InvalidClientRpcObserversWithUniTask()
        {
            // Verify broadcast RPCs cannot return task values because of multi-client response ambiguity
            var code = @"
using Mirage;
using Cysharp.Threading.Tasks;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc(target = RpcTarget.Observers)]
    public UniTask {|#0:RpcCalculate|}()
    {
        return default;
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1204")
                .WithLocation(0)
                .WithArguments("RpcCalculate", "must return void when target is Observers");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task InvalidClientRpcPlayerWithoutConnectionParameter()
        {
            // Verify targeted client RPCs require a player connection identifier as the first parameter
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc(target = RpcTarget.Player)]
    public void {|#0:RpcMessage|}(string msg)
    {
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1204")
                .WithLocation(0)
                .WithArguments("RpcMessage", "method with target = Player requires first parameter to be INetworkPlayer or NetworkConnection");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task InvalidClientRpcPlayerWithWrongFirstParameter()
        {
            // Verify targeted client RPCs reject non-connection types for the first parameter
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc(target = RpcTarget.Player)]
    public void {|#0:RpcMessage|}(int connectionId, string msg)
    {
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1204")
                .WithLocation(0)
                .WithArguments("RpcMessage", "method with target = Player requires first parameter to be INetworkPlayer or NetworkConnection");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task CustomClientRpcAttributeIgnored()
        {
            // Verify the analyzer does not run checks on client RPC attributes defined outside of Mirage
            var code = @"
namespace CustomNamespace
{
    public class ClientRpcAttribute : System.Attribute
    {
        public int target { get; set; }
    }
}

public class MyBehaviour
{
    [CustomNamespace.ClientRpc(target = 2)]
    public void RpcMessage(int connectionId)
    {
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
