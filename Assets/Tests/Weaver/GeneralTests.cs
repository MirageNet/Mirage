using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class GeneralTests : WeaverTestBase
    {
        [Test, BatchSafe]
        public void RecursionCount()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void TestingScriptableObjectArraySerialization()
        {
            IsSuccess();
        }
    }
}
