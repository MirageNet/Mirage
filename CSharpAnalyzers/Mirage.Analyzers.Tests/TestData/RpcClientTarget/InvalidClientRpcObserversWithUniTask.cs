using Mirage;
using Cysharp.Threading.Tasks;

namespace Cysharp.Threading.Tasks
{
    public struct UniTask {}
    public struct UniTask<T> {}
}

public class MyBehaviour : NetworkBehaviour
{
    [ClientRpc(target = RpcTarget.Observers)]
    public UniTask {|#0:RpcCalculate|}()
    {
        return default;
    }
}
