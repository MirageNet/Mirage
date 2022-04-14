using Mirage;

namespace SyncVarHookTests.ExplicitMethod2NotFound
{
    class ExplicitMethod2NotFound : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth), hookType = SyncHookType.MethodWith2Arg)]
        int health;

        public void onChangeHealth(int newValue)
        {

        }
    }
}
