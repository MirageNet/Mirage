using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    public class HostSetup<T> : HostSetupBase<T> where T : NetworkBehaviour
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp() => HostSetUp().ToCoroutine();

        [UnityTearDown]
        public IEnumerator UnityTearDown() => HostTearDown().ToCoroutine();
    }
}
