using Mirage;

namespace SyncVarHookTests.FindsStaticHook
{
    class FindsStaticHook : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth))]
        int health { get; set; }

        static void onChangeHealth(int oldValue, int newValue)
        {

        }
    }
}
