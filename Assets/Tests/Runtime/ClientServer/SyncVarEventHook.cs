using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class BehaviourWithSyncVarEvent : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnHealthChanged))]
        public int health;

        public event Action<int, int> OnHealthChanged;
    }

    public class SyncVarEventHook : ClientServerSetup<BehaviourWithSyncVarEvent>
    {
        [UnityTest]
        public IEnumerator SyncVarHookEventIsCalled()
        {
            const int SValue = 10;
            const int CValue = 2;
            int clientA = default;
            int clientB = default;
            int called = 0;
            clientComponent.health = CValue;
            clientComponent.OnHealthChanged += (a, b) =>
            {
                clientA = a;
                clientB = b;
                called++;
            };
            serverComponent.health = SValue;
            yield return null;
            yield return null;

            Assert.That(called, Is.EqualTo(1));
            Assert.That(clientA, Is.EqualTo(CValue));
            Assert.That(clientB, Is.EqualTo(SValue));
        }
    }
}
