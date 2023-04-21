using Mirage.Authenticators;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class BasicAuthenticatorTest : ClientServerSetup<MockComponent>
    {
        private BasicAuthenticator authenticator;

        [Test]
        public void CheckConnected()
        {
            // Should have connected
            Assert.That(clientPlayer, Is.Not.Null);
            Assert.That(ServerPlayer(0), Is.Not.Null);
        }

        public override void ExtraSetup()
        {
            authenticator = server.gameObject.AddComponent<BasicAuthenticator>();

            server.authenticator = authenticator;
            client.authenticator = authenticator;

            base.ExtraSetup();
        }

    }
}
