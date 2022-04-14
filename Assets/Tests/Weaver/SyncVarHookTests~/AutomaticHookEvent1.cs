using Mirage;
using System;

namespace SyncVarHookTests.AutomaticHookEvent1
{
    class AutomaticHookEvent1 : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth))]
        int health;

        event Action<int> onChangeHealth;
    }
}
