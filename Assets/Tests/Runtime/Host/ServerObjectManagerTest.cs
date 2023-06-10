using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    [TestFixture]
    public class ServerObjectManagerHostTest : HostSetup
    {
        [Test]
        public void HideForPlayerTest()
        {
            // add connection

            var player = Substitute.For<INetworkPlayer>();

            var identity = CreateNetworkIdentity();
            // objectManager must be set in order for Visibility too be called
            identity.ServerObjectManager = serverObjectManager;

            serverObjectManager.HideToPlayer(identity, player);

            player.Received().Send(Arg.Is<ObjectHideMessage>(msg => msg.NetId == identity.NetId));
        }

        [Test]
        public void ValidateSceneObject()
        {
            hostIdentity.SetSceneId(42);
            Assert.That(serverObjectManager.ValidateSceneObject(hostIdentity), Is.True);
            hostIdentity.SetSceneId(0);
            Assert.That(serverObjectManager.ValidateSceneObject(hostIdentity), Is.False);
        }

        [Test]
        public void HideFlagsTest()
        {
            // shouldn't be valid for certain hide flags
            hostPlayerGO.hideFlags = HideFlags.NotEditable;
            Assert.That(serverObjectManager.ValidateSceneObject(hostIdentity), Is.False);
            hostPlayerGO.hideFlags = HideFlags.HideAndDontSave;
            Assert.That(serverObjectManager.ValidateSceneObject(hostIdentity), Is.False);
        }

        [Test]
        public void UnSpawn()
        {
            // unspawn
            serverObjectManager.Destroy(hostPlayerGO, false);

            // it should have been marked for reset now
            Assert.That(hostIdentity.NetId, Is.Zero);
        }

        [UnityTest]
        public IEnumerator DestroyAllSpawnedOnStopTest() => UniTask.ToCoroutine(async () =>
        {
            var spawnTestObj = CreateNetworkIdentity();
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
