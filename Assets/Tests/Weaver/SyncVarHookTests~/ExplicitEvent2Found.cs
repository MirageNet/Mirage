using Mirage;
using System;

namespace SyncVarHookTests.ExplicitEvent2Found
{
    class ExplicitEvent2Found : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth), hookType = SyncHookType.EventWith2Arg)]
        int health { get; set; }

        event Action<int, int> onChangeHealth;
    }
}
