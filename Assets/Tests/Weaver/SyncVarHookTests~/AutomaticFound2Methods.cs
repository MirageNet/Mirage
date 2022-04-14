using Mirage;

namespace SyncVarHookTests.AutomaticFound2Methods
{
    class AutomaticFound2Methods : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth))]
        int health;

        public void onChangeHealth(int oldValue, int newValue)
        {

        }

        public void onChangeHealth(int newValue)
        {

        }
    }
}
