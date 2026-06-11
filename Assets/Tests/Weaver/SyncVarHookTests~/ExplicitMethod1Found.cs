using Mirage;

namespace SyncVarHookTests.ExplicitMethod1Found
{
    class ExplicitMethod1Found : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth), hookType = SyncHookType.MethodWith1Arg)]
        int health { get; set; }

        public void onChangeHealth(int newValue)
        {

        }
    }
}
