using System;
using System.Collections;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
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

    public class SyncVarHookWith1Arg : ClientServerSetup<SyncVarHookWith1ArgBehaviour>
    {
        [UnityTest]
        public IEnumerator HookIsCalledWithNewValue()
        {
            const int value = 50;

            var sub = Substitute.For<Action<int>>();
            clientComponent.onChangedCalled += sub;
            serverComponent._lastSyncTime = 0; // make sure syncs quick
            serverComponent.var = value;
            yield return null;
            yield return null;

            sub.Received(1).Invoke(value);
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
            serverComponent._lastSyncTime = 0; // make sure syncs quick
            serverComponent.var = value;
            yield return null;
            yield return null;

            sub.Received(1).Invoke(value);
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
            serverComponent._lastSyncTime = 0; // make sure syncs quick
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
            serverComponent._lastSyncTime = 0; // make sure syncs quick
            serverComponent.var = value;
            yield return null;
            yield return null;

            sub.Received(1).Invoke(value);
        }
    }
}
