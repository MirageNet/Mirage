using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Syncing
{
    public class MockPlayerHook : NetworkBehaviour
    {
        [SyncVar(hook = nameof(MyNumberEventChanged), invokeHookOnServer = true)]
        public int myNumberEvent;

        [SyncVar(hook = nameof(MyNumberEventChanged), invokeHookOnServer = false)]
        public int myNumberEvent2;


        public event Action<int> MyNumberEventChanged;


        [SyncVar(hook = nameof(MyNumberMethodChanged), invokeHookOnServer = true)]
        public int myNumberMethod;

        public event Action<int> MyNumberMethodChangedCalled;
        public void MyNumberMethodChanged(int newValue)
        {
            MyNumberMethodChangedCalled?.Invoke(newValue);
        }
    }

    public class SyncDirectionFromServerHostToOwner : SyncDirectionTestBase_Host
    {
        /// <summary>
        /// object on host, owned by client 2
        /// </summary>
        private MockPlayerHook hostWithHook;
        /// <summary>
        /// object on client 2, owned by client 2
        /// </summary>
        private MockPlayerHook ownerWithHook;

        protected override async UniTask LateSetup()
        {
            await base.LateSetup();

            var prefab = CreateBehaviour<MockPlayerHook>(true).Identity;
            prefab.PrefabHash = 456;

            _remoteClients[0].ClientObjectManager.RegisterPrefab(prefab);
            clientObjectManager.RegisterPrefab(prefab);

            var clone = InstantiateForTest(prefab);
            clone.gameObject.SetActive(true);
            hostWithHook = clone.GetComponent<MockPlayerHook>();
            serverObjectManager.Spawn(clone, owner: _serverPlayer2);

            await UniTask.DelayFrame(2);

            ownerWithHook = _remoteClients[0].Get(hostWithHook);
        }

        [UnityTest]
        public IEnumerator SettingValueOnHostInvokesHookEventForBoth()
        {
            var hostHookInvoked = new List<int>();
            var ownerHookInvoked = new List<int>();
            hostWithHook.MyNumberEventChanged += (n) => hostHookInvoked.Add(n);
            ownerWithHook.MyNumberEventChanged += (n) => ownerHookInvoked.Add(n);

            const int Value1 = 10;

            hostWithHook.myNumberEvent = Value1;


            Assert.That(hostHookInvoked, Has.Count.EqualTo(1), "hook not called on host");
            Assert.That(hostHookInvoked[0], Is.EqualTo(Value1));

            yield return null;
            yield return null;


            Assert.That(ownerHookInvoked, Has.Count.EqualTo(1), "hook not called on server");
            Assert.That(ownerHookInvoked[0], Is.EqualTo(Value1));

            Assert.That(hostWithHook.myNumberEvent, Is.EqualTo(Value1));
            Assert.That(ownerWithHook.myNumberEvent, Is.EqualTo(Value1));
        }

        [UnityTest]
        public IEnumerator SettingValueOnHostInvokesHookMethodForBoth()
        {
            var hostHookInvoked = new List<int>();
            var ownerHookInvoked = new List<int>();
            hostWithHook.MyNumberMethodChangedCalled += (n) => hostHookInvoked.Add(n);
            ownerWithHook.MyNumberMethodChangedCalled += (n) => ownerHookInvoked.Add(n);

            const int Value1 = 10;

            hostWithHook.myNumberMethod = Value1;


            Assert.That(hostHookInvoked, Has.Count.EqualTo(1), "hook not called on host");
            Assert.That(hostHookInvoked[0], Is.EqualTo(Value1));

            yield return null;
            yield return null;


            Assert.That(ownerHookInvoked, Has.Count.EqualTo(1), "hook not called on server");
            Assert.That(ownerHookInvoked[0], Is.EqualTo(Value1));

            Assert.That(hostWithHook.myNumberMethod, Is.EqualTo(Value1));
            Assert.That(ownerWithHook.myNumberMethod, Is.EqualTo(Value1));
        }
    }
}
