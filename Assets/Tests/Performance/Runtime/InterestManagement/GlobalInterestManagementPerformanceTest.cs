using System.Collections;
using Mirage.InterestManagement;

namespace Mirage.Tests.Performance.Runtime
{
    public class GlobalInterestManagementPerformanceTest : InterestManagementPerformanceBase
    {
        protected override IEnumerator SetupInterestManagement(NetworkServer server)
        {
            server.gameObject.AddComponent<GlobalInterestManager>();

            // wait frame for setup
            yield return null;
        }
    }
}
