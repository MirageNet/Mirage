using NUnit.Framework;

namespace Mirage.Weaver
{
    public class GeneralTests : TestsBuildFromTestName
    {
        [Test]
        public void RecursionCount()
        {
            IsSuccess();
        }

        [Test]
        public void TestingScriptableObjectArraySerialization()
        {
            IsSuccess();
        }
    }
}
