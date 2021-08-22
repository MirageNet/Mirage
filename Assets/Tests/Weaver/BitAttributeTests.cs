using NUnit.Framework;

namespace Mirage.Weaver
{
    public class BitAttributeTests : TestsBuildFromTestName
    {
        [Test]
        public void BitCount()
        {
            IsSuccess();
        }

        [Test]
        public void BitCountOverTypeSize()
        {
            HasErrorCount(13);

            HasError("BitCount can not be above target type size, bitCount:9, type:Byte, max size:8",
                "System.Byte BitAttributeTests.BitCountOverTypeSize.MyBehaviour::value1");

            HasError("BitCount can not be above target type size, bitCount:17, type:Int16, max size:16",
                "System.Int16 BitAttributeTests.BitCountOverTypeSize.MyBehaviour::value2");

            HasError("BitCount can not be above target type size, bitCount:17, type:UInt16, max size:16",
                "System.UInt16 BitAttributeTests.BitCountOverTypeSize.MyBehaviour::value3");

            HasError("BitCount can not be above target type size, bitCount:33, type:Int32, max size:32",
                "System.Int32 BitAttributeTests.BitCountOverTypeSize.MyBehaviour::value4");

            HasError("BitCount can not be above target type size, bitCount:33, type:UInt32, max size:32",
                "System.UInt32 BitAttributeTests.BitCountOverTypeSize.MyBehaviour::value5");

            HasError("BitCount can not be above target type size, bitCount:65, type:Int64, max size:64",
                "System.Int64 BitAttributeTests.BitCountOverTypeSize.MyBehaviour::value6");

            HasError("BitCount can not be above target type size, bitCount:65, type:UInt64, max size:64",
                "System.UInt64 BitAttributeTests.BitCountOverTypeSize.MyBehaviour::value7");

            HasError("BitCount can not be above target type size, bitCount:9, type:MyByteEnum, max size:8",
                "BitAttributeTests.BitCountOverTypeSize.MyByteEnum BitAttributeTests.BitCountOverTypeSize.MyBehaviour::value8");

            HasError("BitCount can not be above target type size, bitCount:17, type:MyShortEnum, max size:16",
                "BitAttributeTests.BitCountOverTypeSize.MyShortEnum BitAttributeTests.BitCountOverTypeSize.MyBehaviour::value9");

            HasError("BitCount can not be above target type size, bitCount:33, type:MyIntEnum, max size:32",
                "BitAttributeTests.BitCountOverTypeSize.MyIntEnum BitAttributeTests.BitCountOverTypeSize.MyBehaviour::value10");

            // todo add errors for other fields

            HasError("UnityEngine.Vector3 is not a supported type for [BitCount]",
                "UnityEngine.Vector3 BitAttributeTests.BitCountOverTypeSize.MyBehaviour::value11");
            HasError("BitCount should be above 0",
                "System.Int32 BitAttributeTests.BitCountOverTypeSize.MyBehaviour::value12");
            HasError("BitCount should be above 0",
                "System.Int32 BitAttributeTests.BitCountOverTypeSize.MyBehaviour::value13");

        }
    }
}
