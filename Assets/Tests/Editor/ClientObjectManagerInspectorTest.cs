using NUnit.Framework;
using UnityEditor;

namespace Mirage.Tests
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

        //[Test]
        //public void CreateNetworkPrefabs()
        //{
        //    var com = CreateMonoBehaviour<ClientObjectManager>();

        //    var inspector = CreateEditor<ClientObjectManagerInspector>(com);
        //    inspector.CreateNetworkPrefabs(NETWORK_PREFABS_PATH);

        //    Assert.That(com.NetworkPrefabs, Is.Not.Null);
        //}

        //[Test]
        //public void CreateNetworkPrefabsWithNullPath()
        //{
        //    var com = CreateMonoBehaviour<ClientObjectManager>();

        //    var inspector = CreateEditor<ClientObjectManagerInspector>(com);
        //    inspector.CreateNetworkPrefabs(null);

        //    Assert.That(com.NetworkPrefabs, Is.Null);
        //}

        //[Test]
        //public void CreateNetworkPrefabsWithEmptyPath()
        //{
        //    var com = CreateMonoBehaviour<ClientObjectManager>();

        //    var inspector = CreateEditor<ClientObjectManagerInspector>(com);
        //    inspector.CreateNetworkPrefabs("");

        //    Assert.That(com.NetworkPrefabs, Is.Null);
        //}

        //[Test]
        //public void CreateNetworkPrefabsWithWhitespacePath()
        //{
        //    var com = CreateMonoBehaviour<ClientObjectManager>();

        //    var inspector = CreateEditor<ClientObjectManagerInspector>(com);
        //    inspector.CreateNetworkPrefabs(" ");

        //    Assert.That(com.NetworkPrefabs, Is.Null);
        //}

        //        [Test]
        //        public void CreateNetworkPrefabsKeepsOldPrefabs()
        //        {
        //            var existing = CreateNetworkIdentity();
        //            var com = CreateMonoBehaviour<ClientObjectManager>();

        //            var prefab = PrefabUtility.SaveAsPrefabAsset(existing.gameObject, NETWORKED_PREFAB_PATH);
        //            // We disable the warning as we're using it for backwards compatibility reasons.
        //#pragma warning disable CS0618 // Type or member is obsolete
        //            com.spawnPrefabs.Add(prefab.GetComponent<NetworkIdentity>());
        //#pragma warning restore CS0618

        //            var inspector = CreateEditor<ClientObjectManagerInspector>(com);
        //            inspector.CreateNetworkPrefabs(NETWORK_PREFABS_PATH);

        //            Assert.That(com.NetworkPrefabs, Is.Not.Null);
        //            Assert.That(com.NetworkPrefabs.Prefabs, Contains.Item(prefab.GetComponent<NetworkIdentity>()));
        //        }
    }
}
