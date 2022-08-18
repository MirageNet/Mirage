using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class ClientServerSetup<T> : ClientServerSetupBase<T> where T : NetworkBehaviour
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp() => ClientServerSetUp().ToCoroutine();

        [UnityTearDown]
        public IEnumerator UnityTearDown() => ClientServerTearDown().ToCoroutine();
    }
}
