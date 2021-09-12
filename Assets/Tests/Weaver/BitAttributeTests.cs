using NUnit.Framework;

namespace Mirage.Tests.Weaver
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

            HasError("BitCount can not be above target type size, bitCount:9, max size:8, type:Byte",
                "System.Byte BitAttributeTests.BitCountInvalid.MyBehaviour::value1");

            HasError("BitCount can not be above target type size, bitCount:17, max size:16, type:Int16",
                "System.Int16 BitAttributeTests.BitCountInvalid.MyBehaviour::value2");

            HasError("BitCount can not be above target type size, bitCount:17, max size:16, type:UInt16",
                "System.UInt16 BitAttributeTests.BitCountInvalid.MyBehaviour::value3");

            HasError("BitCount can not be above target type size, bitCount:33, max size:32, type:Int32",
                "System.Int32 BitAttributeTests.BitCountInvalid.MyBehaviour::value4");

            HasError("BitCount can not be above target type size, bitCount:33, max size:32, type:UInt32",
                "System.UInt32 BitAttributeTests.BitCountInvalid.MyBehaviour::value5");

            HasError("BitCount can not be above target type size, bitCount:65, max size:64, type:Int64",
                "System.Int64 BitAttributeTests.BitCountInvalid.MyBehaviour::value6");

            HasError("BitCount can not be above target type size, bitCount:65, max size:64, type:UInt64",
                "System.UInt64 BitAttributeTests.BitCountInvalid.MyBehaviour::value7");

            HasError("BitCount can not be above target type size, bitCount:9, max size:8, type:MyByteEnum",
                "BitAttributeTests.BitCountInvalid.MyByteEnum BitAttributeTests.BitCountInvalid.MyBehaviour::value8");

            HasError("BitCount can not be above target type size, bitCount:17, max size:16, type:MyShortEnum",
                "BitAttributeTests.BitCountInvalid.MyShortEnum BitAttributeTests.BitCountInvalid.MyBehaviour::value9");

            HasError("BitCount can not be above target type size, bitCount:33, max size:32, type:MyIntEnum",
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
            HasErrorCount(8);

            HasError("Max must be greater than min",
                "System.Int32 BitAttributeTests.BitCountFromRangeInvalid.MyBehaviour::value1");

            HasError("Max must be greater than min",
                "System.Int32 BitAttributeTests.BitCountFromRangeInvalid.MyBehaviour::value2");

            HasError("[BitCountFromRange] can't be used with [BitCount], [VarInt] or [VarIntBlocks]",
                "System.Int32 BitAttributeTests.BitCountFromRangeInvalid.MyBehaviour::value3");

            HasError($"Max must be greater than types max value, max:{300}, max allowed:{byte.MaxValue}, type:Byte",
                "System.Byte BitAttributeTests.BitCountFromRangeInvalid.MyBehaviour::value4");

            HasError($"Max must be greater than types max value, max:{int.MaxValue}, max allowed:{short.MaxValue}, type:Int16",
                "System.Int16 BitAttributeTests.BitCountFromRangeInvalid.MyBehaviour::value5");

            HasError($"Min must be less than types min value, min:{-50}, min allowed:{uint.MinValue}, type:UInt32",
                "System.UInt32 BitAttributeTests.BitCountFromRangeInvalid.MyBehaviour::value6");

            HasError("UnityEngine.Vector3 is not a supported type for [BitCountFromRange]",
               "UnityEngine.Vector3 BitAttributeTests.BitCountFromRangeInvalid.MyBehaviour::value7");

            HasError("System.Int64 is not a supported type for [BitCountFromRange]",
               "System.Int64 BitAttributeTests.BitCountFromRangeInvalid.MyBehaviour::value8");
        }

        [Test]
        public void FloatPack()
        {
            IsSuccess();
        }

        [Test]
        public void FloatPackInvalid()
        {
            HasErrorCount(9);

            HasError("System.Double is not a supported type for [FloatPack]",
                "System.Double BitAttributeTests.FloatPackInvalid.MyBehaviour::value1");

            HasError("System.Int32 is not a supported type for [FloatPack]",
                "System.Int32 BitAttributeTests.FloatPackInvalid.MyBehaviour::value2");

            HasError("UnityEngine.Vector3 is not a supported type for [FloatPack]",
                "UnityEngine.Vector3 BitAttributeTests.FloatPackInvalid.MyBehaviour::value3");

            HasError("BitCount must be between 1 and 30 (inclusive), bitCount:31",
                "System.Single BitAttributeTests.FloatPackInvalid.MyBehaviour::value4");
            HasError("BitCount must be between 1 and 30 (inclusive), bitCount:0",
                "System.Single BitAttributeTests.FloatPackInvalid.MyBehaviour::value5");

            HasError("Max must be above 0, max:0",
                "System.Single BitAttributeTests.FloatPackInvalid.MyBehaviour::value6");
            HasError("Max must be above 0, max:-5",
                "System.Single BitAttributeTests.FloatPackInvalid.MyBehaviour::value7");

            HasError($"Precsion is too small, precision:{float.Epsilon}",
                "System.Single BitAttributeTests.FloatPackInvalid.MyBehaviour::value8");

            HasError("Precsion must be positive, precision:-0.1",
                "System.Single BitAttributeTests.FloatPackInvalid.MyBehaviour::value9");
        }

        [Test]
        public void Vector3Pack()
        {
            IsSuccess();
        }

        [Test]
        public void Vector3PackInvalid()
        {
            HasErrorCount(11);

            HasError("System.Single is not a supported type for [Vector3Pack]",
               "System.Single BitAttributeTests.Vector3PackInvalid.MyBehaviour::value1");
            HasError("UnityEngine.Vector2 is not a supported type for [Vector3Pack]",
               "UnityEngine.Vector2 BitAttributeTests.Vector3PackInvalid.MyBehaviour::value2");

            HasError("BitCount must be between 1 and 30 (inclusive), bitCount:31",
               "UnityEngine.Vector3 BitAttributeTests.Vector3PackInvalid.MyBehaviour::value3");
            HasError("BitCount must be between 1 and 30 (inclusive), bitCount:31",
               "UnityEngine.Vector3 BitAttributeTests.Vector3PackInvalid.MyBehaviour::value4");

            HasError("BitCount must be between 1 and 30 (inclusive), bitCount:0",
               "UnityEngine.Vector3 BitAttributeTests.Vector3PackInvalid.MyBehaviour::value5");
            HasError("BitCount must be between 1 and 30 (inclusive), bitCount:0",
               "UnityEngine.Vector3 BitAttributeTests.Vector3PackInvalid.MyBehaviour::value6");
            HasError("BitCount must be between 1 and 30 (inclusive), bitCount:0",
               "UnityEngine.Vector3 BitAttributeTests.Vector3PackInvalid.MyBehaviour::value7");

            HasError("Max must be above 0, max:(-1.0, 0.0, 0.0)",
               "UnityEngine.Vector3 BitAttributeTests.Vector3PackInvalid.MyBehaviour::value8");
            HasError("Max must be above 0, max:(1.0, -1.0, 0.0)",
               "UnityEngine.Vector3 BitAttributeTests.Vector3PackInvalid.MyBehaviour::value9");
            HasError("Max must be above 0, max:(1.0, 1.0, -1.0)",
               "UnityEngine.Vector3 BitAttributeTests.Vector3PackInvalid.MyBehaviour::value10");
            HasError("Max must be above 0, max:(-1.0, 0.0, 0.0)",
               "UnityEngine.Vector3 BitAttributeTests.Vector3PackInvalid.MyBehaviour::value11");
        }

        [Test]
        public void Vector2Pack()
        {
            IsSuccess();
        }

        [Test]
        public void Vector2PackInvalid()
        {
            HasErrorCount(9);
            HasError("System.Single is not a supported type for [Vector2Pack]",
               "System.Single BitAttributeTests.Vector2PackInvalid.MyBehaviour::value1");
            HasError("UnityEngine.Vector3 is not a supported type for [Vector2Pack]",
               "UnityEngine.Vector3 BitAttributeTests.Vector2PackInvalid.MyBehaviour::value2");

            HasError("BitCount must be between 1 and 30 (inclusive), bitCount:31",
               "UnityEngine.Vector2 BitAttributeTests.Vector2PackInvalid.MyBehaviour::value3");
            HasError("BitCount must be between 1 and 30 (inclusive), bitCount:31",
               "UnityEngine.Vector2 BitAttributeTests.Vector2PackInvalid.MyBehaviour::value4");
            HasError("BitCount must be between 1 and 30 (inclusive), bitCount:0",
               "UnityEngine.Vector2 BitAttributeTests.Vector2PackInvalid.MyBehaviour::value5");
            HasError("BitCount must be between 1 and 30 (inclusive), bitCount:0",
               "UnityEngine.Vector2 BitAttributeTests.Vector2PackInvalid.MyBehaviour::value6");
            HasError("Max must be above 0, max:(-1.0, 0.0)",
               "UnityEngine.Vector2 BitAttributeTests.Vector2PackInvalid.MyBehaviour::value7");
            HasError("Max must be above 0, max:(1.0, -1.0)",
               "UnityEngine.Vector2 BitAttributeTests.Vector2PackInvalid.MyBehaviour::value8");
            HasError("Max must be above 0, max:(-1.0, 0.0)",
               "UnityEngine.Vector2 BitAttributeTests.Vector2PackInvalid.MyBehaviour::value9");
        }

        [Test]
        public void QuaternionPack()
        {
            IsSuccess();
        }

        [Test]
        public void QuaternionPackInvalid()
        {
            HasErrorCount(4);

            HasError("System.Single is not a supported type for [QuaternionPack]",
               "System.Single BitAttributeTests.QuaternionPackInvalid.MyBehaviour::value1");
            HasError("UnityEngine.Vector3 is not a supported type for [QuaternionPack]",
               "UnityEngine.Vector3 BitAttributeTests.QuaternionPackInvalid.MyBehaviour::value2");

            HasError("BitCount should be above 0",
               "UnityEngine.Quaternion BitAttributeTests.QuaternionPackInvalid.MyBehaviour::value3");
            HasError("BitCount should be below 20",
               "UnityEngine.Quaternion BitAttributeTests.QuaternionPackInvalid.MyBehaviour::value4");
        }

        [Test]
        public void VarInt()
        {
            IsSuccess();
        }

        [Test]
        public void VarIntInvalid()
        {
            HasErrorCount(13);

            HasError("System.Single is not a supported type for [VarInt]",
                "System.Single BitAttributeTests.VarIntInvalid.MyBehaviour::value1");
            HasError("UnityEngine.Vector3 is not a supported type for [VarInt]",
                "UnityEngine.Vector3 BitAttributeTests.VarIntInvalid.MyBehaviour::value2");

            HasError("Small value should be greater than 0",
                "System.Int32 BitAttributeTests.VarIntInvalid.MyBehaviour::value3");
            HasError("Medium value should be greater than 0",
                "System.Int32 BitAttributeTests.VarIntInvalid.MyBehaviour::value4");
            HasError("Large value should be greater than 0",
                "System.Int32 BitAttributeTests.VarIntInvalid.MyBehaviour::value5");

            HasError("The small bit count should be less than medium bit count",
                "System.Int32 BitAttributeTests.VarIntInvalid.MyBehaviour::value6");
            HasError("The medium bit count should be less than large bit count",
                "System.Int32 BitAttributeTests.VarIntInvalid.MyBehaviour::value7");
            HasError("The small bit count should be less than medium bit count",
                "System.Int32 BitAttributeTests.VarIntInvalid.MyBehaviour::value8");

            HasError("Small bit count can not be above target type size, bitCount:9, max size:8, type:Byte",
                "System.Byte BitAttributeTests.VarIntInvalid.MyBehaviour::value9");
            HasError("Medium bit count can not be above target type size, bitCount:10, max size:8, type:Byte",
                "System.Byte BitAttributeTests.VarIntInvalid.MyBehaviour::value10");
            HasError("Large bit count can not be above target type size, bitCount:10, max size:8, type:Byte",
                "System.Byte BitAttributeTests.VarIntInvalid.MyBehaviour::value11");


            HasError("[VarInt] can't be used with [BitCount], [VarIntBlocks] or [BitCountFromRange]",
                "System.Int32 BitAttributeTests.VarIntInvalid.MyBehaviour::value12");
            HasError("[BitCountFromRange] can't be used with [BitCount], [VarInt] or [VarIntBlocks]",
                "System.Int32 BitAttributeTests.VarIntInvalid.MyBehaviour::value13");
        }

        [Test]
        public void VarIntBlocks()
        {
            IsSuccess();
        }

        [Test]
        public void VarIntBlocksInvalid()
        {
            HasErrorCount(5);

            HasError("Blocksize should be above 0",
                "System.Int32 BitAttributeTests.VarIntBlocksInValid.MyBehaviour::value1");
            HasError("Blocksize should be above 0",
                "System.Int32 BitAttributeTests.VarIntBlocksInValid.MyBehaviour::value2");
            HasError("Blocksize should be below 32",
                "System.Int32 BitAttributeTests.VarIntBlocksInValid.MyBehaviour::value3");
            HasError("System.Single is not supported for [VarIntBlocks]",
                "System.Single BitAttributeTests.VarIntBlocksInValid.MyBehaviour::value4");
            HasError("UnityEngine.Vector3 is not supported for [VarIntBlocks]",
                "UnityEngine.Vector3 BitAttributeTests.VarIntBlocksInValid.MyBehaviour::value5");
        }
    }
}
