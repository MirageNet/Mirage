using Mirage;
using System;

namespace SyncVarHookTests.ErrorWhenEventArgsAreWrong
{
    class ErrorWhenEventArgsAreWrong : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnChangeHealth))]
        int health;

        public event Action<int, float> OnChangeHealth;
    }
}
