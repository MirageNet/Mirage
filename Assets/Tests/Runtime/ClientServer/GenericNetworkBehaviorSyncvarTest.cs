using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class GenericBehaviourWithSyncVar<T> : NetworkBehaviour
    {
        [SyncVar]
        public int baseValue = 0;
        [SyncVar(hook = nameof(OnSyncedBaseValueWithHook))]
        public int baseValueWithHook = 0;
        [SyncVar]
        public NetworkBehaviour target;
        [SyncVar]
        public NetworkIdentity targetIdentity;

        public Action<int, int> onBaseValueChanged;

        // Not used, just here to trigger possible errors.
        private T value;
        private T[] values;

        public void OnSyncedBaseValueWithHook(int oldValue, int newValue)
        {
            onBaseValueChanged?.Invoke(oldValue, newValue);
        }
    }

    public class GenericBehaviourWithSyncVarImplement : GenericBehaviourWithSyncVar<UnityEngine.Vector3>
    {
        [SyncVar]
        public int childValue = 0;
        [SyncVar(hook = nameof(OnSyncedChildValueWithHook))]
        public int childValueWithHook = 0;
        [SyncVar]
        public NetworkBehaviour childTarget;
        [SyncVar]
        public NetworkIdentity childIdentity;

        public Action<int, int> onChildValueChanged;

        public void OnSyncedChildValueWithHook(int oldValue, int newValue)
        {
            onChildValueChanged?.Invoke(oldValue, newValue);
        }
    }

    public class GenericNetworkBehaviorSyncvarTest : ClientServerSetup<GenericBehaviourWithSyncVarImplement>
    {
        [Test]
        public void IsZeroByDefault()
        {
            Assert.AreEqual(clientComponent.baseValue, 0);
            Assert.AreEqual(clientComponent.baseValueWithHook, 0);
            Assert.AreEqual(clientComponent.childValue, 0);
            Assert.AreEqual(clientComponent.childValueWithHook, 0);
            Assert.IsNull(clientComponent.target);
            Assert.IsNull(clientComponent.targetIdentity);
            Assert.IsNull(clientComponent.childTarget);
            Assert.IsNull(clientComponent.childIdentity);
        }

        [UnityTest]
        public IEnumerator ChangeValue() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.baseValue = 2;

            await UniTask.WaitUntil(() => clientComponent.baseValue != 0);

            Assert.AreEqual(clientComponent.baseValue, 2);
        });

        [UnityTest]
        public IEnumerator ChangeValueHook() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.baseValueWithHook = 2;
            clientComponent.onBaseValueChanged += (oldValue, newValue) =>
            {
                Assert.AreEqual(0, oldValue);
                Assert.AreEqual(2, newValue);
            };

            await UniTask.WaitUntil(() => clientComponent.baseValueWithHook != 0);
        });

        [UnityTest]
        public IEnumerator ChangeTarget() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.target = serverComponent;

            await UniTask.WaitUntil(() => clientComponent.target != null);

            Assert.That(clientComponent.target, Is.SameAs(clientComponent));
        });

        [UnityTest]
        public IEnumerator ChangeNetworkIdentity() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.targetIdentity = serverIdentity;

            await UniTask.WaitUntil(() => clientComponent.targetIdentity != null);

            Assert.That(clientComponent.targetIdentity, Is.SameAs(clientIdentity));
        });

        [UnityTest]
        public IEnumerator ChangeChildValue() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.childValue = 2;

            await UniTask.WaitUntil(() => clientComponent.childValue != 0);

            Assert.AreEqual(clientComponent.childValue, 2);
        });

        [UnityTest]
        public IEnumerator ChangeChildValueHook() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.childValueWithHook = 2;
            clientComponent.onChildValueChanged += (oldValue, newValue) =>
            {
                Assert.AreEqual(0, oldValue);
                Assert.AreEqual(2, newValue);
            };

            await UniTask.WaitUntil(() => clientComponent.childValueWithHook != 0);
        });

        [UnityTest]
        public IEnumerator ChangeChildTarget() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.childTarget = serverComponent;

            await UniTask.WaitUntil(() => clientComponent.childTarget != null);

            Assert.That(clientComponent.childTarget, Is.SameAs(clientComponent));
        });

        [UnityTest]
        public IEnumerator ChangeChildNetworkIdentity() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.childIdentity = serverIdentity;

            await UniTask.WaitUntil(() => clientComponent.childIdentity != null);

            Assert.That(clientComponent.childIdentity, Is.SameAs(clientIdentity));
        });

        [UnityTest]
        public IEnumerator SpawnWithValue() => UniTask.ToCoroutine(async () =>
        {
            // create an object, set the target and spawn it
            UnityEngine.GameObject newObject = UnityEngine.Object.Instantiate(playerPrefab);
            GenericBehaviourWithSyncVarImplement newBehavior = newObject.GetComponent<GenericBehaviourWithSyncVarImplement>();
            newBehavior.baseValue = 2;
            newBehavior.childValue = 22;
            newBehavior.target = serverComponent;
            newBehavior.targetIdentity = serverIdentity;
            newBehavior.childTarget = serverComponent;
            newBehavior.childIdentity = serverIdentity;
            serverObjectManager.Spawn(newObject);

            // wait until the client spawns it
            uint newObjectId = newBehavior.NetId;
            await UniTask.WaitUntil(() => clientObjectManager.SpawnedObjects.ContainsKey(newObjectId));
            // check if the target was set correctly in the client
            NetworkIdentity newClientObject = clientObjectManager.SpawnedObjects[newObjectId];
            GenericBehaviourWithSyncVarImplement newClientBehavior = newClientObject.GetComponent<GenericBehaviourWithSyncVarImplement>();
            Assert.AreEqual(newClientBehavior.baseValue, 2);
            Assert.AreEqual(newClientBehavior.childValue, 22);
            Assert.That(newClientBehavior.target, Is.SameAs(clientComponent));
            Assert.That(newClientBehavior.targetIdentity, Is.SameAs(clientIdentity));
            Assert.That(newClientBehavior.childTarget, Is.SameAs(clientComponent));
            Assert.That(newClientBehavior.childIdentity, Is.SameAs(clientIdentity));

            // cleanup
            serverObjectManager.Destroy(newObject);
        });
    }
}
