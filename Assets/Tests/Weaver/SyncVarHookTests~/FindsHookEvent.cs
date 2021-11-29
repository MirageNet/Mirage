using Mirage;
using System;

namespace SyncVarHookTests.FindsHookEvent
{
    class FindsHookEvent : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnChangeHealth))]
        int health;

        public event Action<int, int> OnChangeHealth;
    }
}
