using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirror.Tests
{
    public class NetworkSceneManagerTests : HostSetup<MockComponent>
    {
        [Test]
        public void ChangeServerSceneExceptionTest()
        {
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            {
                sceneManager.ChangeServerScene(string.Empty);
            });
        }

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

        [Test]
        public void FinishLoadServerOnlyTest()
        {
            client.Disconnect();

            sceneManager.ServerSceneChanged.AddListener(TestOnServerSceneChangedInvoke);

            sceneManager.FinishLoadScene();

            Assert.That(onOnServerSceneChangedCounter, Is.EqualTo(1));
        }
    }
}
