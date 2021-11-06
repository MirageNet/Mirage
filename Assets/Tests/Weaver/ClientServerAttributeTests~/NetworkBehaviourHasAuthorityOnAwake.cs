using Mirage;

namespace ClientServerAttributeTests.NetworkBehaviourHasAuthority
{
    class NetworkBehaviourHasAuthorityOnAwake : NetworkBehaviour
    {
        [HasAuthority]
        void Awake()
        {
            // test method
        }
    }
}
