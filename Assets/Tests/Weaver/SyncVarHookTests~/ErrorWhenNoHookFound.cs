using Mirage;

namespace SyncVarHookTests.ErrorWhenNoHookFound
{
    class ErrorWhenNoHookFound : NetworkBehaviour
    {
        [SyncVar(hook = "onChangeHealth")]
        int health;
    }
}
