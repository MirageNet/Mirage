using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Syncing
{
    public class MockPlayerInvokeHooks : NetworkBehaviour
    {
        [SyncVar(hook = nameof(EventServerOwner), invokeHookOnServer = true, invokeHookOnOwner = true)]
        public int FieldServerOwner;
        public event Action<int> EventServerOwner;

        [SyncVar(hook = nameof(EventServer), invokeHookOnServer = true, invokeHookOnOwner = false)]
        public int FieldServer;
        public event Action<int> EventServer;

        [SyncVar(hook = nameof(EventOwner), invokeHookOnServer = false, invokeHookOnOwner = true)]
        public int FieldOwner;
        public event Action<int> EventOwner;

        [SyncVar(hook = nameof(EventNone), invokeHookOnServer = false, invokeHookOnOwner = false)]
        public int FieldNone;
        public event Action<int> EventNone;
    }

    public class SyncDirectionHookFromOwner : SyncDirectionTestBase<MockPlayerInvokeHooks>
    {
        private const int VALUE_1 = 10;
        private List<int> _serverHookInvoked;
        private List<int> _ownerHookInvoked;

        protected override async UniTask LateSetup()
        {
            await base.LateSetup();
            SetDirection(SyncFrom.Owner, SyncTo.Server);

            _serverHookInvoked = new List<int>();
            _ownerHookInvoked = new List<int>();
        }

        [UnityTest]
        public IEnumerator HookInvokesForOwnerOnly()
        {
            ServerComponent.EventOwner += (n) => _serverHookInvoked.Add(n);
            OwnerComponent.EventOwner += (n) => _ownerHookInvoked.Add(n);
            // set on owner
            OwnerComponent.FieldOwner = VALUE_1;

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(1), "Hook should be called on owner right away");
            Assert.That(_ownerHookInvoked[0], Is.EqualTo(VALUE_1));

            yield return null;
            yield return null;

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(0), "Hook should not have been invoked on server");

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(1), "Hook should not have been called more than once on owner");

            Assert.That(ServerComponent.FieldOwner, Is.EqualTo(VALUE_1), "field should have been updated");
            Assert.That(OwnerComponent.FieldOwner, Is.EqualTo(VALUE_1), "field should have been updated");
        }

        [UnityTest]
        public IEnumerator HookInvokesForServerOnly()
        {
            ServerComponent.EventServer += (n) => _serverHookInvoked.Add(n);
            OwnerComponent.EventServer += (n) => _ownerHookInvoked.Add(n);
            // set on owner
            OwnerComponent.FieldServer = VALUE_1;

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(0), "Should not have been invoked on owner");

            yield return null;
            yield return null;

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(1), "Should have been invoked on server");
            Assert.That(_serverHookInvoked[0], Is.EqualTo(VALUE_1));

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(0), "Should still not be invoked on owner");

            Assert.That(ServerComponent.FieldServer, Is.EqualTo(VALUE_1), "field should have been updated");
            Assert.That(OwnerComponent.FieldServer, Is.EqualTo(VALUE_1), "field should have been updated");
        }

        [UnityTest]
        public IEnumerator HookInvokesForOwnerServer()
        {
            ServerComponent.EventServerOwner += (n) => _serverHookInvoked.Add(n);
            OwnerComponent.EventServerOwner += (n) => _ownerHookInvoked.Add(n);
            // set on owner
            OwnerComponent.FieldServerOwner = VALUE_1;

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(1), "Should be invoked on owner right away");
            Assert.That(_ownerHookInvoked[0], Is.EqualTo(VALUE_1));

            yield return null;
            yield return null;

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(1), "Should be invoked on server when it is received");
            Assert.That(_serverHookInvoked[0], Is.EqualTo(VALUE_1));

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(1), "Should not have been invoked any more times");

            Assert.That(ServerComponent.FieldServerOwner, Is.EqualTo(VALUE_1), "field should have been updated");
            Assert.That(OwnerComponent.FieldServerOwner, Is.EqualTo(VALUE_1), "field should have been updated");
        }

        [UnityTest]
        public IEnumerator HookInvokesForNone()
        {
            ServerComponent.EventNone += (n) => _serverHookInvoked.Add(n);
            OwnerComponent.EventNone += (n) => _ownerHookInvoked.Add(n);
            // set on owner
            OwnerComponent.FieldNone = VALUE_1;

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(0), "Should not have been invoked on owner");

            yield return null;
            yield return null;

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(0), "Should not have been invoked on server");

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(0), "Should still not be invoked on owner");

            Assert.That(ServerComponent.FieldNone, Is.EqualTo(VALUE_1), "field should have been updated");
            Assert.That(OwnerComponent.FieldNone, Is.EqualTo(VALUE_1), "field should have been updated");
        }
    }

    public class SyncDirectionHookFromServer : SyncDirectionTestBase<MockPlayerInvokeHooks>
    {
        private const int VALUE_1 = 10;
        private List<int> _serverHookInvoked;
        private List<int> _ownerHookInvoked;
        private List<int> _observerHookInvoked;

        protected override async UniTask LateSetup()
        {
            await base.LateSetup();
            SetDirection(SyncFrom.Server, SyncTo.OwnerAndObservers);

            _serverHookInvoked = new List<int>();
            _ownerHookInvoked = new List<int>();
            _observerHookInvoked = new List<int>();
        }

        [UnityTest]
        public IEnumerator HookInvokesForOwnerOnly()
        {
            ServerComponent.EventOwner += (n) => _serverHookInvoked.Add(n);
            OwnerComponent.EventOwner += (n) => _ownerHookInvoked.Add(n);
            ObserverComponent.EventOwner += (n) => _observerHookInvoked.Add(n);
            // set on server   
            ServerComponent.FieldOwner = VALUE_1;

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(0), "Should not have been invoked on server");

            yield return null;
            yield return null;

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(1), "Should have been invoked on owner");
            Assert.That(_ownerHookInvoked[0], Is.EqualTo(VALUE_1));
            Assert.That(_observerHookInvoked, Has.Count.EqualTo(1), "Should have been invoked on observer");
            Assert.That(_observerHookInvoked[0], Is.EqualTo(VALUE_1));

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(0), "Should still not have been invoked on server");

            Assert.That(ServerComponent.FieldOwner, Is.EqualTo(VALUE_1), "field should have been updated");
            Assert.That(OwnerComponent.FieldOwner, Is.EqualTo(VALUE_1), "field should have been updated");
        }

        [UnityTest]
        public IEnumerator HookInvokesForServerOnly()
        {
            ServerComponent.EventServer += (n) => _serverHookInvoked.Add(n);
            OwnerComponent.EventServer += (n) => _ownerHookInvoked.Add(n);
            ObserverComponent.EventServer += (n) => _observerHookInvoked.Add(n);
            // set on server   
            ServerComponent.FieldServer = VALUE_1;

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(1), "Should be called on server right away");
            Assert.That(_serverHookInvoked[0], Is.EqualTo(VALUE_1));

            yield return null;
            yield return null;

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(1), "Should have been invoked on owner");
            Assert.That(_ownerHookInvoked[0], Is.EqualTo(VALUE_1));
            Assert.That(_observerHookInvoked, Has.Count.EqualTo(1), "Should have been invoked on observer");
            Assert.That(_observerHookInvoked[0], Is.EqualTo(VALUE_1));

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(1), "Should still not be invoked on owner");

            Assert.That(ServerComponent.FieldServer, Is.EqualTo(VALUE_1), "field should have been updated");
            Assert.That(OwnerComponent.FieldServer, Is.EqualTo(VALUE_1), "field should have been updated");
        }

        [UnityTest]
        public IEnumerator HookInvokesForOwnerServer()
        {
            ServerComponent.EventServerOwner += (n) => _serverHookInvoked.Add(n);
            OwnerComponent.EventServerOwner += (n) => _ownerHookInvoked.Add(n);
            ObserverComponent.EventServerOwner += (n) => _observerHookInvoked.Add(n);
            // set on server   
            ServerComponent.FieldServerOwner = VALUE_1;

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(1), "Should be called on server right away");
            Assert.That(_serverHookInvoked[0], Is.EqualTo(VALUE_1));

            yield return null;
            yield return null;

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(1), "Should be called on owner when it is received");
            Assert.That(_ownerHookInvoked[0], Is.EqualTo(VALUE_1));
            Assert.That(_observerHookInvoked, Has.Count.EqualTo(1), "Should have been invoked on observer");
            Assert.That(_observerHookInvoked[0], Is.EqualTo(VALUE_1));

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(1), "Should not have been invoked any more times");

            Assert.That(ServerComponent.FieldServerOwner, Is.EqualTo(VALUE_1), "field should have been updated");
            Assert.That(OwnerComponent.FieldServerOwner, Is.EqualTo(VALUE_1), "field should have been updated");
        }

        [UnityTest]
        public IEnumerator HookInvokesForNone()
        {
            ServerComponent.EventNone += (n) => _serverHookInvoked.Add(n);
            OwnerComponent.EventNone += (n) => _ownerHookInvoked.Add(n);
            ObserverComponent.EventNone += (n) => _observerHookInvoked.Add(n);
            // set on server   
            ServerComponent.FieldNone = VALUE_1;

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(0), "Should not have been invoked on server");

            yield return null;
            yield return null;

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(1), "Should be called on owner when it is received");
            Assert.That(_ownerHookInvoked[0], Is.EqualTo(VALUE_1));
            Assert.That(_observerHookInvoked, Has.Count.EqualTo(1), "Should have been invoked on observer");
            Assert.That(_observerHookInvoked[0], Is.EqualTo(VALUE_1));

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(0), "Should still not be invoked on server");

            Assert.That(ServerComponent.FieldNone, Is.EqualTo(VALUE_1), "field should have been updated");
            Assert.That(OwnerComponent.FieldNone, Is.EqualTo(VALUE_1), "field should have been updated");
        }
    }

    public class SyncDirectionHookFromOwnerToObservers : SyncDirectionTestBase<MockPlayerInvokeHooks>
    {
        private const int VALUE_1 = 10;
        private List<int> _serverHookInvoked;
        private List<int> _ownerHookInvoked;
        private List<int> _observerHookInvoked;

        protected override async UniTask LateSetup()
        {
            await base.LateSetup();
            SetDirection(SyncFrom.Owner, SyncTo.Server | SyncTo.ObserversOnly);

            _serverHookInvoked = new List<int>();
            _ownerHookInvoked = new List<int>();
            _observerHookInvoked = new List<int>();
        }

        [UnityTest]
        public IEnumerator HookInvokesForOwnerOnly()
        {
            ServerComponent.EventOwner += (n) => _serverHookInvoked.Add(n);
            OwnerComponent.EventOwner += (n) => _ownerHookInvoked.Add(n);
            ObserverComponent.EventOwner += (n) => _observerHookInvoked.Add(n);
            // set on owner
            OwnerComponent.FieldOwner = VALUE_1;

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(1), "Hook should be called on owner right away");
            Assert.That(_ownerHookInvoked[0], Is.EqualTo(VALUE_1));

            yield return null;
            yield return null;

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(0), "Hook should not have been invoked on server");
            Assert.That(_observerHookInvoked, Has.Count.EqualTo(1), "Should have been invoked on observer");
            Assert.That(_observerHookInvoked[0], Is.EqualTo(VALUE_1));

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(1), "Hook should not have been invoked any more times");

            Assert.That(ServerComponent.FieldOwner, Is.EqualTo(VALUE_1), "field should have been updated");
            Assert.That(OwnerComponent.FieldOwner, Is.EqualTo(VALUE_1), "field should have been updated");
        }

        [UnityTest]
        public IEnumerator HookInvokesForServerOnly()
        {
            ServerComponent.EventServer += (n) => _serverHookInvoked.Add(n);
            OwnerComponent.EventServer += (n) => _ownerHookInvoked.Add(n);
            ObserverComponent.EventServer += (n) => _observerHookInvoked.Add(n);
            // set on owner
            OwnerComponent.FieldServer = VALUE_1;

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(0), "Should not have been invoked on owner");

            yield return null;
            yield return null;

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(1), "Should have been invoked on server");
            Assert.That(_serverHookInvoked[0], Is.EqualTo(VALUE_1));
            Assert.That(_observerHookInvoked, Has.Count.EqualTo(1), "Should have been invoked on observer");
            Assert.That(_observerHookInvoked[0], Is.EqualTo(VALUE_1));

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(0), "Should still not be invoked on owner");

            Assert.That(ServerComponent.FieldServer, Is.EqualTo(VALUE_1), "field should have been updated");
            Assert.That(OwnerComponent.FieldServer, Is.EqualTo(VALUE_1), "field should have been updated");
        }

        [UnityTest]
        public IEnumerator HookInvokesForOwnerServer()
        {
            ServerComponent.EventServerOwner += (n) => _serverHookInvoked.Add(n);
            OwnerComponent.EventServerOwner += (n) => _ownerHookInvoked.Add(n);
            ObserverComponent.EventServerOwner += (n) => _observerHookInvoked.Add(n);
            // set on owner
            OwnerComponent.FieldServerOwner = VALUE_1;

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(1), "Should be called on owner right away");
            Assert.That(_ownerHookInvoked[0], Is.EqualTo(VALUE_1));

            yield return null;
            yield return null;

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(1), "Should be called on server when it is received");
            Assert.That(_serverHookInvoked[0], Is.EqualTo(VALUE_1));
            Assert.That(_observerHookInvoked, Has.Count.EqualTo(1), "Should have been invoked on observer");
            Assert.That(_observerHookInvoked[0], Is.EqualTo(VALUE_1));

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(1), "Should not have been invoked any more times");

            Assert.That(ServerComponent.FieldServerOwner, Is.EqualTo(VALUE_1), "field should have been updated");
            Assert.That(OwnerComponent.FieldServerOwner, Is.EqualTo(VALUE_1), "field should have been updated");
        }

        [UnityTest]
        public IEnumerator HookInvokesForNone()
        {
            ServerComponent.EventNone += (n) => _serverHookInvoked.Add(n);
            OwnerComponent.EventNone += (n) => _ownerHookInvoked.Add(n);
            ObserverComponent.EventNone += (n) => _observerHookInvoked.Add(n);
            // set on owner
            OwnerComponent.FieldNone = VALUE_1;

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(0), "Should not have been invoked on owner");

            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;

            Assert.That(_serverHookInvoked, Has.Count.EqualTo(0), "Hook should not have been invoked on server");
            Assert.That(_observerHookInvoked, Has.Count.EqualTo(1), "Should have been invoked on observer");
            Assert.That(_observerHookInvoked[0], Is.EqualTo(VALUE_1));

            Assert.That(_ownerHookInvoked, Has.Count.EqualTo(0), "Should still not be invoked on owner");

            Assert.That(ServerComponent.FieldNone, Is.EqualTo(VALUE_1), "field should have been updated");
            Assert.That(OwnerComponent.FieldNone, Is.EqualTo(VALUE_1), "field should have been updated");
        }
    }
}
