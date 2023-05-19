using System.Collections;
using Mirage.Authenticators;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Authentication
{
    public class BasicAuthenticatorTest : AuthenticatorTestSetup<BasicAuthenticator>
    {
        private const string SERVER_CODE = "hello123";

        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();
            _serverAuthenticator.ServerCode = SERVER_CODE;
        }

        [UnityTest]
        public IEnumerator CanAuthenticateUsingField()
        {
            // Should have connected
            Assert.That(clientPlayer, Is.Not.Null);
            Assert.That(ServerPlayer(0), Is.Not.Null);
            Assert.That(clientPlayer.IsAuthenticated, Is.False);
            Assert.That(serverPlayer.IsAuthenticated, Is.False);


            _clientAuthenticator.ServerCode = SERVER_CODE;
            _clientAuthenticator.SendCode(client);
            yield return null;
            yield return null;

            // Should have authenticated
            Assert.That(clientPlayer.IsAuthenticated, Is.True);
            Assert.That(clientPlayer.Authentication, Is.Not.Null);
            Assert.That(clientPlayer.Authentication.Authenticator, Is.TypeOf<BasicAuthenticator>());

            Assert.That(serverPlayer.IsAuthenticated, Is.True);
            Assert.That(serverPlayer.Authentication, Is.Not.Null);
            Assert.That(serverPlayer.Authentication.Authenticator, Is.TypeOf<BasicAuthenticator>());
            Assert.That(serverPlayer.Authentication.Data, Is.Null, "Basic auth has no data");
        }

        [UnityTest]
        public IEnumerator CanAuthenticateUsingArgment()
        {
            // Should have connected
            Assert.That(clientPlayer, Is.Not.Null);
            Assert.That(ServerPlayer(0), Is.Not.Null);
            Assert.That(clientPlayer.IsAuthenticated, Is.False);
            Assert.That(serverPlayer.IsAuthenticated, Is.False);


            _clientAuthenticator.SendCode(client, SERVER_CODE);
            yield return null;
            yield return null;

            // Should have authenticated
            Assert.That(clientPlayer.IsAuthenticated, Is.True);
            Assert.That(clientPlayer.Authentication, Is.Not.Null);
            Assert.That(clientPlayer.Authentication.Authenticator, Is.TypeOf<BasicAuthenticator>());

            Assert.That(serverPlayer.IsAuthenticated, Is.True);
            Assert.That(serverPlayer.Authentication, Is.Not.Null);
            Assert.That(serverPlayer.Authentication.Authenticator, Is.TypeOf<BasicAuthenticator>());
            Assert.That(serverPlayer.Authentication.Data, Is.Null, "Basic auth has no data");
        }

        [UnityTest]
        public IEnumerator FailsWhenCodeDoesntMatch()
        {
            // Should have connected
            Assert.That(clientPlayer, Is.Not.Null);
            Assert.That(ServerPlayer(0), Is.Not.Null);
            Assert.That(clientPlayer.IsAuthenticated, Is.False);
            Assert.That(serverPlayer.IsAuthenticated, Is.False);

            _clientAuthenticator.SendCode(client, SERVER_CODE + "bad");
            yield return null;
            yield return null;

            // should have disconnected
            Assert.That(clientPlayer.IsAuthenticated, Is.False);
            Assert.That(serverPlayer.IsAuthenticated, Is.False);

            Assert.That(server.Players.Count, Is.Zero);
            Assert.That(client.IsConnected, Is.False);
        }
    }
}
