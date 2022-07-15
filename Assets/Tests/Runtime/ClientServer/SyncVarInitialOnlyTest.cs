using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class SyncVarInitialOnly : NetworkBehaviour
    {
        [SyncVar(initialOnly = true)]
        public int weaponIndex = 1;

        [SyncVar]
        public int health = 100;

        [SyncVar(initialOnly = true)]
        public float otherValue = 13f;
    }

    public class SyncVarInitialOnlyTest : ClientServerSetup<SyncVarInitialOnly>
    {
        [Test]
        public void ChangingInitOnlyVarWontSetBehaviourDirty()
        {
            serverComponent.weaponIndex = 10;
            Assert.That(serverComponent.IsDirty(), Is.False);

            serverComponent.otherValue = 5.2f;
            Assert.That(serverComponent.IsDirty(), Is.False);

            serverComponent.health = 20;
            Assert.That(serverComponent.IsDirty(), Is.True);
        }

        [UnityTest]
        public IEnumerator InitOnlyIsntSentToClient()
        {
            serverComponent.weaponIndex = 10;
            serverComponent.health = 50;
            serverComponent.otherValue = 5.2f;

            yield return new WaitForSeconds(0.2f);

            Assert.That(clientComponent.weaponIndex, Is.EqualTo(1), "Should not have changed");
            Assert.That(clientComponent.health, Is.EqualTo(50));
            Assert.That(clientComponent.otherValue, Is.EqualTo(13f));
        }

        [UnityTest]
        public IEnumerator BothSyncVarsAreSetIntially()
        {
            var prefab = new GameObject("BothSyncVarsAreSet", typeof(NetworkIdentity), typeof(SyncVarInitialOnly));
            var identity = prefab.GetComponent<NetworkIdentity>();
            identity.PrefabHash = Guid.NewGuid().GetHashCode();

            clientObjectManager.RegisterPrefab(identity);

            var clone = GameObject.Instantiate(prefab);
            var behaviour = clone.GetComponent<SyncVarInitialOnly>();
            behaviour.weaponIndex = 3;
            behaviour.health = 20;
            behaviour.otherValue = 5.2f;

            serverObjectManager.Spawn(clone);
            var netId = behaviour.NetId;

            yield return null;
            yield return null;

            client.World.TryGetIdentity(netId, out var clientClient);
            var clientBehaviour = clientClient.GetComponent<SyncVarInitialOnly>();

            Assert.That(clientBehaviour.weaponIndex, Is.EqualTo(3));
            Assert.That(clientBehaviour.health, Is.EqualTo(20));
            Assert.That(clientBehaviour.otherValue, Is.EqualTo(5.2f));

            behaviour.weaponIndex = 2;
            behaviour.health = 30;
            behaviour.otherValue = 7.3f;

            yield return new WaitForSeconds(0.2f);

            Assert.That(clientBehaviour.weaponIndex, Is.EqualTo(3), "does not change");
            Assert.That(clientBehaviour.health, Is.EqualTo(30));
            Assert.That(clientBehaviour.otherValue, Is.EqualTo(5.2f));
        }
    }
}
