using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class BehaviourWithSyncVarOnServerEvent : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnHealthChanged), invokeHookOnServer = true)]
        public int health;

        public event Action<int, int> OnHealthChanged;
    }

    public class BehaviourWithSyncVarOnServerMethod : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnHealthChanged), invokeHookOnServer = true)]
        public int health;

        public int called = 0;

        public void OnHealthChanged(int oldValue, int newValue)
        {
            if(!IsServer) return;

            called++;
        }
    }

    public class SyncVarFireHookEventOnServerTests : ClientServerSetup<BehaviourWithSyncVarOnServerEvent>
    {
        [UnityTest]
        public IEnumerator SyncVarHookEventIsCalledOnServer()
        {
            const int SValue = 10;
            const int CValue = 0;
            int oldValue = default;
            int newValue = default;
            int called = 0;

            serverComponent.OnHealthChanged += (a, b) =>
            {
                oldValue = a;
                newValue = b;
                called++;
            };

            serverComponent.health = CValue;

            yield return null;
            yield return null;

            Assert.That(called, Is.EqualTo(1));
            Assert.That(oldValue, Is.EqualTo(CValue));
            Assert.That(newValue, Is.EqualTo(SValue));

        }
    }

    public class SyncVarFireHookMethodOnServerTests : ClientServerSetup<BehaviourWithSyncVarOnServerMethod>
    {
        [UnityTest]
        public IEnumerator SyncVarHookEventIsCalledOnServer()
        {
            const int SValue = 10;
            int oldValue = serverComponent.health;
            int newValue = default;

            serverComponent.health = SValue;

            yield return null;
            yield return null;

            newValue = serverComponent.health;

            Assert.That(serverComponent.called, Is.EqualTo(1));
            Assert.That(oldValue, Is.Not.EqualTo(serverComponent.health));
            Assert.That(newValue, Is.EqualTo(serverComponent.health));
        }
    }
}
