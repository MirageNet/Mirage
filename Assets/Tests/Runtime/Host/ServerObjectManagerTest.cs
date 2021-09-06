using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Mirage.Tests.LocalConnections;
using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime.Host
{

    [TestFixture]
    public class ServerObjectManagerHostTest : HostSetup<MockComponent>
    {
        [Test]
        public void SetClientReadyAndNotReadyTest()
        {
            (_, NetworkPlayer connection) = PipedConnections(ClientMessageHandler, ServerMessageHandler);
            Assert.That(connection.SceneIsReady, Is.False);

            serverObjectManager.SpawnVisibleObjects(connection);
            Assert.That(connection.SceneIsReady, Is.True);

            serverObjectManager.SetClientNotReady(connection);
            Assert.That(connection.SceneIsReady, Is.False);
        }

        [Test]
        public void SetAllClientsNotReadyTest()
        {
            // add first ready client
            (_, NetworkPlayer first) = PipedConnections(ClientMessageHandler, ServerMessageHandler);
            first.SceneIsReady = true;
            server.Players.Add(first);

            // add second ready client
            (_, NetworkPlayer second) = PipedConnections(ClientMessageHandler, ServerMessageHandler);
            second.SceneIsReady = true;
            server.Players.Add(second);

            // set all not ready
            serverObjectManager.SetAllClientsNotReady();
            Assert.That(first.SceneIsReady, Is.False);
            Assert.That(second.SceneIsReady, Is.False);
        }



        [Test]
        public void HideForConnection()
        {
            // add connection

            INetworkPlayer player = Substitute.For<INetworkPlayer>();

            NetworkIdentity identity = new GameObject().AddComponent<NetworkIdentity>();

            serverObjectManager.HideForConnection(identity, player);

            player.Received().Send(Arg.Is<ObjectHideMessage>(msg => msg.netId == identity.NetId));

            // destroy GO after shutdown, otherwise isServer is true in OnDestroy and it tries to call
            // GameObject.Destroy (but we need DestroyImmediate in Editor)
            Object.Destroy(identity.gameObject);
        }

        [Test]
        public void ValidateSceneObject()
        {
            identity.sceneId = 42;
            Assert.That(serverObjectManager.ValidateSceneObject(identity), Is.True);
            identity.sceneId = 0;
            Assert.That(serverObjectManager.ValidateSceneObject(identity), Is.False);
        }

        [Test]
        public void HideFlagsTest()
        {
            // shouldn't be valid for certain hide flags
            playerGO.hideFlags = HideFlags.NotEditable;
            Assert.That(serverObjectManager.ValidateSceneObject(identity), Is.False);
            playerGO.hideFlags = HideFlags.HideAndDontSave;
            Assert.That(serverObjectManager.ValidateSceneObject(identity), Is.False);
        }

        [Test]
        public void UnSpawn()
        {
            // unspawn
            serverObjectManager.Destroy(playerGO, false);

            // it should have been marked for reset now
            Assert.That(identity.NetId, Is.Zero);
        }

        [UnityTest]
        public IEnumerator DestroyAllSpawnedOnStopTest() => UniTask.ToCoroutine(async () =>
        {
            var spawnTestObj = new GameObject("testObj", typeof(NetworkIdentity));
            serverObjectManager.Spawn(spawnTestObj);

            // need to grab reference to world before Stop, becuase stop will clear reference
            NetworkWorld world = server.World;

            //1 is the player. should be 2 at this point
            Assert.That(world.SpawnedIdentities.Count, Is.GreaterThan(1));


            server.Stop();

            await AsyncUtil.WaitUntilWithTimeout(() => !server.Active);

            Assert.That(world.SpawnedIdentities.Count, Is.Zero);
            // checks that the object was destroyed
            // use unity null check here
            Assert.IsTrue(spawnTestObj == null);
        });
    }
}
