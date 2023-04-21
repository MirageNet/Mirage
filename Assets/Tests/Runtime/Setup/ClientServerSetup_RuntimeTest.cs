using System.Collections;
using Cysharp.Threading.Tasks;
using Mirage.Tests.BaseSetups;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime
{
    public class ServerSetup : ServerSetup_Base
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp() => ServerSeutup().ToCoroutine();

        [UnityTearDown]
        public IEnumerator UnityTearDown() => TearDownAsync().ToCoroutine();
    }

    public class ClientServerSetup : RemoteClientSetup_Base
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp() => ServerSeutup().ToCoroutine();

        [UnityTearDown]
        public IEnumerator UnityTearDown() => TearDownAsync().ToCoroutine();
    }

    public class ClientServerSetup<T> : RemoteClientSetup_Base<T> where T : NetworkBehaviour
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp() => ServerSeutup().ToCoroutine();

        [UnityTearDown]
        public IEnumerator UnityTearDown() => TearDownAsync().ToCoroutine();
    }

    public class ClientServerSetup<T1, T2> : RemoteClientSetup_Base<T1, T2> where T1 : NetworkBehaviour where T2 : NetworkBehaviour
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp() => ServerSeutup().ToCoroutine();

        [UnityTearDown]
        public IEnumerator UnityTearDown() => TearDownAsync().ToCoroutine();
    }

    public class MultiRemoteClientSetup : MultiRemoteClientSetup_Base
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp() => ServerSeutup().ToCoroutine();

        [UnityTearDown]
        public IEnumerator UnityTearDown() => TearDownAsync().ToCoroutine();
    }

    public class MultiRemoteClientSetup<T> : MultiRemoteClientSetup_Base<T> where T : NetworkBehaviour
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp() => ServerSeutup().ToCoroutine();

        [UnityTearDown]
        public IEnumerator UnityTearDown() => TearDownAsync().ToCoroutine();
    }
}
