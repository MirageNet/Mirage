using Mirage;
using System;

namespace SyncVarHookTests.ErrorWhenHookNotAction
{
    delegate void DoStuff(int a, int b);
    class ErrorWhenHookNotAction : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnChangeHealth))]
        int health { get; set; }

        public event DoStuff OnChangeHealth;
    }
}
