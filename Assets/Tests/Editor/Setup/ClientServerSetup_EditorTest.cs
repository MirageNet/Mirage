using System.Collections;
using UnityEngine.TestTools;

namespace Mirage.Tests.EnterRuntime
{
    public class ClientServerSetup_EditorModeTest<T> : ClientServerSetupBase<T> where T : NetworkBehaviour
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            return EditorModeTestUtil.EnterPlayModeAndSetup(ClientServerSetUp);
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            return EditorModeTestUtil.TearDownAndExitPlayMode(ClientServerTearDown);
        }
    }
}
