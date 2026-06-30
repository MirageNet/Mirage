using Mirage;
using System;

namespace SyncVarHookTests.FindsStaticHookEvent
{
    class FindsStaticHookEvent : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnChangeHealth))]
        int health;

        public static event Action<int, int> OnChangeHealth;
    }
}
