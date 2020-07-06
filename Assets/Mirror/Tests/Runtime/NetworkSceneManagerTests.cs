using System;
using System.Collections;
using NUnit.Framework;
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
    }

    public class NetworkSceneManagerNonHostTests : ClientServerSetup<MockComponent>
    {
        [Test]
        public void ChangeServerSceneExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                sceneManager.ChangeServerScene(string.Empty);
            });
        }
    }
}
