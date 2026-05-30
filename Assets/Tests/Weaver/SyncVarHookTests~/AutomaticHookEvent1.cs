using Mirage;
using System;

namespace SyncVarHookTests.AutomaticHookEvent1
{
    class AutomaticHookEvent1 : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth))]
        int health { get; set; }

        event Action<int> onChangeHealth;
    }
}
