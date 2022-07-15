using System;
using System.Collections;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.Generics
{
    public class WithGenericSyncVarEvent_behaviour<T> : NetworkBehaviour
    {
        public event Action<T, T> hook;

        [SyncVar(hook = nameof(hook))]
        public T value;
    }

    public class WithGenericSyncVarEvent_behaviourInt : WithGenericSyncVarEvent_behaviour<int>
    {
    }
    public class WithGenericSyncVarEvent_behaviourObject : WithGenericSyncVarEvent_behaviour<MyClass>
    {
    }

    public class WithGenericSyncVarEventInt : ClientServerSetup<WithGenericSyncVarEvent_behaviourInt>
    {
        [Test]
        public void DoesNotError()
        {
            // passes setup without errors
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator HookEventCalled()
        {
            const int num = 32;
            var hook = Substitute.For<Action<int, int>>();
            clientComponent.hook += hook;
            serverComponent.value = num;

            yield return null;
            yield return null;

            hook.Received(1).Invoke(0, num);
        }
    }
    public class WithGenericSyncVarEventObject : ClientServerSetup<WithGenericSyncVarEvent_behaviourObject>
    {
        [Test]
        public void DoesNotError()
        {
            // passes setup without errors
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator HookEventCalled()
        {
            const int num = 32;
            var hook = Substitute.For<Action<MyClass, MyClass>>();

            clientComponent.hook += hook;
            serverComponent.value = new MyClass { Value = num };

            yield return null;
            yield return null;

            hook.Received(1).Invoke(
                Arg.Is<MyClass>(x => x == null),
                Arg.Is<MyClass>(x => x != null && x.Value == num)
            );
        }
    }
}
