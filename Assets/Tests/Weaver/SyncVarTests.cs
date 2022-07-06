using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class SyncVarTests : WeaverTestBase
    {
        [Test, BatchSafe]
        public void SyncVarsValid()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncVarsValidInitialOnly()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncVarArraySegment()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncVarsDerivedNetworkBehaviour()
        {
            IsSuccess();
        }

        [Test]
        public void SyncVarsStatic()
        {
            HasError("invalidVar cannot be static",
                "System.Int32 SyncVarTests.SyncVarsStatic.SyncVarsStatic::invalidVar");
        }

        [Test, BatchSafe]
        public void SyncVarsGenericField()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncVarsGenericParam()
        {
            IsSuccess();
        }

        [Test]
        public void SyncVarsInterface()
        {
            HasError("Cannot generate write function for interface IMySyncVar. Use a supported type or provide a custom write function",
                "SyncVarTests.SyncVarsInterface.SyncVarsInterface/IMySyncVar SyncVarTests.SyncVarsInterface.SyncVarsInterface::invalidVar");
        }

        [Test]
        public void SyncVarsUnityComponent()
        {
            HasError("Cannot generate write function for component type TextMesh. Use a supported type or provide a custom write function",
                "UnityEngine.TextMesh SyncVarTests.SyncVarsUnityComponent.SyncVarsUnityComponent::invalidVar");
        }

        [Test]
        public void SyncVarsCantBeArray()
        {
            HasError("thisShouldntWork has invalid type. Use SyncLists instead of arrays",
                "System.Int32[] SyncVarTests.SyncVarsCantBeArray.SyncVarsCantBeArray::thisShouldntWork");
        }

        [Test]
        public void SyncVarsSyncList()
        {
            HasError("syncints has [SyncVar] attribute. ISyncObject should not be marked with SyncVar",
                "Mirage.Collections.SyncList`1<System.Int32> SyncVarTests.SyncVarsSyncList.SyncVarsSyncList::syncints");
        }

        [Test]
        public void SyncVarsMoreThan63()
        {
            HasError("SyncVarsMoreThan63 has too many [SyncVar]. Consider refactoring your class into multiple components",
                "SyncVarTests.SyncVarsMoreThan63.SyncVarsMoreThan63");
        }
    }
}
