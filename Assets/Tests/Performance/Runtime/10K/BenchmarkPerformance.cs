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
    public class BenchmarkPerformance
    {
        const string ScenePath = "Assets/Tests/Performance/Runtime/10K/Scenes/Scene.unity";
        const int Warmup = 50;
        const int MeasureCount = 120;

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
                Object.Destroy(networkManager.gameObject);
        }

        static void EnableHealth(bool value)
        {
            Health[] all = Object.FindObjectsOfType<Health>();
            foreach (Health health in all)
            {
                health.enabled = value;
            }
        }

        [UnityTest]
        [Performance]
        public IEnumerator Benchmark10K()
        {
            EnableHealth(true);

            yield return Measure.Frames().MeasurementCount(MeasureCount).WarmupCount(Warmup).Run();
        }

        [UnityTest]
        [Performance]
        public IEnumerator Benchmark10KIdle()
        {
            EnableHealth(false);

            yield return Measure.Frames().MeasurementCount(MeasureCount).WarmupCount(Warmup).Run();
        }
    }
}

