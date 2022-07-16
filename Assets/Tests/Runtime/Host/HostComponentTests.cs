using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.Events;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    public class HostComponentTests : HostSetupWithSceneManager<MockComponent>
    {
        [UnityTest]
        public IEnumerator ServerRpc() => UniTask.ToCoroutine(async () =>
        {
            playerComponent.Send2Args(1, "hello");

            await AsyncUtil.WaitUntilWithTimeout(() => playerComponent.cmdArg1 != 0);

            Assert.That(playerComponent.cmdArg1, Is.EqualTo(1));
            Assert.That(playerComponent.cmdArg2, Is.EqualTo("hello"));
        });

        [UnityTest]
        public IEnumerator ServerRpcWithSender() => UniTask.ToCoroutine(async () =>
        {
            playerComponent.SendWithSender(1);

            await AsyncUtil.WaitUntilWithTimeout(() => playerComponent.cmdArg1 != 0);

            Assert.That(playerComponent.cmdArg1, Is.EqualTo(1));
            Assert.That(playerComponent.cmdSender, Is.EqualTo(server.LocalPlayer), "Server Rpc call on host will have localplayer (server version) as sender");
        });

        [UnityTest]
        public IEnumerator ServerRpcWithNetworkIdentity() => UniTask.ToCoroutine(async () =>
        {
            playerComponent.CmdNetworkIdentity(playerIdentity);

            await AsyncUtil.WaitUntilWithTimeout(() => playerComponent.cmdNi != null);

            Assert.That(playerComponent.cmdNi, Is.SameAs(playerIdentity));
        });

        [UnityTest]
        public IEnumerator ClientRpc() => UniTask.ToCoroutine(async () =>
        {
            playerComponent.RpcTest(1, "hello");
            // process spawn message from server
            await AsyncUtil.WaitUntilWithTimeout(() => playerComponent.rpcArg1 != 0);

            Assert.That(playerComponent.rpcArg1, Is.EqualTo(1));
            Assert.That(playerComponent.rpcArg2, Is.EqualTo("hello"));
        });

        [UnityTest]
        public IEnumerator ClientConnRpc() => UniTask.ToCoroutine(async () =>
        {
            playerComponent.ClientConnRpcTest(manager.Server.LocalPlayer, 1, "hello");
            // process spawn message from server
            await AsyncUtil.WaitUntilWithTimeout(() => playerComponent.targetRpcArg1 != 0);

            Assert.That(playerComponent.targetRpcPlayer, Is.EqualTo(manager.Client.Player));
            Assert.That(playerComponent.targetRpcArg1, Is.EqualTo(1));
            Assert.That(playerComponent.targetRpcArg2, Is.EqualTo("hello"));
        });

        [UnityTest]
        public IEnumerator ClientOwnerRpc() => UniTask.ToCoroutine(async () =>
        {
            playerComponent.RpcOwnerTest(1, "hello");
            // process spawn message from server
            await AsyncUtil.WaitUntilWithTimeout(() => playerComponent.rpcOwnerArg1 != 0);

            Assert.That(playerComponent.rpcOwnerArg1, Is.EqualTo(1));
            Assert.That(playerComponent.rpcOwnerArg2, Is.EqualTo("hello"));
        });

        [Test]
        public void StopHostTest()
        {
            server.Stop();

            // state cleared?
            Assert.That(server.Players, Is.Empty);
            Assert.That(server.Active, Is.False);
            Assert.That(server.LocalPlayer, Is.Null);
            Assert.That(server.LocalClientActive, Is.False);
        }

        [Test]
        public void StoppingHostShouldCallDisconnectedOnLocalClient()
        {
            var invoked = 0;
            client.Disconnected.AddListener((reason) =>
            {
                Assert.That(reason, Is.EqualTo(ClientStoppedReason.HostModeStopped));
                invoked++;
            });

            server.Stop();

            // state cleared?
            Assert.That(invoked, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ClientSceneChangedOnReconnect() => UniTask.ToCoroutine(async () =>
        {
            server.Stop();

            // wait for server to disconnect
            await UniTask.WaitUntil(() => !server.Active);

            var mockListener = Substitute.For<UnityAction<string, SceneOperation>>();
            sceneManager.OnClientStartedSceneChange.AddListener(mockListener);
            await StartHost();

            client.Update();
            mockListener.Received().Invoke(Arg.Any<string>(), Arg.Any<SceneOperation>());
        });
    }
}
