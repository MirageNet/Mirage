using System;
using System.Collections;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.SyncVarHooks
{
    public class SyncVarHookWith0ArgBehaviour : NetworkBehaviour
    {
        public event Action onChangedCalled;

        [SyncVar(hook = nameof(OnChange))] public int var;

        private void OnChange()
        {
            onChangedCalled?.Invoke();
        }
    }

    public class SyncVarHookWith0ArgEventBehaviour : NetworkBehaviour
    {
        public event Action OnChange;
        [SyncVar(hook = nameof(OnChange))] public int var;
    }

    public class SyncVarHookWith1ArgBehaviour : NetworkBehaviour
    {
        public event Action<int> onChangedCalled;

        [SyncVar(hook = nameof(OnChange))] public int var;

        private void OnChange(int newValue)
        {
            onChangedCalled?.Invoke(newValue);
        }
    }

    public class SyncVarHookWith1ArgEventBehaviour : NetworkBehaviour
    {
        public event Action<int> OnChange;
        [SyncVar(hook = nameof(OnChange))] public int var;
    }

    public class SyncVarHookMethod0ArgWithOverLoadBehaviour : NetworkBehaviour
    {
        public event Action onChangedCalled;

        [SyncVar(hook = nameof(OnChange), hookType = SyncHookType.MethodWith0Arg)]
        public int var;

        private void OnChange()
        {
            onChangedCalled?.Invoke();
        }

        private void OnChange(int newValue)
        {
            // use log error here not assert, mirage will catch the assert execption and possible hide it.
            Debug.LogError("Should not be called");
        }

        private void OnChange(int oldValue, int newValue)
        {
            // use log error here not assert, mirage will catch the assert execption and possible hide it.
            Debug.LogError("Should not be called");
        }
    }

    public class SyncVarHookMethod1ArgWithOverLoadBehaviour : NetworkBehaviour
    {
        public event Action<int> onChangedCalled;

        [SyncVar(hook = nameof(OnChange), hookType = SyncHookType.MethodWith1Arg)]
        public int var;

        private void OnChange(int newValue)
        {
            onChangedCalled?.Invoke(newValue);
        }

        private void OnChange(int oldValue, int newValue)
        {
            // use log error here not assert, mirage will catch the assert execption and possible hide it.
            Debug.LogError("Should not be called");
        }
    }
    public class SyncVarHookMethod2ArgWithOverLoadBehaviour : NetworkBehaviour
    {
        public event Action<int> onChangedCalled;

        [SyncVar(hook = nameof(OnChange), hookType = SyncHookType.MethodWith2Arg)]
        public int var;

        private void OnChange(int newValue)
        {
            // use log error here not assert, mirage will catch the assert execption and possible hide it.
            Debug.LogError("Should not be called");
        }

        private void OnChange(int oldValue, int newValue)
        {
            onChangedCalled?.Invoke(newValue);
        }
    }

    public class SyncVarHookWith0Arg : ClientServerSetup<SyncVarHookWith0ArgBehaviour>
    {
        [UnityTest]
        public IEnumerator HookIsCalledWithNoValues()
        {
            const int value = 50;

            var sub = Substitute.For<Action>();
            clientComponent.onChangedCalled += sub;
            serverComponent._nextSyncTime = 0; // make sure syncs quick
            serverComponent.var = value;
            yield return null;
            yield return null;

            sub.Received(1).Invoke();
        }
    }

    public class SyncVarHookWith1Arg : ClientServerSetup<SyncVarHookWith1ArgBehaviour>
    {
        [UnityTest]
        public IEnumerator HookIsCalledWithNewValue()
        {
            const int value = 50;

            var sub = Substitute.For<Action<int>>();
            clientComponent.onChangedCalled += sub;
            serverComponent._nextSyncTime = 0; // make sure syncs quick
            serverComponent.var = value;
            yield return null;
            yield return null;

            sub.Received(1).Invoke(value);
        }
    }

    public class SyncVarHookWith0ArgEvent : ClientServerSetup<SyncVarHookWith0ArgEventBehaviour>
    {
        [UnityTest]
        public IEnumerator HookIsCalledWithNoValues()
        {
            const int value = 50;

            var sub = Substitute.For<Action>();
            clientComponent.OnChange += sub;
            serverComponent._nextSyncTime = 0; // make sure syncs quick
            serverComponent.var = value;
            yield return null;
            yield return null;

            sub.Received(1).Invoke();
        }
    }

    public class SyncVarHookWith1ArgEvent : ClientServerSetup<SyncVarHookWith1ArgEventBehaviour>
    {
        [UnityTest]
        public IEnumerator HookIsCalledWithNewValue()
        {
            const int value = 50;

            var sub = Substitute.For<Action<int>>();
            clientComponent.OnChange += sub;
            serverComponent._nextSyncTime = 0; // make sure syncs quick
            serverComponent.var = value;
            yield return null;
            yield return null;

            sub.Received(1).Invoke(value);
        }
    }

    public class SyncVarHookMethod0ArgWithOverLoad : ClientServerSetup<SyncVarHookMethod0ArgWithOverLoadBehaviour>
    {
        [UnityTest]
        public IEnumerator HookIsCalledWithNoValues()
        {
            const int value = 50;

            var sub = Substitute.For<Action>();
            clientComponent.onChangedCalled += sub;
            serverComponent._nextSyncTime = 0; // make sure syncs quick
            serverComponent.var = value;
            yield return null;
            yield return null;

            sub.Received(1).Invoke();
        }
    }

    public class SyncVarHookMethod1ArgWithOverLoad : ClientServerSetup<SyncVarHookMethod1ArgWithOverLoadBehaviour>
    {
        [UnityTest]
        public IEnumerator HookIsCalledWithNewValue()
        {
            const int value = 50;

            var sub = Substitute.For<Action<int>>();
            clientComponent.onChangedCalled += sub;
            serverComponent._nextSyncTime = 0; // make sure syncs quick
            serverComponent.var = value;
            yield return null;
            yield return null;

            sub.Received(1).Invoke(value);
        }
    }

    public class SyncVarHookMethod2ArgWithOverLoad : ClientServerSetup<SyncVarHookMethod2ArgWithOverLoadBehaviour>
    {
        [UnityTest]
        public IEnumerator HookIsCalledWithNewValue()
        {
            const int value = 50;

            var sub = Substitute.For<Action<int>>();
            clientComponent.onChangedCalled += sub;
            serverComponent._nextSyncTime = 0; // make sure syncs quick
            serverComponent.var = value;
            yield return null;
            yield return null;

            sub.Received(1).Invoke(value);
        }
    }
}
