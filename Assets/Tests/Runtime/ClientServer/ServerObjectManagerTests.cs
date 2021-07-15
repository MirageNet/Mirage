using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{

    [TestFixture]
    public class ServerObjectManagerTests : ClientServerSetup<MockComponent>
    {
        GameObject playerReplacement;

        void AssertNoIdentityMessage(InvalidOperationException ex, string name) => Assert.That(ex.Message, Is.EqualTo($"Gameobject {name} doesn't have NetworkIdentity."));
        void AssertNoIdentityMessage(InvalidOperationException ex) => AssertNoIdentityMessage(ex, new GameObject().name);

        [Test]
        public void ThrowsIfSpawnCalledWhenServerIsNotAcctive()
        {
            var obj = new GameObject();

            server.Stop();

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.Spawn(new GameObject().AddComponent<NetworkIdentity>(), serverPlayer);
            });

            Assert.That(ex.Message, Is.EqualTo("NetworkServer is not active. Cannot spawn objects without an active server."));
            GameObject.DestroyImmediate(obj);
        }

        [Test]
        public void ThrowsIfSpawnCalledOwnerHasNoNetworkIdentity()
        {
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.Spawn(new GameObject(), new GameObject());
            });

            AssertNoIdentityMessage(ex);
        }

        [UnityTest]
        public IEnumerator SpawnByIdentityTest() => UniTask.ToCoroutine(async () =>
        {
            serverObjectManager.Spawn(serverIdentity);

            await AsyncUtil.WaitUntilWithTimeout(() => (NetworkServer)serverIdentity.Server == server);
        });

        [Test]
        public void ThrowsIfSpawnCalledWithOwnerWithNoOwnerTest()
        {
            var badOwner = new GameObject();
            badOwner.AddComponent<NetworkIdentity>();

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.Spawn(new GameObject(), badOwner);
            });

            Assert.That(ex.Message, Is.EqualTo("Player object is not a player in the connection"));
            GameObject.DestroyImmediate(badOwner);
        }

        [UnityTest]
        public IEnumerator ShowForConnection() => UniTask.ToCoroutine(async () =>
        {
            bool invoked = false;

            ClientMessageHandler.RegisterHandler<SpawnMessage>(msg => invoked = true);

            serverPlayer.IsReady = true;

            // call ShowForConnection
            serverObjectManager.ShowForConnection(serverIdentity, serverPlayer);

            // todo assert correct message was sent using Substitute for socket or player

            //connectionToServer.ProcessMessagesAsync().Forget();

            await AsyncUtil.WaitUntilWithTimeout(() => invoked);
        });

        [Test]
        public void SpawnSceneObject()
        {
            serverIdentity.sceneId = 42;
            serverIdentity.gameObject.SetActive(false);
            serverObjectManager.SpawnObjects();
            Assert.That(serverIdentity.gameObject.activeSelf, Is.False);
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
            Action<NetworkIdentity> mockHandler = Substitute.For<Action<NetworkIdentity>>();
            server.World.onSpawn += mockHandler;
            var newObj = GameObject.Instantiate(playerPrefab);
            serverObjectManager.Spawn(newObj);

            mockHandler.Received().Invoke(Arg.Any<NetworkIdentity>());
            serverObjectManager.Destroy(newObj);
        }

        [UnityTest]
        public IEnumerator ClientSpawnEvent() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkIdentity> mockHandler = Substitute.For<Action<NetworkIdentity>>();
            client.World.onSpawn += mockHandler;
            var newObj = GameObject.Instantiate(playerPrefab);
            serverObjectManager.Spawn(newObj);

            await UniTask.WaitUntil(() => mockHandler.ReceivedCalls().Any()).Timeout(TimeSpan.FromMilliseconds(200));

            mockHandler.Received().Invoke(Arg.Any<NetworkIdentity>());
            serverObjectManager.Destroy(newObj);
        });

        [UnityTest]
        public IEnumerator ClientUnSpawnEvent() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkIdentity> mockHandler = Substitute.For<Action<NetworkIdentity>>();
            client.World.onUnspawn += mockHandler;
            var newObj = GameObject.Instantiate(playerPrefab);
            serverObjectManager.Spawn(newObj);
            serverObjectManager.Destroy(newObj);

            await UniTask.WaitUntil(() => mockHandler.ReceivedCalls().Any()).Timeout(TimeSpan.FromMilliseconds(200));
            mockHandler.Received().Invoke(Arg.Any<NetworkIdentity>());
        });

        [Test]
        public void UnSpawnEvent()
        {
            Action<NetworkIdentity> mockHandler = Substitute.For<Action<NetworkIdentity>>();
            server.World.onUnspawn += mockHandler;
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

            serverObjectManager.ReplaceCharacter(serverPlayer, playerReplacement);

            Assert.That(serverPlayer.Identity, Is.EqualTo(replacementIdentity));
        }

        [Test]
        public void ReplacePlayerDontKeepAuthTest()
        {
            playerReplacement = new GameObject("replacement", typeof(NetworkIdentity));
            NetworkIdentity replacementIdentity = playerReplacement.GetComponent<NetworkIdentity>();
            replacementIdentity.AssetId = Guid.NewGuid();
            clientObjectManager.RegisterPrefab(replacementIdentity);

            serverObjectManager.ReplaceCharacter(serverPlayer, playerReplacement, true);

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

            serverObjectManager.ReplaceCharacter(serverPlayer, playerReplacement, replacementGuid);

            Assert.That(serverPlayer.Identity.AssetId, Is.EqualTo(replacementGuid));
        }

        [Test]
        public void AddPlayerForConnectionAssetIdTest()
        {
            var replacementGuid = Guid.NewGuid();
            playerReplacement = new GameObject("replacement", typeof(NetworkIdentity));
            NetworkIdentity replacementIdentity = playerReplacement.GetComponent<NetworkIdentity>();
            replacementIdentity.AssetId = replacementGuid;
            clientObjectManager.RegisterPrefab(replacementIdentity);

            serverPlayer.Identity = null;

            serverObjectManager.AddCharacter(serverPlayer, playerReplacement, replacementGuid);

            Assert.That(replacementIdentity == serverPlayer.Identity);
        }

        [UnityTest]
        public IEnumerator RemovePlayerForConnectionTest() => UniTask.ToCoroutine(async () =>
        {
            serverObjectManager.RemovePlayerForConnection(serverPlayer);

            await AsyncUtil.WaitUntilWithTimeout(() => !clientIdentity);

            Assert.That(serverPlayerGO);
        });

        [UnityTest]
        public IEnumerator RemovePlayerForConnectionExceptionTest() => UniTask.ToCoroutine(async () =>
        {
            serverObjectManager.RemovePlayerForConnection(serverPlayer);

            await AsyncUtil.WaitUntilWithTimeout(() => !clientIdentity);

            Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.RemovePlayerForConnection(serverPlayer);
            });
        });

        [UnityTest]
        public IEnumerator RemovePlayerForConnectionDestroyTest() => UniTask.ToCoroutine(async () =>
        {
            serverObjectManager.RemovePlayerForConnection(serverPlayer, true);

            await AsyncUtil.WaitUntilWithTimeout(() => !clientIdentity);

            Assert.That(!serverPlayerGO);
        });

        [Test]
        public void ThrowsIfSpawnedCalledWithoutANetworkIdentity()
        {
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.Spawn(new GameObject(), clientPlayer);
            });

            AssertNoIdentityMessage(ex);
        }


        [Test]
        public void AddCharacterNoIdentityExceptionTest()
        {
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.AddCharacter(serverPlayer, new GameObject());
            });
            AssertNoIdentityMessage(ex);

        }

        [Test]
        public void ReplacePlayerNoIdentityExceptionTest()
        {
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.ReplaceCharacter(serverPlayer, new GameObject(), true);
            });
            AssertNoIdentityMessage(ex);
        }

        [UnityTest]
        public IEnumerator SpawnObjectsExceptionTest() => UniTask.ToCoroutine(async () =>
        {
            server.Stop();

            await AsyncUtil.WaitUntilWithTimeout(() => !server.Active);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                serverObjectManager.SpawnObjects();
            });

            Assert.That(exception, Has.Message.EqualTo("Server was not active"));
        });
    }
}

