using System.Collections;
using UnityEngine.TestTools;
using Mirage.Tests.BaseSetups;

namespace Mirage.Tests.EnterRuntime
{
    public class HostSetup_EditorModeTest<T> : HostSetupBase<T> where T : NetworkBehaviour
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            return EditorModeTestUtil.EnterPlayModeAndSetup(HostSetUp);
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            return EditorModeTestUtil.TearDownAndExitPlayMode(HostTearDown);
        }
    }
}
