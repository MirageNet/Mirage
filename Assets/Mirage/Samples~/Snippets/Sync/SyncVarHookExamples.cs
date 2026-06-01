using System;
using Mirage;

namespace Mirage.Snippets.Sync.VarHooks
{
    public class SyncVarHookAttributeExample : NetworkBehaviour
    {
        // CodeEmbed-Start: SyncVarHookAttribute
        [SyncVar(hook = nameof(HookName))]
        // CodeEmbed-End: SyncVarHookAttribute
        public int myValue;

        void HookName(int oldValue, int newValue) {}
    }

    public class HookSignaturesExample
    {
        // CodeEmbed-Start: HookSignatures
        void hook0() { }

        void hook1(int newValue) { }

        void hook2(int oldValue, int newValue) { }

        event Action event0;

        event Action<int> event1;

        event Action<int, int> event2;
        // CodeEmbed-End: HookSignatures
    }
}
