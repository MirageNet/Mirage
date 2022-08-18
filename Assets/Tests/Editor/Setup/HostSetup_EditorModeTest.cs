using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Mirage.Tests.EnterRuntime
{
    public class HostSetup_EditorModeTest<T> : HostSetupBase<T> where T : NetworkBehaviour
    {
        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            // edit tests can only yield null..
            yield return new EnterPlayMode();
            yield return null;

            // load start scene because NetworkSceneManager doesn't like empty scenes with no names
            var op = SceneManager.LoadSceneAsync(TestScenes.StartScene);
            while (!op.isDone)
                yield return null;

            yield return null;


            var task = HostSetUp();
            Debug.Log("Starting Hostsetup after EnterPlayMode");
            while (task.Status == UniTaskStatus.Pending)
            {
                yield return null;
            }
            Debug.Log("Hostsetup Finished");
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            Debug.Log("Starting HostTearDown before ExitPlayMode");
            var task = HostTearDown();
            while (task.Status == UniTaskStatus.Pending)
            {
                yield return null;
            }
            Debug.Log("HosHostTearDowntsetup Finished");

            yield return HostTearDown().ToCoroutine();
            yield return new ExitPlayMode();
            yield return null;
        }
    }
}
