using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.Events;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    public class HostComponentTests : HostSetup<MockComponent>
    {
        [UnityTest]
        public IEnumerator ServerRpc() => UniTask.ToCoroutine(async () =>
        {
            component.Send2Args(1, "hello");

            await AsyncUtil.WaitUntilWithTimeout(() => component.cmdArg1 != 0);

            Assert.That(component.cmdArg1, Is.EqualTo(1));
            Assert.That(component.cmdArg2, Is.EqualTo("hello"));
        });

        [UnityTest]
        public IEnumerator ServerRpcWithSender() => UniTask.ToCoroutine(async () =>
        {
            component.SendWithSender(1);

            await AsyncUtil.WaitUntilWithTimeout(() => component.cmdArg1 != 0);

            Assert.That(component.cmdArg1, Is.EqualTo(1));
            Assert.That(component.cmdSender, Is.EqualTo(server.LocalPlayer), "Server Rpc call on host will have localplayer (server version) as sender");
        });

        [UnityTest]
        public IEnumerator ServerRpcWithNetworkIdentity() => UniTask.ToCoroutine(async () =>
        {
            component.CmdNetworkIdentity(identity);

            await AsyncUtil.WaitUntilWithTimeout(() => component.cmdNi != null);

            Assert.That(component.cmdNi, Is.SameAs(identity));
        });

        [UnityTest]
        public IEnumerator ClientRpc() => UniTask.ToCoroutine(async () =>
        {
            component.RpcTest(1, "hello");
            // process spawn message from server
            await AsyncUtil.WaitUntilWithTimeout(() => component.rpcArg1 != 0);

            Assert.That(component.rpcArg1, Is.EqualTo(1));
            Assert.That(component.rpcArg2, Is.EqualTo("hello"));
        });

        [UnityTest]
        public IEnumerator ClientConnRpc() => UniTask.ToCoroutine(async () =>
        {
            component.ClientConnRpcTest(manager.Server.LocalPlayer, 1, "hello");
            // process spawn message from server
            await AsyncUtil.WaitUntilWithTimeout(() => component.targetRpcArg1 != 0);

            Assert.That(component.targetRpcPlayer, Is.EqualTo(manager.Client.Player));
            Assert.That(component.targetRpcArg1, Is.EqualTo(1));
            Assert.That(component.targetRpcArg2, Is.EqualTo("hello"));
        });

        [UnityTest]
        public IEnumerator ClientOwnerRpc() => UniTask.ToCoroutine(async () =>
        {
            component.RpcOwnerTest(1, "hello");
            // process spawn message from server
            await AsyncUtil.WaitUntilWithTimeout(() => component.rpcOwnerArg1 != 0);

            Assert.That(component.rpcOwnerArg1, Is.EqualTo(1));
            Assert.That(component.rpcOwnerArg2, Is.EqualTo("hello"));
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
