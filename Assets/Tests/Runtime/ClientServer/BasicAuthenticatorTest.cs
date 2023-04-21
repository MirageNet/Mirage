using Mirage.Authenticators;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class BasicAuthenticatorTest : ClientServerSetup<MockComponent>
    {
        private BasicAuthenticator serverAuthenticator;
        private BasicAuthenticator clientAuthenticator;

        protected override void ExtraServerSetup()
        {
            serverAuthenticator = serverGo.AddComponent<BasicAuthenticator>();
            server.authenticator = serverAuthenticator;

            serverAuthenticator.serverCode = "hello123";
        }
        protected override void ExtraClientSetup(IClientInstance instance)
        {
            clientAuthenticator = instance.GameObject.AddComponent<BasicAuthenticator>();
            instance.Client.authenticator = clientAuthenticator;

            clientAuthenticator.serverCode = "hello123";
        }

        [Test]
        public void ClientCanConnectWithAuthenticator()
        {
            // Should have connected
            Assert.That(clientPlayer, Is.Not.Null);
            Assert.That(ServerPlayer(0), Is.Not.Null);
        }
    }
}
