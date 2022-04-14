using Mirage;

namespace SyncVarHookTests.ExplicitEvent2NotFound
{
    class ExplicitEvent2NotFound : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth), hookType = SyncHookType.EventWith2Arg)]
        int health;

        public void onChangeHealth(int newValue)
        {

        }
    }
}
