using Mirage;
using System;

namespace SyncVarHookTests.ExplicitEvent1NotFound
{
    class ExplicitEvent1NotFound : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth), hookType = SyncHookType.EventWith1Arg)]
        int health;

        event Action<int, int> onChangeHealth;
    }
}
