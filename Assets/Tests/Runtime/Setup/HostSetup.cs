using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using Mirage.Tests.BaseSetups;

namespace Mirage.Tests.Runtime.Host
{
    public class HostSetup : HostSetup_Base
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp() => ServerSeutup().ToCoroutine();

        [UnityTearDown]
        public IEnumerator UnityTearDown() => TearDownAsync().ToCoroutine();
    }

    public class HostSetup<T> : HostSetup_Base<T> where T : NetworkBehaviour
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp() => ServerSeutup().ToCoroutine();

        [UnityTearDown]
        public IEnumerator UnityTearDown() => TearDownAsync().ToCoroutine();
    }
}
