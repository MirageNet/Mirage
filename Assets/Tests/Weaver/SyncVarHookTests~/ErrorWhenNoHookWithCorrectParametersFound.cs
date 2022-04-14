using Mirage;

namespace SyncVarHookTests.ErrorWhenNoHookWithCorrectParametersFound
{
    class ErrorWhenNoHookWithCorrectParametersFound : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth))]
        int health;

        void onChangeHealth()
        {

        }

        void onChangeHealth(int someOtherValue, int moreValue, bool anotherValue)
        {

        }
    }
}
