using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.Generics
{
    public class WithGenericSyncVar_behaviour<T> : NetworkBehaviour
    {
        [SyncVar]
        public T value;
    }

    public class WithGenericSyncVar_behaviourInt : WithGenericSyncVar_behaviour<int>
    {
    }
    public class WithGenericSyncVar_behaviourObject : WithGenericSyncVar_behaviour<MyClass>
    {
    }

    public class WithGenericSyncVarInt : ClientServerSetup<WithGenericSyncVar_behaviourInt>
    {
        [Test]
        public void DoesNotError()
        {
            // passes setup without errors
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator SyncToClient()
        {
            const int num = 32;
            serverComponent.value = num;

            yield return null;
            yield return null;

            Assert.That(clientComponent.value, Is.EqualTo(num));
        }
    }
    public class WithGenericSyncVarObject : ClientServerSetup<WithGenericSyncVar_behaviourObject>
    {
        [Test]
        public void DoesNotError()
        {
            // passes setup without errors
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator SyncToClient()
        {
            const int num = 32;
            serverComponent.value = new MyClass { Value = num };

            yield return null;
            yield return null;

            Assert.That(clientComponent.value, Is.Not.Null);
            Assert.That(clientComponent.value.Value, Is.EqualTo(num));
        }
    }
}
