using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Mirage.Tests.EnterRuntime
{
    public static class EditorModeTestUtil
    {
        public static IEnumerator EnterPlayModeAndSetup(Func<UniTask> setup)
        {
            // edit tests can only yield null..
            yield return new EnterPlayMode();
            yield return null;

            // load start scene because NetworkSceneManager doesn't like empty scenes with no names
            var op = SceneManager.LoadSceneAsync(TestScenes.StartScene);
            while (!op.isDone)
                yield return null;

            yield return null;


            Debug.Log("Starting Setup after EnterPlayMode");
            var runner = new Runner();
            var task = setup.Invoke();
            runner.RunTask(task).Forget();
            while (!runner.SetupComplete)
            {
                if (runner.Exception != null)
                    throw runner.Exception;

                yield return null;
            }
            Debug.Log("Setup Finished");
        }

        public static IEnumerator TearDownAndExitPlayMode(Func<UniTask> teardown)
        {
            Debug.Log("Starting TearDown before ExitPlayMode");
            var runner = new Runner();
            var task = teardown.Invoke();
            runner.RunTask(task).Forget();
            while (!runner.SetupComplete)
            {
                if (runner.Exception != null)
                    throw runner.Exception;

                yield return null;
            }
            Debug.Log("TearDown Finished");

            yield return null;
            yield return new ExitPlayMode();
        }

        private class Runner
        {
            public bool SetupComplete;
            public Exception Exception;

            public async UniTaskVoid RunTask(UniTask task)
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    Exception = ex;
                }
                finally
                {
                    SetupComplete = true;
                }
            }
        }
    }
}
