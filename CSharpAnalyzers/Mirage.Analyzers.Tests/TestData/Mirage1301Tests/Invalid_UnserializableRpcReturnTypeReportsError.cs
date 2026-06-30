using Mirage;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Cysharp.Threading.Tasks
{
    public struct UniTask {}
    public struct UniTask<T> {}
}

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public UniTask<Thread> {|#0:CmdGetSession|}() => default;
}
