using Mirage;

namespace SyncVarHookTests.AutomaticHookMethod2
{
    class AutomaticHookMethod2 : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth))]
        int health;

        public void onChangeHealth(int oldValue, int newValue)
        {

        }
    }
}
