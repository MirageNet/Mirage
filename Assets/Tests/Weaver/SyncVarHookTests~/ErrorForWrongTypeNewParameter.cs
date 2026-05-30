using Mirage;

namespace SyncVarHookTests.ErrorForWrongTypeNewParameter
{
    class ErrorForWrongTypeNewParameter : NetworkBehaviour
    {
        [SyncVar(hook = nameof(onChangeHealth))]
        int health { get; set; }

        void onChangeHealth(int oldValue, float wrongNewValue)
        {

        }
    }
}
