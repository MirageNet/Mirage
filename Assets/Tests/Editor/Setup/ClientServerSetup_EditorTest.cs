using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Mirage.Tests.EnterRuntime
{
    public class ClientServerSetup_EditorModeTest<T> : ClientServerSetupBase<T> where T : NetworkBehaviour
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            yield return new EnterPlayMode();
            // load start scene because NetworkSceneManager doesn't like empty scenes with no names
            yield return SceneManager.LoadSceneAsync(TestScenes.StartScene);
            yield return null;
            yield return ClientServerSetUp().ToCoroutine();
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            yield return ClientServerTearDown().ToCoroutine();
            yield return new ExitPlayMode();
        }
    }
}
