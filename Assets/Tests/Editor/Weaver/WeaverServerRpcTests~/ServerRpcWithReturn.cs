using Mirror;
using Cysharp.Threading.Tasks;

namespace WeaverServerRpcTests.ServerRpcWithReturn
{
    class ServerRpcWithReturn : NetworkBehaviour
    {
        [ServerRpc]
        UniTask<int> CmdThatIsTotallyValid() { }
    }
}
