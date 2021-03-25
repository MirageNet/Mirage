using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;

namespace Mirage.Tests.ClientServer
{

    [TestFixture]
    public class ServerObjectManagerTest : ClientServerSetup<MockComponent>
    {
        GameObject playerReplacement;

        [Test]
        public void SpawnObjectExposeExceptionTest()
        {
            var gameObject = new GameObject();
            ServerObjectManager comp = gameObject.AddComponent<ServerObjectManager>();

            var obj = new GameObject();

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                comp.SpawnObject(obj, connectionToServer);
            });

            Assert.That(ex.Message, Is.EqualTo("SpawnObject for " + obj + ", NetworkServer is not active. Cannot spawn objects without an active server."));
        }

        [Test]
        public void SpawnNoIdentExceptionTest()
        {
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.Spawn(new GameObject(), new GameObject());
            });

            Assert.That(ex.Message, Is.EqualTo("Player object has no NetworkIdentity"));
        }

        [UnityTest]
        public IEnumerator SpawnByIdentityTest() => UniTask.ToCoroutine(async () =>
        {
            serverObjectManager.Spawn(serverIdentity);

            await AsyncUtil.WaitUntilWithTimeout(() => serverIdentity.Server == server);
        });

        [Test]
        public void SpawnNotPlayerExceptionTest()
        {
            var player = new GameObject();
            player.AddComponent<NetworkIdentity>();

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.Spawn(new GameObject(), player);
            });

            Assert.That(ex.Message, Is.EqualTo("Player object is not a player in the connection"));
        }

        [UnityTest]
        public IEnumerator ShowForConnection() => UniTask.ToCoroutine(async () =>
        {
            bool invoked = false;

            client.MessageHandler.RegisterHandler<SpawnMessage>(msg => invoked = true);

            connectionToClient.IsReady = true;

            // call ShowForConnection
            serverObjectManager.ShowForConnection(serverIdentity, connectionToClient);

            client.MessageHandler.ProcessMessagesAsync(connectionToClient).Forget();

            await AsyncUtil.WaitUntilWithTimeout(() => invoked);
        });

        [Test]
        public void SpawnSceneObject()
        {
            serverIdentity.sceneId = 42;
            serverIdentity.gameObject.SetActive(false);
            serverObjectManager.SpawnObjects();
            Assert.That(serverIdentity.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void SpawnPrefabObject()
        {
            serverIdentity.sceneId = 0;
            serverIdentity.gameObject.SetActive(false);
            serverObjectManager.SpawnObjects();
            Assert.That(serverIdentity.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void SpawnEvent()
        {

            UnityAction<NetworkIdentity> mockHandler = Substitute.For<UnityAction<NetworkIdentity>>();
            serverObjectManager.Spawned.AddListener(mockHandler);
            var newObj = GameObject.Instantiate(playerPrefab);
            serverObjectManager.Spawn(newObj);

            mockHandler.Received().Invoke(Arg.Any<NetworkIdentity>());
            serverObjectManager.Destroy(newObj);
        }

        [UnityTest]
        public IEnumerator ClientSpawnEvent() => UniTask.ToCoroutine(async () =>
        {
            UnityAction<NetworkIdentity> mockHandler = Substitute.For<UnityAction<NetworkIdentity>>();
            clientObjectManager.Spawned.AddListener(mockHandler);
            var newObj = GameObject.Instantiate(playerPrefab);
            serverObjectManager.Spawn(newObj);

            await UniTask.WaitUntil(() => mockHandler.ReceivedCalls().Any()).Timeout(TimeSpan.FromMilliseconds(200));

            mockHandler.Received().Invoke(Arg.Any<NetworkIdentity>());
            serverObjectManager.Destroy(newObj);
        });

        [UnityTest]
        public IEnumerator ClientUnSpawnEvent() => UniTask.ToCoroutine(async () =>
        {
            UnityAction<NetworkIdentity> mockHandler = Substitute.For<UnityAction<NetworkIdentity>>();
            clientObjectManager.UnSpawned.AddListener(mockHandler);
            var newObj = GameObject.Instantiate(playerPrefab);
            serverObjectManager.Spawn(newObj);
            serverObjectManager.Destroy(newObj);

            await UniTask.WaitUntil(() => mockHandler.ReceivedCalls().Any()).Timeout(TimeSpan.FromMilliseconds(200));
            mockHandler.Received().Invoke(Arg.Any<NetworkIdentity>());
        });

        [Test]
        public void UnSpawnEvent()
        {
            UnityAction<NetworkIdentity> mockHandler = Substitute.For<UnityAction<NetworkIdentity>>();
            serverObjectManager.UnSpawned.AddListener(mockHandler);
            var newObj = GameObject.Instantiate(playerPrefab);
            serverObjectManager.Spawn(newObj);
            serverObjectManager.Destroy(newObj);
            mockHandler.Received().Invoke(newObj.GetComponent<NetworkIdentity>());
        }

        [Test]
        public void ReplacePlayerBaseTest()
        {
            playerReplacement = new GameObject("replacement", typeof(NetworkIdentity));
            NetworkIdentity replacementIdentity = playerReplacement.GetComponent<NetworkIdentity>();
            replacementIdentity.AssetId = Guid.NewGuid();
            clientObjectManager.RegisterPrefab(replacementIdentity);

            serverObjectManager.ReplaceCharacter(connectionToClient, client, playerReplacement);

            Assert.That(connectionToClient.Identity, Is.EqualTo(replacementIdentity));
        }

        [Test]
        public void ReplacePlayerDontKeepAuthTest()
        {
            playerReplacement = new GameObject("replacement", typeof(NetworkIdentity));
            NetworkIdentity replacementIdentity = playerReplacement.GetComponent<NetworkIdentity>();
            replacementIdentity.AssetId = Guid.NewGuid();
            clientObjectManager.RegisterPrefab(replacementIdentity);

            serverObjectManager.ReplaceCharacter(connectionToClient, client, playerReplacement, true);

            Assert.That(clientIdentity.ConnectionToClient, Is.EqualTo(null));
        }

        [Test]
        public void ReplacePlayerAssetIdTest()
        {
            var replacementGuid = Guid.NewGuid();
            playerReplacement = new GameObject("replacement", typeof(NetworkIdentity));
            NetworkIdentity replacementIdentity = playerReplacement.GetComponent<NetworkIdentity>();
            replacementIdentity.AssetId = replacementGuid;
            clientObjectManager.RegisterPrefab(replacementIdentity);

            serverObjectManager.ReplaceCharacter(connectionToClient, client, playerReplacement, replacementGuid);

            Assert.That(connectionToClient.Identity.AssetId, Is.EqualTo(replacementGuid));
        }

        [Test]
        public void AddPlayerForConnectionAssetIdTest()
        {
            var replacementGuid = Guid.NewGuid();
            playerReplacement = new GameObject("replacement", typeof(NetworkIdentity));
            NetworkIdentity replacementIdentity = playerReplacement.GetComponent<NetworkIdentity>();
            replacementIdentity.AssetId = replacementGuid;
            clientObjectManager.RegisterPrefab(replacementIdentity);

            connectionToClient.Identity = null;

            Assert.That(serverObjectManager.AddCharacter(connectionToClient, playerReplacement, replacementGuid), Is.True);
        }

        [UnityTest]
        public IEnumerator RemovePlayerForConnectionTest() => UniTask.ToCoroutine(async () =>
        {
            serverObjectManager.RemovePlayerForConnection(connectionToClient);

            await AsyncUtil.WaitUntilWithTimeout(() => !clientIdentity);

            Assert.That(serverPlayerGO);
        });

        [UnityTest]
        public IEnumerator RemovePlayerForConnectionExceptionTest() => UniTask.ToCoroutine(async () =>
        {
            serverObjectManager.RemovePlayerForConnection(connectionToClient);

            await AsyncUtil.WaitUntilWithTimeout(() => !clientIdentity);

            Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.RemovePlayerForConnection(connectionToClient);
            });
        });

        [UnityTest]
        public IEnumerator RemovePlayerForConnectionDestroyTest() => UniTask.ToCoroutine(async () =>
        {
            serverObjectManager.RemovePlayerForConnection(connectionToClient, true);

            await AsyncUtil.WaitUntilWithTimeout(() => !clientIdentity);

            Assert.That(!serverPlayerGO);
        });

        [Test]
        public void SpawnObjectExceptionTest()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.SpawnObject(new GameObject(), connectionToClient);
            });
        }

        [Test]
        public void AddPlayerForConnectionFalseTest()
        {
            Assert.That(serverObjectManager.AddCharacter(connectionToClient, new GameObject()), Is.False);
        }

        [UnityTest]
        public IEnumerator SpawnObjectsFalseTest() => UniTask.ToCoroutine(async () =>
        {
            server.Disconnect();

            await AsyncUtil.WaitUntilWithTimeout(() => !server.Active);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.SpawnObjects();
            });

            Assert.That(exception, Has.Message.EqualTo("Server was not active"));
        });
    }
}

