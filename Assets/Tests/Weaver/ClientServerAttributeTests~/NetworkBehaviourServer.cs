using Mirage;

namespace ClientServerAttributeTests.NetworkBehaviourServer
{
    class NetworkBehaviourServer : NetworkBehaviour
    {
        [Server]
        void ServerOnlyMethod()
        {
            // test method
        }
    }
}
