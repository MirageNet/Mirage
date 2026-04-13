using System.Collections;
using Mirage.Tests.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class ClientObjectManagerParentingTest : ClientServerSetup
    {
        [UnityTest]
        public IEnumerator AutoParentingWorks()
        {
            var parent = InstantiateForTest(_characterPrefab);
            serverObjectManager.Spawn(parent);

            var child = InstantiateForTest(_characterPrefab);
            child.SpawnSettings = new NetworkSpawnSettings 
            { 
                SendPosition = true,
                SendRotation = true,
                SendParent = SpawnParentingMode.Auto 
            };
            child.transform.SetParent(parent.transform);
            child.transform.localPosition = new Vector3(1, 2, 3);
            serverObjectManager.Spawn(child);

            // Wait for spawn messages to be processed
            yield return null;
            yield return null;

            var clientParent = _remoteClients[0].Get(parent);
            var clientChild = _remoteClients[0].Get(child);

            Assert.That(clientChild.transform.parent, Is.EqualTo(clientParent.transform));
            Assert.That(clientChild.transform.localPosition, Is.EqualTo(new Vector3(1, 2, 3)));
        }

        [UnityTest]
        public IEnumerator ManualParentingWorks()
        {
            var parent = InstantiateForTest(_characterPrefab);
            serverObjectManager.Spawn(parent);

            var child = InstantiateForTest(_characterPrefab);
            child.SpawnSettings = new NetworkSpawnSettings 
            { 
                SendPosition = true,
                SendParent = SpawnParentingMode.Manual 
            };
            child.Parent = parent;
            child.transform.localPosition = new Vector3(4, 5, 6);
            serverObjectManager.Spawn(child);

            yield return null;
            yield return null;

            var clientParent = _remoteClients[0].Get(parent);
            var clientChild = _remoteClients[0].Get(child);

            Assert.That(clientChild.transform.parent, Is.EqualTo(clientParent.transform));
            Assert.That(clientChild.transform.localPosition, Is.EqualTo(new Vector3(4, 5, 6)));
        }

        [UnityTest]
        public IEnumerator SpawnWithParentIdentityOverload()
        {
            var parent = InstantiateForTest(_characterPrefab);
            serverObjectManager.Spawn(parent);

            var child = InstantiateForTest(_characterPrefab);
            child.SpawnSettings = new NetworkSpawnSettings { SendParent = SpawnParentingMode.Manual };
            serverObjectManager.Spawn(child, parent);

            yield return null;
            yield return null;

            var clientParent = _remoteClients[0].Get(parent);
            var clientChild = _remoteClients[0].Get(child);

            Assert.That(child.transform.parent, Is.EqualTo(parent.transform), "Should be parented on server");
            Assert.That(clientChild.transform.parent, Is.EqualTo(clientParent.transform), "Should be parented on client");
        }

        [UnityTest]
        public IEnumerator AutoParentingFindsHighestIdentity()
        {
            var grandParent = InstantiateForTest(_characterPrefab);
            serverObjectManager.Spawn(grandParent);

            var parentWithoutIdentity = new GameObject("ParentNoNI").transform;
            parentWithoutIdentity.SetParent(grandParent.transform);
            
            var child = InstantiateForTest(_characterPrefab);
            child.SpawnSettings = new NetworkSpawnSettings { SendParent = SpawnParentingMode.Auto };
            child.transform.SetParent(parentWithoutIdentity);
            serverObjectManager.Spawn(child);

            yield return null;
            yield return null;

            var clientGrandParent = _remoteClients[0].Get(grandParent);
            var clientChild = _remoteClients[0].Get(child);

            Assert.That(clientChild.transform.parent, Is.EqualTo(clientGrandParent.transform));
            
            Object.DestroyImmediate(parentWithoutIdentity.gameObject);
        }
    }
}
