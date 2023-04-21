using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    public class HostComponentTests : HostSetupWithSceneManager<MockComponent>
    {
        [UnityTest]
        public IEnumerator ServerRpc()
        {
            base.hostComponent.Send2Args(1, "hello");

            yield return null;
            yield return null;

            Assert.That(hostComponent.cmdArg1, Is.EqualTo(1));
            Assert.That(hostComponent.cmdArg2, Is.EqualTo("hello"));
        }

        [UnityTest]
        public IEnumerator ServerRpcWithSender()
        {
            base.hostComponent.SendWithSender(1);

            yield return null;
            yield return null;

            Assert.That(hostComponent.cmdArg1, Is.EqualTo(1));
            Assert.That(hostComponent.cmdSender, Is.EqualTo(server.LocalPlayer), "Server Rpc call on host will have localplayer (server version) as sender");
        }

        [UnityTest]
        public IEnumerator ServerRpcWithNetworkIdentity()
        {
            hostComponent.CmdNetworkIdentity(hostIdentity);

            yield return null;
            yield return null;

            Assert.That(hostComponent.cmdNi, Is.SameAs(hostIdentity));
        }

        [UnityTest]
        public IEnumerator ClientRpc()
        {
            base.hostComponent.RpcTest(1, "hello");
            // process spawn message from server
            yield return null;
            yield return null;

            Assert.That(hostComponent.rpcArg1, Is.EqualTo(1));
            Assert.That(hostComponent.rpcArg2, Is.EqualTo("hello"));
        }

        [UnityTest]
        public IEnumerator ClientConnRpc()
        {
            hostComponent.ClientConnRpcTest(server.LocalPlayer, 1, "hello");
            // process spawn message from server
            yield return null;
            yield return null;

            Assert.That(hostComponent.targetRpcPlayer, Is.EqualTo(client.Player));
            Assert.That(hostComponent.targetRpcArg1, Is.EqualTo(1));
            Assert.That(hostComponent.targetRpcArg2, Is.EqualTo("hello"));
        }

        [UnityTest]
        public IEnumerator ClientOwnerRpc()
        {
            base.hostComponent.RpcOwnerTest(1, "hello");
            // process spawn message from server
            yield return null;
            yield return null;

            Assert.That(hostComponent.rpcOwnerArg1, Is.EqualTo(1));
            Assert.That(hostComponent.rpcOwnerArg2, Is.EqualTo("hello"));
        }

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

        [UnityTest, Description("Scene will already be loaded on host connect, so no events should be invoked")]
        public IEnumerator HostPlayerShouldReceiveClientStartFinishSceneEvents() => UniTask.ToCoroutine(async () =>
        {
            server.Stop();

            // wait for server to disconnect
            await UniTask.WaitUntil(() => !server.Active);

            var mockStart = Substitute.For<UnityAction<string, SceneOperation>>();
            var mockFinish = Substitute.For<UnityAction<Scene, SceneOperation>>();
            sceneManager.OnClientStartedSceneChange.AddListener(mockStart);
            sceneManager.OnClientFinishedSceneChange.AddListener(mockFinish);

            // start host again
            server.StartServer(client);

            client.Update();
            var activeScene = SceneManager.GetActiveScene();
            mockStart.Received(1).Invoke(activeScene.path, SceneOperation.Normal);
            mockFinish.Received(1).Invoke(SceneManager.GetActiveScene(), SceneOperation.Normal);
        });
    }
}
