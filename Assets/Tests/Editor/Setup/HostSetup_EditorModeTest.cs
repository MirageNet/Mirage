using System.Collections;
using UnityEngine.TestTools;
using Mirage.Tests.BaseSetups;

namespace Mirage.Tests.EnterRuntime
{
    public class HostSetup_EditorModeTest : HostSetup_Base
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            return EditorModeTestUtil.EnterPlayModeAndSetup(ServerSeutup);
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            return EditorModeTestUtil.TearDownAndExitPlayMode(TearDownAsync);
        }
    }

    public class HostSetup_EditorModeTest<T> : HostSetup_Base<T> where T : NetworkBehaviour
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            return EditorModeTestUtil.EnterPlayModeAndSetup(ServerSeutup);
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            return EditorModeTestUtil.TearDownAndExitPlayMode(TearDownAsync);
        }
    }
}
