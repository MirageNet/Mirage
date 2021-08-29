using NUnit.Framework;

namespace Mirage.Tests.Weaver
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
            HasError("Cannot generate write function for Object. Use a supported type or provide a custom write function",
                "UnityEngine.Object");

            HasError("Cannot generate read function for Object. Use a supported type or provide a custom read function",
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
            HasError("Cannot generate write function for Object. Use a supported type or provide a custom write function",
                "UnityEngine.Object");

            HasError("Cannot generate read function for Object. Use a supported type or provide a custom read function",
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
            HasError("Cannot generate write function for generic type MyGenericStruct`1. Use a supported type or provide a custom write function",
                "SyncListTests.SyncListErrorForGenericStruct.SyncListErrorForGenericStruct/MyGenericStruct`1<System.Single>");
            HasError("Cannot generate read function for generic type MyGenericStruct`1. Use a supported type or provide a custom read function",
                "SyncListTests.SyncListErrorForGenericStruct.SyncListErrorForGenericStruct/MyGenericStruct`1<System.Single>");
        }

        [Test]
        public void SyncListErrorForInterface()
        {
            HasError("Cannot generate read function for interface MyInterface. Use a supported type or provide a custom read function",
                "SyncListTests.SyncListErrorForInterface.MyInterface");
            HasError("Cannot generate write function for interface MyInterface. Use a supported type or provide a custom write function",
                "SyncListTests.SyncListErrorForInterface.MyInterface");
        }

        [Test]
        public void SyncListErrorWhenUsingGenericListInNetworkBehaviour()
        {
            IsSuccess();
        }
    }
}
