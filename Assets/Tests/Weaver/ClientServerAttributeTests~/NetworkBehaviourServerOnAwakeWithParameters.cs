using Mirage;

namespace ClientServerAttributeTests.NetworkBehaviourServer
{
    class NetworkBehaviourServerOnAwakeWithParameters : NetworkBehaviour
    {
        [Server]
        void Awake(int fake)
        {
            // test method
        }
    }
}
