using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class ClientServerAttributeTests : TestsBuildFromTestName
    {
        [Test]
        public void NetworkBehaviourServer()
        {
            IsSuccess();
        }

        [Test]
        public void NetworkBehaviourClient()
        {
            IsSuccess();
        }

        [Test]
        public void NetworkBehaviourHasAuthority()
        {
            IsSuccess();
        }

        [Test]
        public void NetworkBehaviourLocalPlayer()
        {
            IsSuccess();
        }
    }
}
