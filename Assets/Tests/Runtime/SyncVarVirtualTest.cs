using System;
using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime
{
    abstract class SyncVarHookTesterBase : NetworkBehaviour
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

    class SyncVarHookTester : SyncVarHookTesterBase
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
    public class SyncVarVirtualTest : TestBase
    {
        private SyncVarHookTester serverTester;
        private NetworkIdentity serverIdentity;

        private SyncVarHookTester clientTester;
        private NetworkIdentity clientIdentity;

        readonly NetworkWriter ownerWriter = new NetworkWriter(1300);
        readonly NetworkWriter observersWriter = new NetworkWriter(1300);
        readonly NetworkReader reader = new NetworkReader();

        [SetUp]
        public void Setup()
        {
            serverTester = CreateBehaviour<SyncVarHookTester>();
            serverIdentity = serverTester.Identity;
            clientTester = CreateBehaviour<SyncVarHookTester>();
            clientIdentity = clientTester.Identity;

            serverTester.value1 = 1;
            serverTester.value2 = 2;

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

            serverIdentity.OnSerializeAll(true, ownerWriter, observersWriter);

            // apply all the data from the server object

            reader.Reset(ownerWriter.ToArraySegment());
            clientIdentity.OnDeserializeAll(reader, true);
        }


        [Test]
        public void AbstractMethodOnChangeWorkWithHooks()
        {
            serverTester.ChangeValues();

            bool value1OverrideCalled = false;
            clientTester.OnValue1ChangedOverrideCalled += () =>
            {
                value1OverrideCalled = true;
            };

            SyncValuesWithClient();

            Assert.AreEqual(serverTester.value1, serverTester.value1);
            Assert.IsTrue(value1OverrideCalled);
        }
        [Test]
        public void VirtualMethodOnChangeWorkWithHooks()
        {
            serverTester.ChangeValues();

            bool value2OverrideCalled = false;
            clientTester.OnValue2ChangedOverrideCalled += () =>
            {
                value2OverrideCalled = true;
            };

            bool value2VirtualCalled = false;
            clientTester.OnValue2ChangedVirtualCalled += () =>
            {
                value2VirtualCalled = true;
            };

            SyncValuesWithClient();

            Assert.AreEqual(serverTester.value2, serverTester.value2);
            Assert.IsTrue(value2OverrideCalled, "Override method not called");
            Assert.IsFalse(value2VirtualCalled, "Virtual method called when Override exists");
        }

        [Test]
        public void ManuallyCallingVirtualMethodCallsOverride()
        {
            // this to check that class are set up correct for tests above
            serverTester.ChangeValues();

            bool value2OverrideCalled = false;
            clientTester.OnValue2ChangedOverrideCalled += () =>
            {
                value2OverrideCalled = true;
            };

            bool value2VirtualCalled = false;
            clientTester.OnValue2ChangedVirtualCalled += () =>
            {
                value2VirtualCalled = true;
            };

            var baseClass = clientTester as SyncVarHookTesterBase;
            baseClass.OnValue2Changed(1, 1);

            Assert.AreEqual(serverTester.value2, serverTester.value2);
            Assert.IsTrue(value2OverrideCalled, "Override method not called");
            Assert.IsFalse(value2VirtualCalled, "Virtual method called when Override exists");
        }
        [Test]
        public void ManuallyCallingVirtualMethodInsideBaseClassCallsOverride()
        {
            // this to check that class are set up correct for tests above
            serverTester.ChangeValues();

            bool value2OverrideCalled = false;
            clientTester.OnValue2ChangedOverrideCalled += () =>
            {
                value2OverrideCalled = true;
            };

            bool value2VirtualCalled = false;
            clientTester.OnValue2ChangedVirtualCalled += () =>
            {
                value2VirtualCalled = true;
            };

            var baseClass = clientTester as SyncVarHookTesterBase;
            baseClass.CallOnValue2Changed();

            Assert.AreEqual(serverTester.value2, serverTester.value2);
            Assert.IsTrue(value2OverrideCalled, "Override method not called");
            Assert.IsFalse(value2VirtualCalled, "Virtual method called when Override exists");
        }
    }
}
