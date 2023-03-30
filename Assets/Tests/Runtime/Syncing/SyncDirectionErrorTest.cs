using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Syncing
{
    public class SyncDirectionErrorTest : SyncDirectionTestBase<MockPlayer>
    {
        [UnityTest]
        public IEnumerator DoesErrorStuff()
        {
            Assert.Fail();
        }
    }
}
