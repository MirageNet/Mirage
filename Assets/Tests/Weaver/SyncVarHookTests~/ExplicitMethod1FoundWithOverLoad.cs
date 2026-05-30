using Mirage;

namespace SyncVarHookTests.ExplicitMethod1FoundWithOverLoad
{
    class ExplicitMethod1FoundWithOverLoad : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth), hookType = SyncHookType.MethodWith1Arg)]
        int health { get; set; }

        public void onChangeHealth(int newValue)
        {

        }

        public void onChangeHealth(int oldValue, int newValue)
        {

        }
    }
}
