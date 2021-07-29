using NUnit.Framework;

namespace Mirage.Weaver
{
    public class BitAttributeTests : TestsBuildFromTestName
    {
        [Test]
        public void BitCountOverTypeSize()
        {
            HasErrorCount(7);
            HasError("BitCount count can not be above target type size, bitCount:9, type:byte, size:8",
                "BitAttributeTests.BitCountOverTypeSize.MyBehaviour::value1");
            // todo add errors for other fields
        }
    }
}
