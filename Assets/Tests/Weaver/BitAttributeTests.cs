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
        public void BitCountInvalid()
        {
            HasErrorCount(13);

            HasError("BitCount can not be above target type size, bitCount:9, type:Byte, max size:8",
                "System.Byte BitAttributeTests.BitCountInvalid.MyBehaviour::value1");

            HasError("BitCount can not be above target type size, bitCount:17, type:Int16, max size:16",
                "System.Int16 BitAttributeTests.BitCountInvalid.MyBehaviour::value2");

            HasError("BitCount can not be above target type size, bitCount:17, type:UInt16, max size:16",
                "System.UInt16 BitAttributeTests.BitCountInvalid.MyBehaviour::value3");

            HasError("BitCount can not be above target type size, bitCount:33, type:Int32, max size:32",
                "System.Int32 BitAttributeTests.BitCountInvalid.MyBehaviour::value4");

            HasError("BitCount can not be above target type size, bitCount:33, type:UInt32, max size:32",
                "System.UInt32 BitAttributeTests.BitCountInvalid.MyBehaviour::value5");

            HasError("BitCount can not be above target type size, bitCount:65, type:Int64, max size:64",
                "System.Int64 BitAttributeTests.BitCountInvalid.MyBehaviour::value6");

            HasError("BitCount can not be above target type size, bitCount:65, type:UInt64, max size:64",
                "System.UInt64 BitAttributeTests.BitCountInvalid.MyBehaviour::value7");

            HasError("BitCount can not be above target type size, bitCount:9, type:MyByteEnum, max size:8",
                "BitAttributeTests.BitCountInvalid.MyByteEnum BitAttributeTests.BitCountInvalid.MyBehaviour::value8");

            HasError("BitCount can not be above target type size, bitCount:17, type:MyShortEnum, max size:16",
                "BitAttributeTests.BitCountInvalid.MyShortEnum BitAttributeTests.BitCountInvalid.MyBehaviour::value9");

            HasError("BitCount can not be above target type size, bitCount:33, type:MyIntEnum, max size:32",
                "BitAttributeTests.BitCountInvalid.MyIntEnum BitAttributeTests.BitCountInvalid.MyBehaviour::value10");


            HasError("UnityEngine.Vector3 is not a supported type for [BitCount]",
                "UnityEngine.Vector3 BitAttributeTests.BitCountInvalid.MyBehaviour::value11");

            HasError("BitCount should be above 0",
                "System.Int32 BitAttributeTests.BitCountInvalid.MyBehaviour::value12");

            HasError("BitCount should be above 0",
                "System.Int32 BitAttributeTests.BitCountInvalid.MyBehaviour::value13");
        }

        [Test]
        public void ZigZag()
        {
            IsSuccess();
        }

        [Test]
        public void ZigZagInvalid()
        {
            HasErrorCount(3);

            HasError("[ZigZagEncode] can only be used with [BitCount]",
                "System.Int32 BitAttributeTests.ZigZagInvalid.MyBehaviour::value1");

            HasError("[ZigZagEncode] can only be used on a signed type",
                "System.UInt32 BitAttributeTests.ZigZagInvalid.MyBehaviour::value2");

            HasError("[ZigZagEncode] can only be used on a signed type",
                "BitAttributeTests.ZigZagInvalid.MyShortEnum BitAttributeTests.ZigZagInvalid.MyBehaviour::value3");
        }

        [Test]
        public void BitCountFromRange()
        {
            IsSuccess();
        }

        [Test]
        public void BitCountFromRangeInvalid()
        {
            HasErrorCount(4);

            HasError("[BitCountFromRange] max value must be greater than min value",
                "System.Int32 BitAttributeTests.BitCountFromRangeInvalid.MyBehaviour::value1");

            HasError("[BitCountFromRange] max value must be greater than min value",
                "System.Int32 BitAttributeTests.BitCountFromRangeInvalid.MyBehaviour::value2");

            HasError("[BitCountFromRange] can't be used with [ZigZagEncode]",
                "System.Int32 BitAttributeTests.BitCountFromRangeInvalid.MyBehaviour::value3");

            HasError("[BitCountFromRange] can't be used with [BitCount]",
                "System.Int32 BitAttributeTests.BitCountFromRangeInvalid.MyBehaviour::value4");
        }
    }
}
