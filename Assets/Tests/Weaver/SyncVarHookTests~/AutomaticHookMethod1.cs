using Mirage;

namespace SyncVarHookTests.AutomaticHookMethod1
{
    class AutomaticHookMethod1 : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth))]
        int health;

        public void onChangeHealth(int newValue)
        {

        }
    }
}
