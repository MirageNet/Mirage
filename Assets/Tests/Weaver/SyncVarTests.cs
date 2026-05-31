using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class SyncVarTests : WeaverTestBase
    {
        [Test, BatchSafe(BatchType.Success)]
        public void SyncVarsValid()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SyncVarsValidInitialOnly()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SyncVarArraySegment()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
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

        [Test, BatchSafe(BatchType.Success)]
        public void SyncVarsGenericField()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
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

        [Test]
        public void SyncVarClassWarning()
        {
            NoErrors();
            HasWarning("warnedVar is a class. SyncVars that are classes can allocate and are difficult to track changes for. Consider using a struct instead, or mark the class or property with [WeaverSafeClass] if it is custom serialized.",
                "SyncVarTests.SyncVarClassWarning.SomeCustomClass SyncVarTests.SyncVarClassWarning.SyncVarClassWarning::warnedVar");
        }

        [Test]
        public void RpcAndMessageClassWarnings()
        {
            NoErrors();
            HasWarning("UnsafeField is a class. Serializing classes in messages can allocate and are difficult to track changes for. Consider using a struct instead, or mark the class or field with [WeaverSafeClass] if it is custom serialized.",
                "SyncVarTests.RpcAndMessageClassWarnings.UnsafeClass SyncVarTests.RpcAndMessageClassWarnings.TestMessage::UnsafeField");

            HasWarning("unsafeParam is a class. RPC parameters that are classes can allocate. Consider using a struct instead, or mark the class or parameter with [WeaverSafeClass] if it is custom serialized.",
                "System.Void SyncVarTests.RpcAndMessageClassWarnings.RpcAndMessageClassWarnings::SendUnsafeRpc(SyncVarTests.RpcAndMessageClassWarnings.UnsafeClass)");

            HasWarning("Return type UniTask<UnsafeClass> is a class. RPC return values that are classes can allocate. Consider using a struct instead, or mark the class or method with [WeaverSafeClass] if it is custom serialized.",
                "Cysharp.Threading.Tasks.UniTask`1<SyncVarTests.RpcAndMessageClassWarnings.UnsafeClass> SyncVarTests.RpcAndMessageClassWarnings.RpcAndMessageClassWarnings::UnsafeReturnRpc()");
        }
    }
}
