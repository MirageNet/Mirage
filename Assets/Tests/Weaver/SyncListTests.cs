using NUnit.Framework;

namespace Mirage.Weaver
{
    public class SyncListTests : TestsBuildFromTestName
    {
        [Test]
        public void SyncList()
        {
            IsSuccess();
        }

        [Test]
        public void SyncListByteValid()
        {
            IsSuccess();
        }

        [Test]
        public void SyncListGenericAbstractInheritance()
        {
            IsSuccess();
        }

        [Test]
        public void SyncListGenericInheritance()
        {
            IsSuccess();
        }

        [Test]
        public void SyncListGenericInheritanceWithMultipleGeneric()
        {
            IsSuccess();
        }

        [Test]
        public void SyncListInheritance()
        {
            IsSuccess();
        }

        [Test]
        public void SyncListNestedStruct()
        {
            IsSuccess();
        }

        [Test]
        public void SyncListNestedInAbstractClass()
        {
            IsSuccess();
        }

        [Test]
        public void SyncListNestedInAbstractClassWithInvalid()
        {
            HasError("Cannot generate reader for Object. Use a supported type or provide a custom reader",
                "UnityEngine.Object");
            HasError("target has an unsupported type",
                "UnityEngine.Object SyncListTests.SyncListNestedInAbstractClassWithInvalid.SyncListNestedStructWithInvalid/SomeAbstractClass/MyNestedStruct::target");
            HasError("Cannot generate writer for Object. Use a supported type or provide a custom writer",
                "UnityEngine.Object");
        }

        [Test]
        public void SyncListNestedInStruct()
        {
            IsSuccess();
        }

        [Test]
        public void SyncListNestedInStructWithInvalid()
        {
            HasError("Cannot generate reader for Object. Use a supported type or provide a custom reader",
                "UnityEngine.Object");
            HasError("target has an unsupported type",
                "UnityEngine.Object SyncListTests.SyncListNestedInStructWithInvalid.SyncListNestedInStructWithInvalid/SomeData::target");
            HasError("Cannot generate writer for Object. Use a supported type or provide a custom writer",
                "UnityEngine.Object");
        }

        [Test]
        public void SyncListStruct()
        {
            IsSuccess();
        }

        [Test]
        public void SyncListErrorForGenericStruct()
        {
            HasError("Cannot generate reader for generic variable MyGenericStruct`1. Use a supported type or provide a custom reader",
                "SyncListTests.SyncListErrorForGenericStruct.SyncListErrorForGenericStruct/MyGenericStruct`1<System.Single>");
            HasError("Cannot generate writer for generic type MyGenericStruct`1. Use a supported type or provide a custom writer",
                "SyncListTests.SyncListErrorForGenericStruct.SyncListErrorForGenericStruct/MyGenericStruct`1<System.Single>");
        }

        [Test]
        public void SyncListErrorForInterface()
        {
            HasError("Cannot generate reader for interface MyInterface. Use a supported type or provide a custom reader",
                "SyncListTests.SyncListErrorForInterface.MyInterface");
            HasError("Cannot generate writer for interface MyInterface. Use a supported type or provide a custom writer",
                "SyncListTests.SyncListErrorForInterface.MyInterface");
        }

        [Test]
        public void SyncListErrorWhenUsingGenericListInNetworkBehaviour()
        {
            IsSuccess();
        }
    }
}
