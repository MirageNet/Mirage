using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    public class BitHelperTest
    {
        [Test]
        [TestCase(2, 0.05f, ExpectedResult = 7)]
        [TestCase(100, 0.1f, ExpectedResult = 11)]
        [TestCase(.707f, 0.002f, ExpectedResult = 10)]
        [TestCase(1000, 0.1f, ExpectedResult = 15)]
        [TestCase(2000, 0.01f, ExpectedResult = 19)]
        // 1023 is 10 bits, but max is -+ so 11 bits
        [TestCase(1023, 1, ExpectedResult = 11)]
        public int ReturnCorrectBitCount(float max, float precision)
        {
            return BitHelper.BitCount(max, precision);
        }
    }
}
