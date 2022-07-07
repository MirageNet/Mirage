using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class GeneralTests : WeaverTestBase
    {
        [Test, BatchSafe(BatchType.Success)]
        public void RecursionCount()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void TestingScriptableObjectArraySerialization()
        {
            IsSuccess();
        }
    }
}
