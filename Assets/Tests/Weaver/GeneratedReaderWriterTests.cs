using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class GeneratedReaderWriterTests : TestsBuildFromTestName
    {
        [SetUp]
        public override void TestSetup()
        {
            base.TestSetup();
        }

        [Test]
        public void CreatesForStructs()
        {
            IsSuccess();
        }

        [Test]
        public void CreateForExplicitNetworkMessage()
        {
            IsSuccess();
        }

        [Test]
        public void CreatesForClass()
        {
            IsSuccess();
        }

        [Test]
        public void CreatesForClassInherited()
        {
            IsSuccess();
        }

        [Test]
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

        [Test]
        public void CreatesForInheritedFromScriptableObject()
        {
            IsSuccess();
        }

        [Test]
        public void CreatesForStructFromDifferentAssemblies()
        {
            IsSuccess();
        }

        [Test]
        public void CreatesForClassFromDifferentAssemblies()
        {
            IsSuccess();
        }

        [Test]
        public void CreatesForClassFromDifferentAssembliesWithValidConstructor()
        {
            IsSuccess();
        }

        [Test]
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
            HasError("Cannot generate read function for Object. Use a supported type or provide a custom read function",
                "UnityEngine.Object");
        }

        [Test]
        public void GivesErrorWhenUsingScriptableObject()
        {
            HasError("Cannot generate write function for ScriptableObject. Use a supported type or provide a custom write function",
                "UnityEngine.ScriptableObject");
            HasError("Cannot generate read function for ScriptableObject. Use a supported type or provide a custom read function",
                "UnityEngine.ScriptableObject");
        }

        [Test]
        public void GivesErrorWhenUsingMonoBehaviour()
        {
            HasError("Cannot generate write function for component type MonoBehaviour. Use a supported type or provide a custom write function",
                "UnityEngine.MonoBehaviour");
            HasError("Cannot generate read function for component type MonoBehaviour. Use a supported type or provide a custom read function",
                "UnityEngine.MonoBehaviour");
        }

        [Test]
        public void GivesErrorWhenUsingTypeInheritedFromMonoBehaviour()
        {
            HasError("Cannot generate write function for component type MyBehaviour. Use a supported type or provide a custom write function",
                "GeneratedReaderWriter.GivesErrorWhenUsingTypeInheritedFromMonoBehaviour.MyBehaviour");
            HasError("Cannot generate read function for component type MyBehaviour. Use a supported type or provide a custom read function",
                "GeneratedReaderWriter.GivesErrorWhenUsingTypeInheritedFromMonoBehaviour.MyBehaviour");
        }

        [Test]
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
            HasError("Cannot generate read function for interface IData. Use a supported type or provide a custom read function",
                "GeneratedReaderWriter.GivesErrorWhenUsingInterface.IData");
        }

        [Test]
        public void CanUseCustomReadWriteForInterfaces()
        {
            IsSuccess();
        }

        [Test]
        public void GivesErrorWhenUsingAbstractClass()
        {
            HasError("Cannot generate write function for abstract class DataBase. Use a supported type or provide a custom write function", "GeneratedReaderWriter.GivesErrorWhenUsingAbstractClass.DataBase");
            HasError("Cannot generate read function for abstract class DataBase. Use a supported type or provide a custom read function", "GeneratedReaderWriter.GivesErrorWhenUsingAbstractClass.DataBase");
        }

        [Test]
        public void CanUseCustomReadWriteForAbstractClass()
        {
            IsSuccess();
        }

        [Test]
        public void CreatesForEnums()
        {
            IsSuccess();
        }

        [Test]
        public void CreatesForArraySegment()
        {
            IsSuccess();
        }

        [Test]
        public void CreatesForStructArraySegment()
        {
            IsSuccess();
        }

        [Test]
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
            HasError("Cannot generate read function for component type MonoBehaviour. Use a supported type or provide a custom read function", "UnityEngine.MonoBehaviour");
        }

        [Test]
        public void GivesErrorForInvalidArraySegmentType()
        {
            HasError("Cannot generate write function for component type MonoBehaviour. Use a supported type or provide a custom write function", "UnityEngine.MonoBehaviour");
            HasError("Cannot generate read function for component type MonoBehaviour. Use a supported type or provide a custom read function", "UnityEngine.MonoBehaviour");
        }

        [Test]
        public void CreatesForList()
        {
            IsSuccess();
        }

        [Test]
        public void CreatesForStructList()
        {
            IsSuccess();
        }

        [Test]
        public void GivesErrorForInvalidListType()
        {
            HasError("Cannot generate write function for component type MonoBehaviour. Use a supported type or provide a custom write function", "UnityEngine.MonoBehaviour");
            HasError("Cannot generate read function for component type MonoBehaviour. Use a supported type or provide a custom read function", "UnityEngine.MonoBehaviour");
        }

        [Test]
        public void CreatesForNullable()
        {
            IsSuccess();
        }

        [Test]
        public void GivesWarningForMultipleMethodsForSameType()
        {
            NoErrors();

            HasWarning(
                $"Registering a write function for System.Int32 when one already exists\n" +
                $"  old:{1}\n" +
                $"  new:{2}", "");
            HasWarning(
               $"Registering a write function for System.Int32 when one already exists\n" +
               $"  old:{1}\n" +
               $"  new:{2}", "");

            HasWarning(
               $"Registering a read function for System.Int32 when one already exists\n" +
               $"  old:{1}\n" +
               $"  new:{2}", "");
            HasWarning(
               $"Registering a read function for System.Int32 when one already exists\n" +
               $"  old:{1}\n" +
               $"  new:{2}", "");
        }
    }
}
