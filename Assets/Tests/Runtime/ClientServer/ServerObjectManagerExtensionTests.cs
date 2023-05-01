using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class ServerObjectManagerExtensionTests : ClientServerSetup
    {
        private const int hash = 10;


        private NetworkIdentity prefabNI;
        private GameObject prefabGO => prefabNI.gameObject;

        protected override async UniTask LateSetup()
        {
            await base.LateSetup();

            prefabNI = CreateNetworkIdentity(disable: true);

            // add dumby handler, just something that wont give error
            clientObjectManager.RegisterSpawnHandler(hash, new SpawnHandlerDelegate((msg) => CreateNetworkIdentity()), null);
        }


        [Test]
        public void SpawnInstantiate_NI(
            [Values(true, false)] bool setOwner,
            [Values(true, false)] bool setHash)
        {
            var prefab = prefabNI;
            var owner = setOwner ? serverPlayer : null;
            int? newHash;
            if (setHash)
            {
                newHash = hash;
            }
            else
            {
                prefabNI.PrefabHash = hash;
                newHash = null;
            }

            var clone = serverObjectManager.SpawnInstantiate(prefab, newHash, owner);

            Assert.That(clone, Is.Not.EqualTo(prefab));
            Assert.That(clone.name, Is.EqualTo($"{prefab.name}(Clone)"));
            Assert.That(clone.NetId, Is.Not.Zero);
            Assert.That(clone.Owner, Is.EqualTo(owner));
            Assert.That(clone.PrefabHash, Is.EqualTo(hash));
        }

        [Test]
        public void SpawnInstantiate_GO(
            [Values(true, false)] bool setOwner,
            [Values(true, false)] bool setHash)
        {
            var prefab = prefabGO;
            var owner = setOwner ? serverPlayer : null;
            int? newHash;
            if (setHash)
            {
                newHash = hash;
            }
            else
            {
                prefabNI.PrefabHash = hash;
                newHash = null;
            }

            var clone = serverObjectManager.SpawnInstantiate(prefab, newHash, owner);

            Assert.That(clone, Is.Not.EqualTo(prefab));
            Assert.That(clone.name, Is.EqualTo($"{prefab.name}(Clone)"));
            var cloneNI = clone.GetNetworkIdentity();
            Assert.That(cloneNI.NetId, Is.Not.Zero);
            Assert.That(cloneNI.Owner, Is.EqualTo(owner));
            Assert.That(cloneNI.PrefabHash, Is.EqualTo(hash));
        }
    }
}

