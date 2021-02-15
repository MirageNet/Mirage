using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirror.Tests.ClientServer
{
    public class GenericBehaviourWithSyncVar<T> : NetworkBehaviour
    {
        [SyncVar]
        public int baseValue = 0;
        [SyncVar(hook = nameof(OnSyncedBaseValueWithHook))]
        public int baseValueWithHook = 0;
        //[SyncVar]
        //public NetworkBehaviour target;


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
            clientComponent.onBaseValueChanged += (oldValue, newValue) =>
            {
                Assert.AreEqual(0, oldValue);
                Assert.AreEqual(2, newValue);
            };

            await UniTask.WaitUntil(() => clientComponent.childValueWithHook != 0);
        });

        [UnityTest]
        public IEnumerator SpawnWithValue() => UniTask.ToCoroutine(async () =>
        {
            // create an object, set the target and spawn it
            UnityEngine.GameObject newObject = UnityEngine.Object.Instantiate(playerPrefab);
            GenericBehaviourWithSyncVarImplement newBehavior = newObject.GetComponent<GenericBehaviourWithSyncVarImplement>();
            newBehavior.baseValue = 2;
            serverObjectManager.Spawn(newObject);

            // wait until the client spawns it
            uint newObjectId = newBehavior.NetId;
            await UniTask.WaitUntil(() => client.Spawned.ContainsKey(newObjectId));

            // check if the target was set correctly in the client
            NetworkIdentity newClientObject = client.Spawned[newObjectId];
            GenericBehaviourWithSyncVarImplement newClientBehavior = newClientObject.GetComponent<GenericBehaviourWithSyncVarImplement>();
            Assert.AreEqual(newClientBehavior.baseValue, 2);

            // cleanup
            serverObjectManager.Destroy(newObject);
        });
    }
}
