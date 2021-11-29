using Mirage;
using System;

namespace SyncVarHookTests.ErrorWhenNotGenericAction
{
    class ErrorWhenNotGenericAction : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnChangeHealth))]
        int health;

        public event Action OnChangeHealth;
    }
}
