using Mirage;

namespace SyncVarHookTests.FindsPublicHook
{
    class FindsPublicHook : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth))]
        int health { get; set; }

        public void onChangeHealth(int oldValue, int newValue)
        {

        }
    }
}
