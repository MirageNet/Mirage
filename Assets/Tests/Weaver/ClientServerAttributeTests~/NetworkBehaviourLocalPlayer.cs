using Mirage;

namespace ClientServerAttributeTests.NetworkBehaviourLocalPlayer
{
    class NetworkBehaviourLocalPlayer : NetworkBehaviour
    {
        [LocalPlayer]
        void LocalPlayerMethod()
        {
            // test method
        }
    }
}
