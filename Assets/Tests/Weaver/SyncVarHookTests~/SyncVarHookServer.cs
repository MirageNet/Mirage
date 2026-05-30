using Mirage;

namespace SyncVarHookTests.SyncVarHookServer
{
    class SyncVarHookServer : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth), invokeHookOnServer = true)]
        int health { get; set; }

        void onChangeHealth(int oldValue, int newValue)
        {

        }
    }
}
