using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Authentication
{
    public class NetworkAuthenticatorTest : AuthenticatorTestSetup<MockAuthenticator>
    {
        [UnityTest]
        public IEnumerator ServerAuthenticateReturnsResult() => UniTask.ToCoroutine(async () =>
        {
            var player = Substitute.For<INetworkPlayer>();
            // start auth task for player
            var authTask = _serverSettings.ServerAuthenticate(player);

            var result = await authTask;

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.TypeOf<MockAuthenticator.MockData>());
            Assert.That(result.Authenticator, Is.EqualTo(_serverAuthenticator));
        });


        [UnityTest]
        public IEnumerator ServerAuthenticateTimesout() => UniTask.ToCoroutine(async () =>
        {
            const int timeout = 2;

            var player = Substitute.For<INetworkPlayer>();
            // start auth task for player
            _serverSettings.TimeoutSeconds = timeout;
            var authTask = _serverSettings.ServerAuthenticate(player);

            // more than timeout
            await UniTask.Delay((timeout * 1000) + 100);

            // should be complete
            Assert.That(authTask.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            // get result
            var result = await authTask;

            Assert.That(result.Success, Is.False);
            Assert.That(result.Data, Is.Null);
            Assert.That(result.Authenticator, Is.Null);
            Assert.That(result.Reason, Is.EqualTo("Timeout"));
        });


        [UnityTest]
        public IEnumerator ShouldWaitForSendAuth()
        {
            Assert.That(_serverAuthCalls, Is.Empty);
            Assert.That(_clientAuthCalls, Is.Empty);

            _clientAuthenticator.SendAuthentication(client, new MockAuthenticator.MockMessage());

            yield return null;
            yield return null;

            Assert.That(_serverAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_serverAuthCalls[0], Is.EqualTo(serverPlayer));

            Assert.That(serverPlayer.IsAuthenticated, Is.True);
            Assert.That(serverPlayer.Authentication, Is.Not.Null);
            Assert.That(serverPlayer.Authentication.Authenticator, Is.TypeOf<MockAuthenticator>());
            Assert.That(serverPlayer.Authentication.Data, Is.TypeOf<MockAuthenticator.MockData>());

            // client needs extra frame to receive message from server
            yield return null;

            Assert.That(_clientAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_clientAuthCalls[0], Is.EqualTo(clientPlayer));

            Assert.That(clientPlayer.IsAuthenticated, Is.True);
            Assert.That(clientPlayer.Authentication, Is.Not.Null);
            Assert.That(clientPlayer.Authentication.Authenticator, Is.TypeOf<MockAuthenticator>());
        }
    }
}
