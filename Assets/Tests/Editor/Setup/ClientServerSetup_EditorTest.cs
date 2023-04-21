using System.Collections;
using UnityEngine.TestTools;
using Mirage.Tests.BaseSetups;

namespace Mirage.Tests.EnterRuntime
{
    public class ServerSetup_EditorModeTest : ServerSetup_Base
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

    public class ClientServerSetup_EditorModeTest : RemoteClientSetup_Base
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

    public class ClientServerSetup_EditorModeTest<T> : RemoteClientSetup_Base<T> where T : NetworkBehaviour
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

    public class ClientServerSetup_EditorModeTest<T1, T2> : RemoteClientSetup_Base<T1, T2> where T1 : NetworkBehaviour where T2 : NetworkBehaviour
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
