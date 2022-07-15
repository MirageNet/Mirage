using NUnit.Framework;

namespace Mirage.Tests.Runtime.Host.Authenticators
{
    public abstract class AuthenticatorHostModeBase : HostSetup<MockComponent>
    {
        protected abstract void AddAuthenticator();

        private int serverAuthCalled;
        private int clientAuthCalled;

        public sealed override void ExtraSetup()
        {
            AddAuthenticator();

            // reset fields
            serverAuthCalled = 0;
            clientAuthCalled = 0;

            server.Authenticated.AddListener(_ => serverAuthCalled++);
            client.Authenticated.AddListener(_ => clientAuthCalled++);
        }

        [Test]
        public void AuthenticatedCalledOnceOnServer()
        {
            Assert.That(serverAuthCalled, Is.EqualTo(1));
        }

        [Test]
        public void AuthenticatedCalledOnceOnClient()
        {
            Assert.That(clientAuthCalled, Is.EqualTo(1));
        }
    }
}
