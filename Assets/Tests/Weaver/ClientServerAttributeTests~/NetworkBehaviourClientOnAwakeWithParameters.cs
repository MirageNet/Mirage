using Mirage;

namespace ClientServerAttributeTests.NetworkBehaviourClient
{
    class NetworkBehaviourClientOnAwakeWithParameters : NetworkBehaviour
    {
        [Client]
        void Awake(int fake)
        {
            // test method
        }
    }
}
