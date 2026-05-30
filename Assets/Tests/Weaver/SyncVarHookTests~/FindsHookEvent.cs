using Mirage;
using System;

namespace SyncVarHookTests.FindsHookEvent
{
    class FindsHookEvent : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnChangeHealth))]
        int health { get; set; }

        public event Action<int, int> OnChangeHealth;
    }
}
