using NUnit.Framework;

namespace Mirage.Weaver
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
            HasError("Cannot generate read function for generic type MyGenericStruct`1. Use a supported type or provide a custom read function",
                "SyncDictionaryTests.SyncDictionaryErrorForGenericStructKey.SyncDictionaryErrorForGenericStructKey/MyGenericStruct`1<System.Single>");
            HasError("Cannot generate write function for generic type MyGenericStruct`1. Use a supported type or provide a custom write function",
                "SyncDictionaryTests.SyncDictionaryErrorForGenericStructKey.SyncDictionaryErrorForGenericStructKey/MyGenericStruct`1<System.Single>");

        }

        [Test]
        public void SyncDictionaryErrorForGenericStructItem()
        {
            HasError("Cannot generate write function for generic type MyGenericStruct`1. Use a supported type or provide a custom write function",
                "SyncDictionaryTests.SyncDictionaryErrorForGenericStructItem.SyncDictionaryErrorForGenericStructItem/MyGenericStruct`1<System.Single>");
            HasError("Cannot generate read function for generic type MyGenericStruct`1. Use a supported type or provide a custom read function",
                "SyncDictionaryTests.SyncDictionaryErrorForGenericStructItem.SyncDictionaryErrorForGenericStructItem/MyGenericStruct`1<System.Single>");
        }

        [Test]
        public void SyncDictionaryErrorWhenUsingGenericInNetworkBehaviour()
        {
            IsSuccess();
        }
    }
}
