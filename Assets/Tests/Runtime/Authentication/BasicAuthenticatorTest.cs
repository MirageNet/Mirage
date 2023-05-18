using Mirage.Authenticators;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.ClientServer.Authenticators
{
    public class BasicAuthenticatorTest : AuthenticatorTestSetup<BasicAuthenticator>
    {
        private const string SERVER_CODE = "hello123";

        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();
            _serverAuthenticator.ServerCode = SERVER_CODE;
        }
        protected override void ExtraClientSetup(IClientInstance instance)
        {
            base.ExtraClientSetup(instance);
            _clientAuthenticator.ServerCode = SERVER_CODE;
        }

        [Test]
        public void ClientCanConnectWithAuthenticator()
        {
            // Should have connected
            Assert.That(clientPlayer, Is.Not.Null);
            Assert.That(ServerPlayer(0), Is.Not.Null);

            Assert.That(clientPlayer.IsAuthenticated, Is.True);
            Assert.That(serverPlayer.Authentication, Is.Not.Null);
            Assert.That(serverPlayer.Authentication.Authenticator, Is.TypeOf<BasicAuthenticator>());

            Assert.That(serverPlayer.IsAuthenticated, Is.True);
            Assert.That(serverPlayer.Authentication, Is.Not.Null);
            Assert.That(serverPlayer.Authentication.Authenticator, Is.TypeOf<BasicAuthenticator>());
            Assert.That(serverPlayer.Authentication.Data, Is.Null, "Basic auth has no data");
        }
    }
}
namespace Mirage.Tests.Runtime.ClientServer.Authenticators
{
}
