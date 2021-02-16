using Mirage;

namespace SyncVarHookTests.FindsPrivateHook
{
    class FindsPrivateHook : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth))]
        int health;

        void onChangeHealth(int oldValue, int newValue)
        {

        }
    }
}
