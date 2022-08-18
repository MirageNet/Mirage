using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests
{
    [TestFixture(Category = "ClientObjectManager")]
    public class ClientObjectManagerInspectorTest : TestBase
    {
        [TearDown]
        public void TearDown()
        {
            TearDownTestObjects();
        }

        [Test]
        public void RegisterPrefabs()
        {
            var com = CreateMonoBehaviour<ClientObjectManager>();

            var inspector = ScriptableObject.CreateInstance<ClientObjectManagerInspector>();
            inspector.RegisterPrefabs(com);

            Assert.That(com.spawnPrefabs, Has.Count.GreaterThan(2));
        }

        [Test]
        public void PreserveExisting()
        {
            var preexisting = CreateNetworkIdentity();

            var com = CreateMonoBehaviour<ClientObjectManager>();
            com.spawnPrefabs.Add(preexisting);

            var inspector = ScriptableObject.CreateInstance<ClientObjectManagerInspector>();
            inspector.RegisterPrefabs(com);

            Assert.That(com.spawnPrefabs, Contains.Item(preexisting));
        }
    }
}
