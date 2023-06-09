using Mirage;
using System;

namespace SyncVarHookTests.SuccessGenericAction
{
    class SuccessGenericAction : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnChangeHealth))]
        int health;

        public event Action OnChangeHealth;
    }
}
