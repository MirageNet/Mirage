using Mirage;
using Mirage.Serialization;

namespace MaxLengthAttributeTests.MaxLengthInvalid
{
    public struct MyCustomStruct
    {
        public int value;
    }

    public class MyBehaviour : NetworkBehaviour
    {
        [ServerRpc]
        public void SendInvalid([MaxLength(5)] int value)
        {
        }

        [ServerRpc]
        public void SendInvalidCustom([MaxLength(8)] MyCustomStruct custom)
        {
        }

        [ServerRpc]
        public void SendInvalidZero([MaxLength(0)] string message)
        {
        }

        [ServerRpc]
        public void SendInvalidNegative([MaxLength(-5)] string message)
        {
        }
    }
}
