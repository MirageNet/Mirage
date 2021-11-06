using Mirage;

namespace ClientServerAttributeTests.NetworkBehaviourClient
{
    class NetworkBehaviourClientOnAwake : NetworkBehaviour
    {
        [Client]
        void Awake()
        {
            // test method
        }
    }
}
