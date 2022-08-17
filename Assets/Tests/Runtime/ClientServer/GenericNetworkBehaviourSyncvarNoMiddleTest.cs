using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class GenericBehaviourWithSyncVarNoMiddleBase<T> : NetworkBehaviour
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

    public class GenericBehaviourWithSyncVarNoMiddleMiddle<T> : GenericBehaviourWithSyncVarNoMiddleBase<T>
    {
        // Not used, just here to trigger possible errors.
        private T middleValue;
        private T[] middleValues;
    }

    public class GenericBehaviourWithSyncVarNoMiddleImplement : GenericBehaviourWithSyncVarNoMiddleMiddle<UnityEngine.Vector3>
    {
        [SyncVar]
        public int implementValue = 0;
        [SyncVar(hook = nameof(OnSyncedImplementValueWithHook))]
        public int implementValueWithHook = 0;
        [SyncVar]
        public NetworkBehaviour implementTarget;
        [SyncVar]
        public NetworkIdentity implementIdentity;

        public Action<int, int> onImplementValueChanged;

        public void OnSyncedImplementValueWithHook(int oldValue, int newValue)
        {
            onImplementValueChanged?.Invoke(oldValue, newValue);
        }
    }

    public class GenericNetworkBehaviorSyncvarNoMiddleTest : ClientServerSetup<GenericBehaviourWithSyncVarNoMiddleImplement>
    {
        [Test]
        public void IsZeroByDefault()
        {
            Assert.AreEqual(0, clientComponent.baseValue);
            Assert.AreEqual(0, clientComponent.baseValueWithHook);
            Assert.AreEqual(0, clientComponent.implementValue);
            Assert.AreEqual(0, clientComponent.implementValueWithHook);
            Assert.IsNull(clientComponent.target);
            Assert.IsNull(clientComponent.targetIdentity);
            Assert.IsNull(clientComponent.implementTarget);
            Assert.IsNull(clientComponent.implementIdentity);
        }

        [UnityTest]
        public IEnumerator ChangeValue() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.baseValue = 2;

            await UniTask.WaitUntil(() => clientComponent.baseValue != 0);

            Assert.AreEqual(2, clientComponent.baseValue);
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
        public IEnumerator ChangeImplementValue() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.implementValue = 2;

            await UniTask.WaitUntil(() => clientComponent.implementValue != 0);

            Assert.AreEqual(2, clientComponent.implementValue);
        });

        [UnityTest]
        public IEnumerator ChangeImplementValueHook() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.implementValueWithHook = 2;
            clientComponent.onImplementValueChanged += (oldValue, newValue) =>
            {
                Assert.AreEqual(0, oldValue);
                Assert.AreEqual(2, newValue);
            };

            await UniTask.WaitUntil(() => clientComponent.implementValueWithHook != 0);
        });

        [UnityTest]
        public IEnumerator ChangeImplementTarget() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.implementTarget = serverComponent;

            await UniTask.WaitUntil(() => clientComponent.implementTarget != null);

            Assert.That(clientComponent.implementTarget, Is.SameAs(clientComponent));
        });

        [UnityTest]
        public IEnumerator ChangeImplementNetworkIdentity() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.implementIdentity = serverIdentity;

            await UniTask.WaitUntil(() => clientComponent.implementIdentity != null);

            Assert.That(clientComponent.implementIdentity, Is.SameAs(clientIdentity));
        });

        [UnityTest]
        public IEnumerator SpawnWithValue() => UniTask.ToCoroutine(async () =>
        {
            // create an object, set the target and spawn it
            UnityEngine.GameObject newObject = InstantiateForTest(playerPrefab);
            var newBehavior = newObject.GetComponent<GenericBehaviourWithSyncVarNoMiddleImplement>();
            newBehavior.baseValue = 2;
            newBehavior.implementValue = 222;
            newBehavior.target = serverComponent;
            newBehavior.targetIdentity = serverIdentity;
            newBehavior.implementTarget = serverComponent;
            newBehavior.implementIdentity = serverIdentity;
            serverObjectManager.Spawn(newObject);

            // wait until the client spawns it
            var newObjectId = newBehavior.NetId;
            var newClientObject = await AsyncUtil.WaitUntilSpawn(client.World, newObjectId);

            // check if the target was set correctly in the client
            var newClientBehavior = newClientObject.GetComponent<GenericBehaviourWithSyncVarNoMiddleImplement>();
            Assert.AreEqual(2, newClientBehavior.baseValue);
            Assert.AreEqual(222, newClientBehavior.implementValue);
            Assert.That(newClientBehavior.target, Is.SameAs(clientComponent));
            Assert.That(newClientBehavior.targetIdentity, Is.SameAs(clientIdentity));
            Assert.That(newClientBehavior.implementTarget, Is.SameAs(clientComponent));
            Assert.That(newClientBehavior.implementIdentity, Is.SameAs(clientIdentity));

            // cleanup
            serverObjectManager.Destroy(newObject);
        });
    }
}
