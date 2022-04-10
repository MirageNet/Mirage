using System;
using System.Collections;
using NSubstitute;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class SyncVarHookWith1ArgBehaviour : NetworkBehaviour
    {
        public event Action<int> onChangedCalled;

        [SyncVar(hook = nameof(OnChange))] public int var;

        void OnChange(int newValue)
        {
            onChangedCalled?.Invoke(newValue);
        }
    }

    public class SyncVarHookWith1ArgEventBehaviour : NetworkBehaviour
    {
        public event Action<int> OnChange;
        [SyncVar(hook = nameof(OnChange))] public int var;
    }

    public class SyncVarHookWith1Arg : ClientServerSetup<SyncVarHookWith1ArgBehaviour>
    {
        [UnityTest]
        public IEnumerator HookIsCalledWithNewValue()
        {
            const int value = 50;

            Action<int> sub = Substitute.For<Action<int>>();
            clientComponent.onChangedCalled += sub;
            serverComponent.lastSyncTime = 0; // make sure syncs quick
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

            Action<int> sub = Substitute.For<Action<int>>();
            clientComponent.OnChange += sub;
            serverComponent.lastSyncTime = 0; // make sure syncs quick
            serverComponent.var = value;
            yield return null;
            yield return null;

            sub.Received(1).Invoke(value);
        }
    }
}
