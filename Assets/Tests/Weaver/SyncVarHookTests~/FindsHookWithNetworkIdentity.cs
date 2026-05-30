using Mirage;


namespace SyncVarHookTests.FindsHookWithNetworkIdentity
{
    class FindsHookWithNetworkIdentity : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onTargetChanged))]
        NetworkIdentity target { get; set; }

        void onTargetChanged(NetworkIdentity oldValue, NetworkIdentity newValue)
        {

        }
    }
}
