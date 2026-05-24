using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    // Some tests for SyncObjects are in SyncListTests and apply to SyncDictionary too
    public class SyncSetTests : WeaverTestBase
    {
        [Test, BatchSafe(BatchType.Success)]
        public void SyncSetByteValid()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SyncSetGenericAbstractInheritance()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SyncSetGenericInheritance()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SyncSetInheritance()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SyncSetStruct()
        {
            IsSuccess();
        }

        [Test]
        public void SyncSetErrorWhenNotInitialized()
        {
            HasError("SyncObject Foo must be initialized. Please assign a value where the field is declared or in the constructor.",
                "Mirage.Collections.SyncHashSet`1<System.Int32> SyncSetTests.SyncSetErrorWhenNotInitialized.SyncSetBehaviour::Foo");
        }
    }
}
