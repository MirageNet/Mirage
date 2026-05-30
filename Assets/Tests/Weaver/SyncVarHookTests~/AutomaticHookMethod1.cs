using Mirage;

namespace SyncVarHookTests.AutomaticHookMethod1
{
    class AutomaticHookMethod1 : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth))]
        int health { get; set; }

        public void onChangeHealth(int newValue)
        {

        }
    }
}
