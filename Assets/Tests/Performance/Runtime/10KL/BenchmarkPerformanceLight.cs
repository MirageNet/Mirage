using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Mirage.Tests.Performance.Runtime
{
    [Category("Performance")]
    [Category("Benchmark")]
    public class BenchmarkPerformanceLight
    {
        const string ScenePath = "Assets/Tests/Performance/Runtime/10KL/Scenes/Scene.unity";

        private NetworkManager networkManager;

        [UnitySetUp]
        public IEnumerator SetUp() => UniTask.ToCoroutine(async () =>
        {
#if UNITY_EDITOR
            await EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive });
#else
            throw new System.NotSupportedException("Test not supported in player");
#endif

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(scene);

            // load host
            networkManager = Object.FindObjectOfType<NetworkManager>();

            // wait frame for Start to be called
            await UniTask.DelayFrame(1);

            networkManager.Server.StartServer(networkManager.Client);

        });

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // shutdown
            if (networkManager != null)
                networkManager.Server.Stop();

            yield return null;

            // unload scene
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            yield return SceneManager.UnloadSceneAsync(scene);

            if (networkManager != null)
                GameObject.Destroy(networkManager.gameObject);
        }

        [UnityTest]
        [Performance]
        public IEnumerator Benchmark10KLight()
        {
            yield return Measure.Frames().MeasurementCount(240).WarmupCount(50).Run();
        }

    }
}

