using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    // Some tests for SyncObjects are in SyncListTests and apply to SyncDictionary too
    public class SyncSetTests : WeaverTestBase
    {
        [Test, BatchSafe]
        public void SyncSetByteValid()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncSetGenericAbstractInheritance()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncSetGenericInheritance()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncSetInheritance()
        {
            IsSuccess();
        }

        [Test, BatchSafe]
        public void SyncSetStruct()
        {
            IsSuccess();
        }
    }
}
