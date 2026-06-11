using Mirage;

namespace SyncVarHookTests.ErrorForWrongTypeOldParameter
{
    class ErrorForWrongTypeOldParameter : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth))]
        int health { get; set; }

        void onChangeHealth(float wrongOldValue, int newValue)
        {

        }
    }
}
