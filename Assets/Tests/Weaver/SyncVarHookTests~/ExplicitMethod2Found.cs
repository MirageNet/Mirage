using Mirage;

namespace SyncVarHookTests.ExplicitMethod2Found
{
    class ExplicitMethod2Found : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth), hookType = SyncHookType.MethodWith2Arg)]
        int health;

        public void onChangeHealth(int oldValue, int newValue)
        {

        }
    }
}
