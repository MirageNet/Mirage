using Mirage;

namespace SyncVarHookTests.AutomaticNotFound
{
    class AutomaticNotFound : NetworkBehaviour
    {
        [SyncVar(hook = "onChangeHealth")]
        int health;
    }
}
