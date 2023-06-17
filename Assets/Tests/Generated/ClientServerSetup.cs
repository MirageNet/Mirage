using System.Collections;
using Cysharp.Threading.Tasks;
using Mirage.Tests.BaseSetups;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    // copy used by generated tests
    public class ClientServerSetup<T> : RemoteClientSetup_Base<T> where T : NetworkBehaviour
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp() => ServerSeutup().ToCoroutine();

        [UnityTearDown]
        public IEnumerator UnityTearDown() => TearDownAsync().ToCoroutine();
    }
}
