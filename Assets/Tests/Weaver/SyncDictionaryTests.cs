using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    // Some tests for SyncObjects are in SyncListTests and apply to SyncDictionary too
    public class SyncDictionaryTests : WeaverTestBase
    {
        [Test, BatchSafe(BatchType.Success)]
        public void SyncDictionary()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SyncDictionaryGenericAbstractInheritance()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SyncDictionaryGenericInheritance()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SyncDictionaryInheritance()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SyncDictionaryStructKey()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SyncDictionaryStructItem()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SyncDictionaryErrorForGenericStructKey()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SyncDictionaryErrorForGenericStructItem()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SyncDictionaryErrorWhenUsingGenericInNetworkBehaviour()
        {
            IsSuccess();
        }

        [Test]
        public void SyncDictionaryErrorWhenNotInitialized()
        {
            HasError("SyncObject Foo must be initialized. Please assign a value where the field is declared or in the constructor.",
                "Mirage.Collections.SyncDictionary`2<System.Int32,System.String> SyncDictionaryTests.SyncDictionaryErrorWhenNotInitialized.SyncDictionaryBehaviour::Foo");
        }
    }
}
