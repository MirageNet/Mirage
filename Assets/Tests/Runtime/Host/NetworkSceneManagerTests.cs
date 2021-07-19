using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    public class NetworkSceneManagerTests : HostSetup<MockComponent>
    {
        AssetBundle bundle;

        UnityAction<string, SceneOperation> sceneEventFunction;

        public override void ExtraSetup()
        {
            bundle = AssetBundle.LoadFromFile("Assets/Tests/Runtime/TestScene/testscene");

            sceneEventFunction = Substitute.For<UnityAction<string, SceneOperation>>();
            sceneManager.ServerFinishedSceneChange.AddListener(sceneEventFunction);
        }

        public override void ExtraTearDown()
        {
            bundle.Unload(true);
        }

        [Test]
        public void FinishLoadSceneHostTest()
        {
            UnityAction<string, SceneOperation> func2 = Substitute.For<UnityAction<string, SceneOperation>>();
            UnityAction<string, SceneOperation> func3 = Substitute.For<UnityAction<string, SceneOperation>>();

            sceneManager.ServerFinishedSceneChange.AddListener(func2);
            sceneManager.ClientFinishedSceneChange.AddListener(func3);

            sceneManager.CompleteLoadingScene("test", null, SceneOperation.Normal);

            func2.Received(1).Invoke(Arg.Any<string>(), Arg.Any<SceneOperation>());
            func3.Received(1).Invoke(Arg.Any<string>(), Arg.Any<SceneOperation>());
        }

        [UnityTest]
        public IEnumerator FinishLoadServerOnlyTest() => UniTask.ToCoroutine(async () =>
        {
            UnityAction<string, SceneOperation> func1 = Substitute.For<UnityAction<string, SceneOperation>>();

            client.Disconnect();

            await AsyncUtil.WaitUntilWithTimeout(() => !client.Active);

            sceneManager.ServerFinishedSceneChange.AddListener(func1);

            sceneManager.CompleteLoadingScene("test", null, SceneOperation.Normal);

            func1.Received(1).Invoke(Arg.Any<string>(), Arg.Any<SceneOperation>());
        });

        [UnityTest]
        public IEnumerator ServerChangeSceneTest() => UniTask.ToCoroutine(async () =>
        {
            bool invokeClientSceneMessage = false;
            bool invokeNotReadyMessage = false;
            UnityAction<string, SceneOperation> func1 = Substitute.For<UnityAction<string, SceneOperation>>();
            ClientMessageHandler.RegisterHandler<SceneMessage>(msg => invokeClientSceneMessage = true);
            ClientMessageHandler.RegisterHandler<NotReadyMessage>(msg => invokeNotReadyMessage = true);
            sceneManager.ServerStartedSceneChange.AddListener(func1);

            sceneManager.ChangeServerScene("Assets/Mirror/Tests/Runtime/testScene.unity");

            await AsyncUtil.WaitUntilWithTimeout(() => sceneManager.ActiveScenePath.Equals("Assets/Mirror/Tests/Runtime/testScene.unity"));

            func1.Received(1).Invoke(Arg.Any<string>(), Arg.Any<SceneOperation>());
            Assert.That(sceneManager.ActiveScenePath, Is.EqualTo("Assets/Mirror/Tests/Runtime/testScene.unity"));
            Assert.That(invokeClientSceneMessage, Is.True);
            Assert.That(invokeNotReadyMessage, Is.True);
        });

        [Test]
        public void ServerChangedFiredOnceTest()
        {
            sceneEventFunction.Received(1).Invoke(Arg.Any<string>(), Arg.Any<SceneOperation>());
        }

        [Test]
        public void ChangeServerSceneExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                sceneManager.ChangeServerScene(string.Empty);
            });
        }

        [Test]
        public void ReadyTest()
        {
            sceneManager.SetSceneIsReady();
            Assert.That(client.Player.SceneIsReady);
        }

        [UnityTest]
        public IEnumerator ReadyExceptionTest() => UniTask.ToCoroutine(async () =>
        {
            sceneManager.Client.Disconnect();

            await AsyncUtil.WaitUntilWithTimeout(() => !sceneManager.Client.Active);

            Assert.Throws<InvalidOperationException>(() =>
            {
                sceneManager.SetSceneIsReady();
            });
        });

        [Test]
        public void ClientChangeSceneTest()
        {
            UnityAction<string, SceneOperation> func1 = Substitute.For<UnityAction<string, SceneOperation>>();
            sceneManager.ClientStartedSceneChange.AddListener(func1);

            sceneManager.ClientStartedSceneChange.Invoke("", SceneOperation.Normal);

            func1.Received(1).Invoke(Arg.Any<string>(), Arg.Any<SceneOperation>());
        }

        [Test]
        public void ClientSceneChangedTest()
        {
            UnityAction<string, SceneOperation> func1 = Substitute.For<UnityAction<string, SceneOperation>>();
            sceneManager.ClientFinishedSceneChange.AddListener(func1);
            sceneManager.ClientFinishedSceneChange.Invoke("test", SceneOperation.Normal);
            func1.Received(1).Invoke(Arg.Any<string>(), Arg.Any<SceneOperation>());
        }

        [Test]
        public void ClientSceneReadyAfterChangedTest()
        {
            bool _readyAfterSceneChanged = false;
            sceneManager.ClientFinishedSceneChange.AddListener((string name, SceneOperation operation) => _readyAfterSceneChanged = client.Player.SceneIsReady);
            sceneManager.ClientFinishedSceneChange.Invoke("test", SceneOperation.Normal);

            Assert.That(_readyAfterSceneChanged, Is.True);
        }

        [UnityTest]
        public IEnumerator ChangeSceneAdditiveLoadTest() => UniTask.ToCoroutine(async () =>
        {
            sceneManager.ChangeServerScene("Assets/Mirror/Tests/Runtime/testScene.unity", SceneOperation.LoadAdditive);

            await AsyncUtil.WaitUntilWithTimeout(() => SceneManager.GetSceneByName("testScene") != null);

            Assert.That(SceneManager.GetSceneByName("testScene"), Is.Not.Null);
        });

        [Test]
        public void ClientChangeSceneNotNullTest()
        {
            Assert.That(sceneManager.ClientStartedSceneChange, Is.Not.Null);
        }

        [Test]
        public void ClientSceneChangedNotNullTest()
        {
            Assert.That(sceneManager.ClientFinishedSceneChange, Is.Not.Null);
        }

        [Test]
        public void ServerChangeSceneNotNullTest()
        {
            Assert.That(sceneManager.ServerStartedSceneChange, Is.Not.Null);
        }

        [Test]
        public void ServerSceneChangedNotNullTest()
        {
            Assert.That(sceneManager.ServerFinishedSceneChange, Is.Not.Null);
        }
    }
}
