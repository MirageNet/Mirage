using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    [TestFixture]
    public class ServerObjectManagerHostTest : HostSetup<MockComponent>
    {
        [Test]
        public void HideForPlayerTest()
        {
            // add connection

            var player = Substitute.For<INetworkPlayer>();

            NetworkIdentity identity = CreateNetworkIdentity();

            serverObjectManager.HideToPlayer(identity, player);

            player.Received().Send(Arg.Is<ObjectHideMessage>(msg => msg.netId == identity.NetId));
        }

        [Test]
        public void ValidateSceneObject()
        {
            playerIdentity.SetSceneId(42);
            Assert.That(serverObjectManager.ValidateSceneObject(playerIdentity), Is.True);
            playerIdentity.SetSceneId(0);
            Assert.That(serverObjectManager.ValidateSceneObject(playerIdentity), Is.False);
        }

        [Test]
        public void HideFlagsTest()
        {
            // shouldn't be valid for certain hide flags
            playerGO.hideFlags = HideFlags.NotEditable;
            Assert.That(serverObjectManager.ValidateSceneObject(playerIdentity), Is.False);
            playerGO.hideFlags = HideFlags.HideAndDontSave;
            Assert.That(serverObjectManager.ValidateSceneObject(playerIdentity), Is.False);
        }

        [Test]
        public void UnSpawn()
        {
            // unspawn
            serverObjectManager.Destroy(playerGO, false);

            // it should have been marked for reset now
            Assert.That(playerIdentity.NetId, Is.Zero);
        }

        [UnityTest]
        public IEnumerator DestroyAllSpawnedOnStopTest() => UniTask.ToCoroutine(async () =>
        {
            NetworkIdentity spawnTestObj = CreateNetworkIdentity();
            serverObjectManager.Spawn(spawnTestObj);

            // need to grab reference to world before Stop, becuase stop will clear reference
            var world = server.World;

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
