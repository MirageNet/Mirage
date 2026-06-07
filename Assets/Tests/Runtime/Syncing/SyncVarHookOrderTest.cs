using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Mirage.Serialization;
using Mirage.Tests.Runtime.Host;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Syncing
{
    public class HookOrderBehaviour : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnValue1Changed))]
        public int value1;
        [SyncVar(hook = nameof(OnValue2Changed))]
        public int value2;

        public Action<int, int> HookCalled1;
        public Action<int, int> HookCalled2;

        public readonly List<(string where, int value)> CallOrder = new List<(string where, int value)>();

        private void Awake()
        {
            Identity.OnStartClient.AddListener(OnStartClient);
        }

        private void OnStartClient()
        {
            CallOrder.Add(("StartClient", value1));
        }

        private void OnValue1Changed(int _, int newValue)
        {
            HookCalled1?.Invoke(value1, value2);
            CallOrder.Add(("Hook", newValue));
        }

        private void OnValue2Changed(int _, int newValue)
        {
            HookCalled2?.Invoke(value1, value2);
            CallOrder.Add(("Hook", newValue));
        }
    }

    public class SyncVarHookOrderTest : ClientServerSetup<HookOrderBehaviour>
    {
        [Test]
        public void HooksCalledAfterAllValuesSetForInitial()
        {
            const int Value1 = 10;
            const int Value2 = 20;
            serverComponent.value1 = Value1;
            serverComponent.value2 = Value2;

            var hook1Called = 0;
            clientComponent.HookCalled1 += (v1, v2) =>
            {
                hook1Called++;
                Assert.That(v1, Is.EqualTo(Value1));
                Assert.That(v2, Is.EqualTo(Value2));
            };
            var hook2Called = 0;
            clientComponent.HookCalled2 += (v1, v2) =>
            {
                hook2Called++;
                Assert.That(v1, Is.EqualTo(Value1));
                Assert.That(v2, Is.EqualTo(Value2));
            };

            SendSyncvars(true);

            Assert.That(hook1Called, Is.EqualTo(1));
            Assert.That(hook2Called, Is.EqualTo(1));
        }

        [Test]
        public void HooksAreCalledInOrderForLaterUpdates()
        {
            const int ValueOld1 = 1;
            const int ValueOld2 = 2;
            const int ValueNew1 = 10;
            const int ValueNew2 = 20;

            // set initial values
            serverComponent.value1 = ValueOld1;
            serverComponent.value2 = ValueOld2;

            SendSyncvars(true);

            serverComponent.value1 = ValueNew1;
            serverComponent.value2 = ValueNew2;

            var hookCalled1 = 0;
            clientComponent.HookCalled1 += (v1, v2) =>
            {
                hookCalled1++;
                Assert.That(v1, Is.EqualTo(ValueNew1));
                Assert.That(v2, Is.EqualTo(ValueOld2), "Should be old value because hook is called after v1 is set, but before v2 is read");
            };
            var hookCalled2 = 0;
            clientComponent.HookCalled2 += (v1, v2) =>
            {
                hookCalled2++;
                Assert.That(v1, Is.EqualTo(ValueNew1));
                Assert.That(v2, Is.EqualTo(ValueNew2));
            };

            SendSyncvars(false);

            Assert.That(hookCalled1, Is.EqualTo(1));
            Assert.That(hookCalled2, Is.EqualTo(1));
        }

        private void SendSyncvars(bool initial)
        {
            using (var writer = NetworkWriterPool.GetWriter())
            {
                serverComponent.SerializeSyncVars(writer, initial);

                using (var reader = NetworkReaderPool.GetReader(writer.ToArraySegment(), clientComponent.World))
                {
                    clientComponent.DeserializeSyncVars(reader, initial);
                }
            }
        }

        [UnityTest]
        public IEnumerator HooksCalledBeforeStartClientOnSpawn() => UniTask.ToCoroutine(async () =>
        {
            var newObject = InstantiateForTest(_characterPrefabGo);
            var serverComp = newObject.GetComponent<HookOrderBehaviour>();
            serverComp.value1 = 123;
            serverObjectManager.Spawn(newObject);

            var newObjectId = serverComp.NetId;
            var newClientObject = await AsyncUtil.WaitUntilSpawn(client.World, newObjectId);
            var clientComp = newClientObject.GetComponent<HookOrderBehaviour>();

            Assert.That(clientComp.CallOrder, Is.EquivalentTo(new[] { ("Hook", 123), ("StartClient", 123) }));
            Assert.That(clientComp.CallOrder[0], Is.EqualTo(("Hook", 123)));
            Assert.That(clientComp.CallOrder[1], Is.EqualTo(("StartClient", 123)));
        });

        [UnityTest]
        public IEnumerator HooksNotCalledIfValueIsDefaultOnSpawn() => UniTask.ToCoroutine(async () =>
        {
            var newObject = InstantiateForTest(_characterPrefabGo);
            var serverComp = newObject.GetComponent<HookOrderBehaviour>();
            // Keep value1 and value2 as default (0)
            serverObjectManager.Spawn(newObject);

            var newObjectId = serverComp.NetId;
            var newClientObject = await AsyncUtil.WaitUntilSpawn(client.World, newObjectId);
            var clientComp = newClientObject.GetComponent<HookOrderBehaviour>();

            // Hook should not be called because 0 is already the default value on the client
            Assert.That(clientComp.CallOrder, Is.EquivalentTo(new[] { ("StartClient", 0) }));
            Assert.That(clientComp.CallOrder[0], Is.EqualTo(("StartClient", 0)));
        });
    }

    [TestFixture]
    public class SyncVarHookOrderHostTest : HostSetup<HookOrderBehaviour>
    {
        [UnityTest]
        public IEnumerator HostHookBehaviorTests() => UniTask.ToCoroutine(async () =>
        {
            var newObject = InstantiateForTest(_characterPrefabGo);
            var comp = newObject.GetComponent<HookOrderBehaviour>();

            // 1. Set before spawn
            comp.value1 = 999;
            Assert.That(comp.CallOrder, Is.Empty, "Hook should not be called before spawn");

            // 2. Setup listener for OnStartServer to set value
            comp.Identity.OnStartServer.AddListener(() =>
            {
                comp.value1 = 123;
            });

            // 3. Setup listener for OnStartClient to set value
            comp.Identity.OnStartClient.AddListener(() =>
            {
                comp.value1 = 456;
            });

            // Now spawn the object on the host
            serverObjectManager.Spawn(newObject);

            // Wait a frame for initialization to complete
            await UniTask.DelayFrame(1);

            Assert.That(comp.CallOrder, Has.Count.EqualTo(3));
            Assert.That(comp.CallOrder[0], Is.EqualTo(("Hook", 123)), "OnStartServer modification should trigger hook");
            Assert.That(comp.CallOrder[1], Is.EqualTo(("StartClient", 123)), "Awake listener should record StartClient");
            Assert.That(comp.CallOrder[2], Is.EqualTo(("Hook", 456)), "OnStartClient modification should trigger hook");
        });
    }
}
