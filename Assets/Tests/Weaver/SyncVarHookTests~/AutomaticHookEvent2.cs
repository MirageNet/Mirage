using Mirage;
using System;

namespace SyncVarHookTests.AutomaticHookEvent2
{
    class AutomaticHookEvent2 : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth))]
        int health;

        event Action<int, int> onChangeHealth;
    }
}
