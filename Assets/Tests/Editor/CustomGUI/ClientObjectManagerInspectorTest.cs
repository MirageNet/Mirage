using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;

namespace Mirage.Tests.CustomGUI
{
    [TestFixture(Category = "ClientObjectManager")]
    public class ClientObjectManagerInspectorTest : InspectorTestBase
    {
        private const string NETWORK_PREFABS_PATH = "Assets/TEST_NETWORK_PREFABS.asset";
        private const string NETWORKED_PREFAB_PATH = "Assets/TEST_NETWORK_PREFABS.prefab";

        [TearDown]
        public void TearDown()
        {
            TearDownTestObjects();

            if (AssetDatabase.LoadAssetAtPath<NetworkPrefabs>(NETWORK_PREFABS_PATH))
            {
                AssetDatabase.DeleteAsset(NETWORK_PREFABS_PATH);
            }

            if (AssetDatabase.LoadAssetAtPath<NetworkIdentity>(NETWORKED_PREFAB_PATH))
            {
                AssetDatabase.DeleteAsset(NETWORKED_PREFAB_PATH);
            }
        }

        [Test]
        public void LoadAll()
        {
            var found = ClientObjectManagerInspector.LoadAllNetworkIdentities();

            Assert.That(found, Has.Count.GreaterThan(2));
        }

        [Test]
        public void PreserveExisting()
        {
            var preexisting = CreateNetworkIdentity();

            var existing = new List<NetworkIdentity>();
            existing.Add(preexisting);

            var found = ClientObjectManagerInspector.LoadAllNetworkIdentities();
            ClientObjectManagerInspector.AddToPrefabList(existing, found);

            Assert.That(existing, Contains.Item(preexisting));
        }

        [Test]
        public void RegisterAllWithField()
        {
            var com = CreateMonoBehaviour<ClientObjectManager>();
            var inspector = CreateEditor<ClientObjectManagerInspector>(com);

            Assert.That(com.spawnPrefabs, Has.Count.EqualTo(0));
            Assert.That(com.NetworkPrefabs, Is.Null);

            inspector.RegisterAllPrefabs();

            // finds prefabs and adds them to field
            Assert.That(com.spawnPrefabs, Has.Count.GreaterThan(2));
            Assert.That(com.NetworkPrefabs, Is.Null);
        }

        [Test]
        public void RegisterAllWithSO()
        {
            var com = CreateMonoBehaviour<ClientObjectManager>();
            var inspector = CreateEditor<ClientObjectManagerInspector>(com);
            var so = CreateScriptableObject<NetworkPrefabs>();
            com.NetworkPrefabs = so;

            Assert.That(com.spawnPrefabs, Has.Count.EqualTo(0));
            Assert.That(com.NetworkPrefabs, Is.Not.Null);
            Assert.That(com.NetworkPrefabs.Prefabs, Has.Count.EqualTo(0));

            inspector.RegisterAllPrefabs();

            // finds prefabs and adds them to SO
            Assert.That(com.spawnPrefabs, Has.Count.EqualTo(0), "field should stay zero when using SO");
            Assert.That(com.NetworkPrefabs, Is.Not.Null);
            Assert.That(com.NetworkPrefabs.Prefabs, Has.Count.GreaterThan(2));
        }

        [Test]
        public void MovePrefabs()
        {
            var com = CreateMonoBehaviour<ClientObjectManager>();
            var inspector = CreateEditor<ClientObjectManagerInspector>(com);
            var so = CreateScriptableObject<NetworkPrefabs>();
            com.NetworkPrefabs = so;

            var id1 = CreateNetworkIdentity();
            var id2 = CreateNetworkIdentity();
            var id3 = CreateNetworkIdentity();

            com.spawnPrefabs.Add(id1);
            com.spawnPrefabs.Add(id2);

            so.Prefabs.Add(id3);

            Assert.That(com.spawnPrefabs, Has.Count.EqualTo(2));
            Assert.That(com.NetworkPrefabs, Is.Not.Null);
            Assert.That(com.NetworkPrefabs.Prefabs, Has.Count.EqualTo(1));

            inspector.MovePrefabsToSO();

            // finds prefabs and adds them to SO
            Assert.That(com.spawnPrefabs, Has.Count.EqualTo(0), "field should stay zero when using SO");
            Assert.That(com.NetworkPrefabs, Is.Not.Null);
            Assert.That(com.NetworkPrefabs.Prefabs, Has.Count.EqualTo(3));
            Assert.That(com.NetworkPrefabs.Prefabs, Contains.Item(id1));
            Assert.That(com.NetworkPrefabs.Prefabs, Contains.Item(id2));
            Assert.That(com.NetworkPrefabs.Prefabs, Contains.Item(id3));
        }
    }
}
