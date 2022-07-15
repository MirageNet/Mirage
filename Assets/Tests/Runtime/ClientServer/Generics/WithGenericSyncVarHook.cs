using System;
using System.Collections;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.Generics
{
    public class WithGenericSyncVarHook_behaviour<T> : NetworkBehaviour
    {
        public event Action<T, T> hookCalled;

        [SyncVar(hook = nameof(onValueChanged))]
        public T value;

        private void onValueChanged(T oldValue, T newValue)
        {
            hookCalled?.Invoke(oldValue, newValue);
        }
    }

    public class WithGenericSyncVarHook_behaviourInt : WithGenericSyncVarHook_behaviour<int>
    {
    }
    public class WithGenericSyncVarHook_behaviourObject : WithGenericSyncVarHook_behaviour<MyClass>
    {
    }

    public class WithGenericSyncVarHookInt : ClientServerSetup<WithGenericSyncVarHook_behaviourInt>
    {
        [Test]
        public void DoesNotError()
        {
            // passes setup without errors
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator HookMethodCalled()
        {
            const int num = 32;
            Action<int, int> hook = Substitute.For<Action<int, int>>();
            clientComponent.hookCalled += hook;
            serverComponent.value = num;

            yield return null;
            yield return null;

            hook.Received(1).Invoke(0, num);
        }
    }

    public class WithGenericSyncVarHookObject : ClientServerSetup<WithGenericSyncVarHook_behaviourObject>
    {
        [Test]
        public void DoesNotError()
        {
            // passes setup without errors
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator HookMethodCalled()
        {
            const int num = 32;
            Action<MyClass, MyClass> hook = Substitute.For<Action<MyClass, MyClass>>();
            clientComponent.hookCalled += hook;
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
