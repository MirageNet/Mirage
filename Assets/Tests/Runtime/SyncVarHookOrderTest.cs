using System;
using Mirage.Serialization;
using Mirage.Tests.Runtime.ClientServer;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    public class HookOrderBehaviour : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnValue1Changed))]
        public int value1;
        [SyncVar(hook = nameof(OnValue2Changed))]
        public int value2;

        public Action<int, int> HookCalled1;
        public Action<int, int> HookCalled2;

        void OnValue1Changed(int _, int newValue)
        {
            HookCalled1?.Invoke(value1, value2);
        }
        void OnValue2Changed(int _, int newValue)
        {
            HookCalled2?.Invoke(value1, value2);
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

            int hook1Called = 0;
            clientComponent.HookCalled1 += (v1, v2) =>
            {
                hook1Called++;
                Assert.That(v1, Is.EqualTo(Value1));
                Assert.That(v2, Is.EqualTo(Value2));
            };
            int hook2Called = 0;
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

            int hookCalled1 = 0;
            clientComponent.HookCalled1 += (v1, v2) =>
            {
                hookCalled1++;
                Assert.That(v1, Is.EqualTo(ValueNew1));
                Assert.That(v2, Is.EqualTo(ValueOld2), "Should be old value because hook is called after v1 is set, but before v2 is read");
            };
            int hookCalled2 = 0;
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
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                serverComponent.SerializeSyncVars(writer, initial);

                using (PooledNetworkReader reader = NetworkReaderPool.GetReader(writer.ToArraySegment()))
                {
                    clientComponent.DeserializeSyncVars(reader, initial);
                }
            }
        }
    }
}
