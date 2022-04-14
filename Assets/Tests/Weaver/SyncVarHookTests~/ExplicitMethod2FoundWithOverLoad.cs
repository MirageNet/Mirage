using Mirage;

namespace SyncVarHookTests.ExplicitMethod2FoundWithOverLoad
{
    class ExplicitMethod2FoundWithOverLoad : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth), hookType = SyncHookType.MethodWith2Arg)]
        int health;

        public void onChangeHealth(int newValue)
        {

        }

        public void onChangeHealth(int oldValue, int newValue)
        {

        }
    }
}
