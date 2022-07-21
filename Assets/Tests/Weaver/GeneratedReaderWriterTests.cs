using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class GeneratedReaderWriterTests : WeaverTestBase
    {
        [Test, BatchSafe(BatchType.Success)]
        public void CreatesForStructs()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CreateForExplicitNetworkMessage()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CreatesForClass()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CreatesForClassInherited()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CreatesForClassWithValidConstructor()
        {
            IsSuccess();
        }

        [Test]
        public void GivesErrorForClassWithNoValidConstructor()
        {
            HasError("SomeOtherData can't be deserialized because it has no default constructor",
                "GeneratedReaderWriter.GivesErrorForClassWithNoValidConstructor.SomeOtherData");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CreatesForInheritedFromScriptableObject()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CreatesForStructFromDifferentAssemblies()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CreatesForClassFromDifferentAssemblies()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CreatesForClassFromDifferentAssembliesWithValidConstructor()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CanUseCustomReadWriteForTypesFromDifferentAssemblies()
        {
            IsSuccess();
        }

        [Test]
        public void GivesErrorWhenUsingUnityAsset()
        {
            HasError("Material can't be deserialized because it has no default constructor",
                "UnityEngine.Material");
        }

        [Test]
        public void GivesErrorWhenUsingObject()
        {
            HasError("Cannot generate write function for Object. Use a supported type or provide a custom write function",
                "UnityEngine.Object");
        }

        [Test]
        public void GivesErrorWhenUsingScriptableObject()
        {
            HasError("Cannot generate write function for ScriptableObject. Use a supported type or provide a custom write function",
                "UnityEngine.ScriptableObject");
        }

        [Test]
        public void GivesErrorWhenUsingMonoBehaviour()
        {
            HasError("Cannot generate write function for component type MonoBehaviour. Use a supported type or provide a custom write function",
                "UnityEngine.MonoBehaviour");
        }

        [Test]
        public void GivesErrorWhenUsingTypeInheritedFromMonoBehaviour()
        {
            HasError("Cannot generate write function for component type MyBehaviour. Use a supported type or provide a custom write function",
                "GeneratedReaderWriter.GivesErrorWhenUsingTypeInheritedFromMonoBehaviour.MyBehaviour");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void ExcludesNonSerializedFields()
        {
            // we test this by having a not allowed type in the class, but mark it with NonSerialized
            IsSuccess();
        }

        [Test]
        public void GivesErrorWhenUsingInterface()
        {
            HasError("Cannot generate write function for interface IData. Use a supported type or provide a custom write function",
                "GeneratedReaderWriter.GivesErrorWhenUsingInterface.IData");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CanUseCustomReadWriteForInterfaces()
        {
            IsSuccess();
        }

        [Test]
        public void GivesErrorWhenUsingAbstractClass()
        {
            HasError("Cannot generate write function for abstract class DataBase. Use a supported type or provide a custom write function", "GeneratedReaderWriter.GivesErrorWhenUsingAbstractClass.DataBase");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CanUseCustomReadWriteForAbstractClass()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CanUseCustomReadWriteForAbstractClassUsedInMessage()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CreatesForEnums()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CreatesForArraySegment()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CreatesForStructArraySegment()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void GivesErrorForJaggedArray()
        {
            IsSuccess();
        }

        [Test]
        public void GivesErrorForMultidimensionalArray()
        {
            HasError("Int32[0...,0...] is an unsupported type. Multidimensional arrays are not supported",
                "System.Int32[0...,0...]");
        }

        [Test]
        public void GivesErrorForInvalidArrayType()
        {
            HasError("Cannot generate write function for component type MonoBehaviour. Use a supported type or provide a custom write function", "UnityEngine.MonoBehaviour");
        }

        [Test]
        public void GivesErrorForInvalidArraySegmentType()
        {
            HasError("Cannot generate write function for component type MonoBehaviour. Use a supported type or provide a custom write function", "UnityEngine.MonoBehaviour");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CreatesForList()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CreatesForStructList()
        {
            IsSuccess();
        }

        [Test]
        public void GivesErrorForInvalidListType()
        {
            HasError("Cannot generate write function for component type MonoBehaviour. Use a supported type or provide a custom write function", "UnityEngine.MonoBehaviour");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CreatesForNullable()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void CanUseStringInMessage()
        {
            IsSuccess();
        }
    }
}
