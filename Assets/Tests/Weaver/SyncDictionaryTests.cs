using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    // Some tests for SyncObjects are in SyncListTests and apply to SyncDictionary too
    public class SyncDictionaryTests : WeaverTestBase
    {
        [Test, BatchSafe]
        public void SyncDictionary()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncDictionaryGenericAbstractInheritance()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncDictionaryGenericInheritance()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncDictionaryInheritance()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncDictionaryStructKey()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncDictionaryStructItem()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncDictionaryErrorForGenericStructKey()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncDictionaryErrorForGenericStructItem()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncDictionaryErrorWhenUsingGenericInNetworkBehaviour()
        {
            IsSuccess();
        }
    }
}
