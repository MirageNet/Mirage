using Mirage;

namespace ClientServerAttributeTests.NetworkBehaviourHasAuthority
{
    class NetworkBehaviourHasAuthorityOnAwakeWithParameters : NetworkBehaviour
    {
        [HasAuthority]
        void Awake(int fake)
        {
            // test method
        }
    }
}
