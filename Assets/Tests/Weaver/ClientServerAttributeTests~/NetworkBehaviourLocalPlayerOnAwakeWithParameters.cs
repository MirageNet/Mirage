using Mirage;

namespace ClientServerAttributeTests.NetworkBehaviourLocalPlayer
{
    class NetworkBehaviourLocalPlayerOnAwakeWithParameters : NetworkBehaviour
    {
        [LocalPlayer]
        void Awake(int fake)
        {
            // test method
        }
    }
}
