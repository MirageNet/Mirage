using System.Collections;
using Mirage.InterestManagement;

namespace Mirage.Tests.Performance.Runtime
{
    public class DistanceInterestManagementPerformanceTest : InterestManagementPerformanceBase
    {
        protected override IEnumerator SetupInterestManagement(NetworkServer server)
        {
            server.gameObject.AddComponent<DistanceConstantSightInterestManager>();

            // wait frame for setup
            yield return null;
        }
    }
}
