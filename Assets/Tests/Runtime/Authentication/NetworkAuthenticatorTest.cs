using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
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

        [Test]
        public void ShouldWaitForSendAuth()
        {

            Assert.That()


            _clientAuthenticator.SendAuthentication(client, new MockAuthenticator.MockMesasge());

            clientMockMethod.Received().Invoke(Arg.Any<INetworkPlayer>());
        }

        [Test]
        public void ClientOnValidateTest()
        {
            Assert.That(client.authenticator, Is.EqualTo(_clientSettings));
        }

        [Test]
        public void ServerOnValidateTest()
        {
            Assert.That(server.authenticator, Is.EqualTo(_serverSettings));
        }

        [Test]
        public void NetworkClientCallsAuthenticator()
        {
            clientMockMethod.Received().Invoke(Arg.Any<INetworkPlayer>());
        }

        [Test]
        public void NetworkServerCallsAuthenticator()
        {
            clientMockMethod.Received().Invoke(Arg.Any<INetworkPlayer>());
        }
    }
}
