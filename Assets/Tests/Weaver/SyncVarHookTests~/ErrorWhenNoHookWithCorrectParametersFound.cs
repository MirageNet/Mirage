using Mirage;

namespace SyncVarHookTests.ErrorWhenNoHookWithCorrectParametersFound
{
    class ErrorWhenNoHookWithCorrectParametersFound : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth))]
        int health { get; set; }

        void onChangeHealth(int someOtherValue, int moreValue, bool anotherValue)
        {

        }
    }
}
