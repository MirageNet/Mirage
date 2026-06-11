using Mirage;
using System;

namespace SyncVarHookTests.ErrorWhenEventArgsAreWrong
{
    class ErrorWhenEventArgsAreWrong : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnChangeHealth))]
        int health { get; set; }

        public event Action<int, float> OnChangeHealth;
    }
}
