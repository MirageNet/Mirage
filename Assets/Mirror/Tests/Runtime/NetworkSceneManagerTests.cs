using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirror.Tests
{
    public class NetworkSceneManagerTests : HostSetup<MockComponent>
    {
        int onAuthInvokeCounter;
        void TestOnAuthenticatedInvoke(INetworkConnection conn)
        {
            onAuthInvokeCounter++;
        }

        int onOnServerSceneChangedCounter;
        void TestOnServerSceneChangedInvoke(string scene)
        {
            onOnServerSceneChangedCounter++;
        }

        int onOnClientSceneChangedCounter;
        void TestOnClientSceneChangedInvoke(INetworkConnection conn)
        {
            onOnClientSceneChangedCounter++;
        }

        [Test]
        public void FinishLoadSceneHostTest()
        {
            client.Authenticated.AddListener(TestOnAuthenticatedInvoke);
            sceneManager.ServerSceneChanged.AddListener(TestOnServerSceneChangedInvoke);
            sceneManager.ClientSceneChanged.AddListener(TestOnClientSceneChangedInvoke);

            sceneManager.FinishLoadScene();

            Assert.That(onAuthInvokeCounter, Is.EqualTo(1));
            Assert.That(onOnServerSceneChangedCounter, Is.EqualTo(1));
            Assert.That(onOnClientSceneChangedCounter, Is.EqualTo(1));
        }

        int onOnServerSceneOnlyChangedCounter;
        void TestOnServerOnlySceneChangedInvoke(string scene)
        {
            onOnServerSceneOnlyChangedCounter++;
        }

        [UnityTest]
        public IEnumerator FinishLoadServerOnlyTest()
        {
            client.Disconnect();
            yield return null;

            sceneManager.ServerSceneChanged.AddListener(TestOnServerOnlySceneChangedInvoke);

            sceneManager.FinishLoadScene();

            Assert.That(onOnServerSceneOnlyChangedCounter, Is.EqualTo(1));
        }

        int OnServerChangeSceneCounter;
        void TestOnServerChangeSceneInvoke(string scene)
        {
            OnServerChangeSceneCounter++;
        }

        int ClientSceneMessageCounter;
        void ClientSceneMessage(INetworkConnection conn, SceneMessage msg)
        {
            ClientSceneMessageCounter++;
        }

        int NotReadyMessageCounter;
        void NotReadyMessage(INetworkConnection conn, NotReadyMessage msg)
        {
            NotReadyMessageCounter++;
        }

        [UnityTest]
        public IEnumerator ServerChangeSceneTest()
        {
            client.Connection.RegisterHandler<SceneMessage>(ClientSceneMessage);
            client.Connection.RegisterHandler<NotReadyMessage>(NotReadyMessage);
            sceneManager.ServerChangeScene.AddListener(TestOnServerChangeSceneInvoke);

            AssetBundle.LoadFromFile("Assets/Mirror/Tests/Runtime/TestScene/testscene");
            server.sceneManager.ChangeServerScene("testScene");

            Assert.That(server.sceneManager.networkSceneName, Is.EqualTo("testScene"));
            Assert.That(OnServerChangeSceneCounter, Is.EqualTo(1));

            yield return null;

            Assert.That(ClientSceneMessageCounter, Is.EqualTo(1));
            Assert.That(NotReadyMessageCounter, Is.EqualTo(1));
        }

        [Test]
        public void ChangeServerSceneExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                server.sceneManager.ChangeServerScene(string.Empty);
            });
        }
    }

    public class NetworkSceneManagerNonHostTests : ClientServerSetup<MockComponent>
    {
        [Test]
        public void ClientSceneMessageExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                sceneManager.ClientSceneMessage(null, new SceneMessage());
            });
        }
    }
}
