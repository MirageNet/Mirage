using Mirage;
using System;

namespace SyncVarHookTests.ExplicitEvent1Found
{
    class ExplicitEvent1Found : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth), hookType = SyncHookType.EventWith1Arg)]
        int health;

        event Action<int> onChangeHealth;
    }
}
