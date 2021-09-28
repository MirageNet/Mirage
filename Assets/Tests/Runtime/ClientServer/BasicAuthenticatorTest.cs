using Mirage.Authenticators;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class BasicAuthenticatorTest : ClientServerSetup<MockComponent>
    {

        BasicAuthenticator authenticator;

        [Test]
        public void CheckConnected()
        {
            // Should have connected
            Assert.That(clientPlayer, Is.Not.Null);
            Assert.That(serverPlayer, Is.Not.Null);
        }

        public override void ExtraSetup()
        {
            authenticator = serverGo.AddComponent<BasicAuthenticator>();

            server.Authenticator = authenticator;
            client.authenticator = authenticator;

            base.ExtraSetup();
        }

    }
}
