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
        public T valueWithHook;

        void onValueChanged(T oldValue, T newValue)
        {
            hookCalled.Invoke(oldValue, newValue);
        }

        public event Action<T, T> eventHook;

        [SyncVar(hook = nameof(eventHook))]
        public T valueWithEvent;
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
            serverComponent.valueWithHook = num;

            yield return null;
            yield return null;

            hook.Received(1).Invoke(default, num);
        }

        [UnityTest]
        public IEnumerator HookEventCalled()
        {
            const int num = 32;
            Action<int, int> hook = Substitute.For<Action<int, int>>();
            clientComponent.eventHook += hook;
            serverComponent.valueWithEvent = num;

            yield return null;
            yield return null;

            hook.Received(1).Invoke(default, num);
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
            serverComponent.valueWithHook = new MyClass { Value = num };

            yield return null;
            yield return null;

            hook.Received(1).Invoke(default, Arg.Is<MyClass>(x => x != null && x.Value == num));
        }

        [UnityTest]
        public IEnumerator HookEventCalled()
        {
            const int num = 32;
            Action<MyClass, MyClass> hook = Substitute.For<Action<MyClass, MyClass>>();

            clientComponent.eventHook += hook;
            serverComponent.valueWithEvent = new MyClass { Value = num };

            yield return null;
            yield return null;

            hook.Received(1).Invoke(default, Arg.Is<MyClass>(x => x != null && x.Value == num));
        }
    }
}
