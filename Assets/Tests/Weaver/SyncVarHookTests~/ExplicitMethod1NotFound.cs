using Mirage;
using System;

namespace SyncVarHookTests.ExplicitMethod1NotFound
{
    class ExplicitMethod1NotFound : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth), hookType = SyncHookType.MethodWith1Arg)]
        int health { get; set; }

        event Action<int> onChangeHealth;
    }
}
