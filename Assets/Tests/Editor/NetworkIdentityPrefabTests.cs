using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests
{
    public class NetworkIdentityPrefabTests : TestBase
    {
        private NetworkIdentity identity;
        private NetworkServer server;
        private ServerObjectManager serverObjectManager;
        private GameObject networkServerGameObject;

        [SetUp]
        public void SetUp()
        {
            networkServerGameObject = CreateGameObject();
            server = networkServerGameObject.AddComponent<NetworkServer>();
            serverObjectManager = networkServerGameObject.AddComponent<ServerObjectManager>();
            server.ObjectManager = serverObjectManager;
            networkServerGameObject.AddComponent<NetworkClient>();

            // avoid CreateNetworkIdentity because it sets hash
            identity = CreateGameObject().AddComponent<NetworkIdentity>();
            identity.Server = server;
            identity.ServerObjectManager = serverObjectManager;
        }

        [TearDown]
        public void TearDown()
        {
            TearDownTestObjects();
        }

        [Test]
        public void AssignSceneID()
        {
            // OnValidate will have assigned a random sceneId of format 0x00000000FFFFFFFF
            // -> make sure that one was assigned, and that the left part was
            //    left empty for scene hash
            Assert.That(identity.SceneId, Is.Not.Zero);
            Assert.That(identity.SceneId & 0xFFFF_FFFF_0000_0000ul, Is.Zero);

            // make sure that OnValidate added it to sceneIds dict
            Assert.That(NetworkIdentityIdGenerator._sceneIds[(int)(identity.SceneId & 0x0000_0000_FFFF_FFFFul)], Is.Not.Null);
        }

        [Test]
        public void SetSceneIdSceneHashPartInternal()
        {
            // Awake will have assigned a random sceneId of format 0x00000000FFFFFFFF
            // -> make sure that one was assigned, and that the left part was
            //    left empty for scene hash
            Assert.That(identity.SceneId, Is.Not.Zero);
            Assert.That(identity.SceneId & 0xFFFF_FFFF_0000_0000ul, Is.Zero, "scene hash should start empty");
            var originalId = identity.SceneId;

            // set scene hash
            NetworkIdentityIdGenerator.SetSceneHash(identity);

            var newSceneId = identity.SceneId;
            var newID = newSceneId & 0x0000_0000_FFFF_FFFFul;
            var newHash = newSceneId & 0xFFFF_FFFF_0000_0000ul;

            // make sure that the right part is still the random sceneid
            Assert.That(newID, Is.EqualTo(originalId));

            // make sure that the left part is a scene hash now
            Assert.That(newHash, Is.Not.Zero);

            // calling it again should said the exact same hash again
            NetworkIdentityIdGenerator.SetSceneHash(identity);
            Assert.That(identity.SceneId, Is.EqualTo(newSceneId), "should be same value as first time it was called");
        }

        [Test]
        public void OnValidateSetupIDsSetsEmptyPrefabHashForSceneObject()
        {
            // OnValidate will have been called. make sure that PrefabHash was set
            // to 0 empty and not anything else, because this is a scene object
            Assert.That(identity.PrefabHash, Is.EqualTo(0));
        }
    }
}
