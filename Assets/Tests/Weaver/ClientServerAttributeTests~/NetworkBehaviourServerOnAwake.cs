using Mirage;

namespace ClientServerAttributeTests.NetworkBehaviourServer
{
    class NetworkBehaviourServerOnAwake : NetworkBehaviour
    {
        [Server]
        void Awake()
        {
            // test method
        }
    }
}
