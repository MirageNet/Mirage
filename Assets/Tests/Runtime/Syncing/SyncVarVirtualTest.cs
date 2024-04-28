using System;
using Mirage.Serialization;
using Mirage.Tests.Runtime.ClientServer;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime.Syncing
{
    public abstract class SyncVarHookTesterBase : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnValue1Changed))]
        public float value1;
        [SyncVar(hook = nameof(OnValue2Changed))]
        public float value2;

        public event Action OnValue2ChangedVirtualCalled;

        public abstract void OnValue1Changed(float old, float newValue);
        public virtual void OnValue2Changed(float old, float newValue)
        {
            OnValue2ChangedVirtualCalled?.Invoke();
        }

        public void ChangeValues()
        {
            value1 += 1f;
            value2 += 1f;
        }

        public void CallOnValue2Changed()
        {
            OnValue2Changed(1, 1);
        }
    }

    public class SyncVarHookTester : SyncVarHookTesterBase
    {
        public event Action OnValue1ChangedOverrideCalled;
        public event Action OnValue2ChangedOverrideCalled;
        public override void OnValue1Changed(float old, float newValue)
        {
            OnValue1ChangedOverrideCalled?.Invoke();
        }
        public override void OnValue2Changed(float old, float newValue)
        {
            OnValue2ChangedOverrideCalled?.Invoke();
        }
    }

    [TestFixture]
    public class SyncVarVirtualTest : ClientServerSetup<SyncVarHookTester>
    {
        private readonly NetworkWriter ownerWriter = new NetworkWriter(1300);
        private readonly NetworkWriter observersWriter = new NetworkWriter(1300);
        private readonly NetworkReader reader = new NetworkReader();

        [SetUp]
        public void Setup()
        {
            serverComponent.value1 = 1;
            serverComponent.value2 = 2;

            SyncValuesWithClient();
        }

        [TearDown]
        public void TearDown()
        {
            TearDownTestObjects();

            ownerWriter.Reset();
            observersWriter.Reset();
            reader.Dispose();
        }

        private void SyncValuesWithClient()
        {
            ownerWriter.Reset();
            observersWriter.Reset();

            // make sure it is time to sync
            serverComponent._nextSyncTime = Time.timeAsDouble;
            serverIdentity.OnSerializeAll(true, ownerWriter, observersWriter);

            // apply all the data from the server object

            reader.Reset(ownerWriter.ToArraySegment());
            clientIdentity.OnDeserializeAll(reader, true);
        }


        [Test]
        public void AbstractMethodOnChangeWorkWithHooks()
        {
            serverComponent.ChangeValues();

            var value1OverrideCalled = false;
            clientComponent.OnValue1ChangedOverrideCalled += () =>
            {
                value1OverrideCalled = true;
            };

            SyncValuesWithClient();

            Assert.AreEqual(serverComponent.value1, serverComponent.value1);
            Assert.IsTrue(value1OverrideCalled);
        }
        [Test]
        public void VirtualMethodOnChangeWorkWithHooks()
        {
            serverComponent.ChangeValues();

            var value2OverrideCalled = false;
            clientComponent.OnValue2ChangedOverrideCalled += () =>
            {
                value2OverrideCalled = true;
            };

            var value2VirtualCalled = false;
            clientComponent.OnValue2ChangedVirtualCalled += () =>
            {
                value2VirtualCalled = true;
            };

            SyncValuesWithClient();

            Assert.AreEqual(serverComponent.value2, serverComponent.value2);
            Assert.IsTrue(value2OverrideCalled, "Override method not called");
            Assert.IsFalse(value2VirtualCalled, "Virtual method called when Override exists");
        }

        [Test]
        public void ManuallyCallingVirtualMethodCallsOverride()
        {
            // this to check that class are set up correct for tests above
            serverComponent.ChangeValues();

            var value2OverrideCalled = false;
            clientComponent.OnValue2ChangedOverrideCalled += () =>
            {
                value2OverrideCalled = true;
            };

            var value2VirtualCalled = false;
            clientComponent.OnValue2ChangedVirtualCalled += () =>
            {
                value2VirtualCalled = true;
            };

            var baseClass = clientComponent as SyncVarHookTesterBase;
            baseClass.OnValue2Changed(1, 1);

            Assert.AreEqual(serverComponent.value2, serverComponent.value2);
            Assert.IsTrue(value2OverrideCalled, "Override method not called");
            Assert.IsFalse(value2VirtualCalled, "Virtual method called when Override exists");
        }
        [Test]
        public void ManuallyCallingVirtualMethodInsideBaseClassCallsOverride()
        {
            // this to check that class are set up correct for tests above
            serverComponent.ChangeValues();

            var value2OverrideCalled = false;
            clientComponent.OnValue2ChangedOverrideCalled += () =>
            {
                value2OverrideCalled = true;
            };

            var value2VirtualCalled = false;
            clientComponent.OnValue2ChangedVirtualCalled += () =>
            {
                value2VirtualCalled = true;
            };

            var baseClass = clientComponent as SyncVarHookTesterBase;
            baseClass.CallOnValue2Changed();

            Assert.AreEqual(serverComponent.value2, serverComponent.value2);
            Assert.IsTrue(value2OverrideCalled, "Override method not called");
            Assert.IsFalse(value2VirtualCalled, "Virtual method called when Override exists");
        }
    }
}
