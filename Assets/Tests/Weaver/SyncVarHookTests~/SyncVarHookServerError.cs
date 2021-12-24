using Mirage;

namespace SyncVarHookTests.SyncVarHookServerError
{
    class SyncVarHookServerError : NetworkBehaviour
    {
        [SyncVar(invokeHookOnServer = true)]
        int health;
    }
}
