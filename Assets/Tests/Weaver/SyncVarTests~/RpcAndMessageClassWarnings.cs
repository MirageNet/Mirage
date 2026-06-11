using Mirage;
using Cysharp.Threading.Tasks;

namespace SyncVarTests.RpcAndMessageClassWarnings
{
    class UnsafeClass
    {
        public int Value;
    }

    [WeaverSafeClass]
    class SafeClass
    {
        public int Value;
    }

    [NetworkMessage]
    struct TestMessage
    {
        public UnsafeClass UnsafeField;
        
        [WeaverSafeClass]
        public UnsafeClass SafeField;
        
        public SafeClass SafeClassField;
    }

    class RpcAndMessageClassWarnings : NetworkBehaviour
    {
        [ServerRpc]
        public void SendUnsafeRpc(UnsafeClass unsafeParam) { }

        [ServerRpc]
        public void SendSafeParamRpc([WeaverSafeClass] UnsafeClass safeParam) { }

        [ServerRpc]
        public void SendSafeClassRpc(SafeClass safeClassParam) { }

        [ServerRpc]
        public UniTask<UnsafeClass> UnsafeReturnRpc() => UniTask.FromResult<UnsafeClass>(null);

        [ServerRpc]
        [WeaverSafeClass]
        public UniTask<UnsafeClass> SafeReturnRpc() => UniTask.FromResult<UnsafeClass>(null);

        [ServerRpc]
        public UniTask<SafeClass> SafeClassReturnRpc() => UniTask.FromResult<SafeClass>(null);
    }
}
