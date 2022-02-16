using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    // Some tests for SyncObjects are in SyncListTests and apply to SyncDictionary too
    public class SyncDictionaryTests : TestsBuildFromTestName
    {
        [Test]
        public void SyncDictionary()
        {
            IsSuccess();
        }

        [Test]
        public void SyncDictionaryGenericAbstractInheritance()
        {
            IsSuccess();
        }

        [Test]
        public void SyncDictionaryGenericInheritance()
        {
            IsSuccess();
        }

        [Test]
        public void SyncDictionaryInheritance()
        {
            IsSuccess();
        }

        [Test]
        public void SyncDictionaryStructKey()
        {
            IsSuccess();
        }

        [Test]
        public void SyncDictionaryStructItem()
        {
            IsSuccess();
        }

        [Test]
        public void SyncDictionaryErrorForGenericStructKey()
        {
            IsSuccess();
        }

        [Test]
        public void SyncDictionaryErrorForGenericStructItem()
        {
            IsSuccess();
        }

        [Test]
        public void SyncDictionaryErrorWhenUsingGenericInNetworkBehaviour()
        {
            IsSuccess();
        }
    }
}
