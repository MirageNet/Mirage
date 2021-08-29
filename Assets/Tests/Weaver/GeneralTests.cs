using NUnit.Framework;

namespace Mirage.Tests.Weaver
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
