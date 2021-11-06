using Mirage;

namespace ClientServerAttributeTests.NetworkBehaviourLocalPlayer
{
    class NetworkBehaviourLocalPlayerOnAwake : NetworkBehaviour
    {
        [LocalPlayer]
        void Awake()
        {
            // test method
        }
    }
}
