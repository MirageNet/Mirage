using NUnit.Framework;

namespace Mirage.Tests.CustomGUI
{
    [TestFixture(Category = "NetworkPrefabs")]
    public class NetworkPrefabsInspectorTest : InspectorTestBase
    {
        [TearDown]
        public void TearDown()
        {
            TearDownTestObjects();
        }

        [Test]
        public void RegisterPrefabs()
        {
            var com = CreateScriptableObject<NetworkPrefabs>();

            var inspector = CreateEditor<NetworkPrefabsInspector>(com);

            inspector.RegisterPrefabs();

            Assert.That(com.Prefabs, Has.Count.GreaterThan(2));
        }

        [Test]
        public void PreserveExisting()
        {
            var preexisting = CreateNetworkIdentity();

            var com = CreateScriptableObject<NetworkPrefabs>();
            com.Prefabs.Add(preexisting);

            var inspector = CreateEditor<NetworkPrefabsInspector>(com);

            inspector.RegisterPrefabs();

            Assert.That(com.Prefabs, Contains.Item(preexisting));
        }
    }
}
