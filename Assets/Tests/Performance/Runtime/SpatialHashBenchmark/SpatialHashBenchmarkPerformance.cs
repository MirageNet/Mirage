using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Mirage.Tests.Performance.Runtime.SpatialHashBenchmark;
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
    public class SpatialHashBenchmarkPerformance
    {
        private const string ScenePath = "Assets/Tests/Performance/Runtime/SpatialHashBenchmark/Scenes/DebugBootstrap.unity";
        private const int Warmup = 50;
        private const int MeasureCount = 120;

        [UnitySetUp]
        public IEnumerator SetUp() => UniTask.ToCoroutine(async () =>
        {
#if UNITY_EDITOR
            // Additive scene loading is necessary because the test runner runs in its own scene.
            await EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive }).ToUniTask();
#else
            // Tests are intended to run within the editor's play mode framework.
            throw new System.NotSupportedException("Test not supported in player");
#endif
            var scene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(scene);

            // Fetch DebugStart to monitor startup completion and check client count requirements.
            var debugStart = Object.FindObjectOfType<DebugStart>();
            if (debugStart == null)
                throw new System.NullReferenceException("Could not find DebugStart in the loaded scene.");

            var targetClientCount = debugStart.CreateClientCount;
            var timeoutFrames = 1000;
            var frame = 0;

            // Wait for all local physics scenes, the server, and clients to load and connect.
            while (frame < timeoutFrames)
            {
                var managers = Object.FindObjectsOfType<NetworkManager>();
                var activeServerCount = 0;
                var connectedClientCount = 0;

                foreach (var manager in managers)
                {
                    if (manager.Server != null && manager.Server.Active)
                        activeServerCount++;
                    if (manager.Client != null && manager.Client.IsConnected)
                        connectedClientCount++;
                }

                if (activeServerCount >= 1 && connectedClientCount >= targetClientCount)
                    break;

                await UniTask.Yield();
                frame++;
            }

            // Prevent starting the performance measurements if initialization failed.
            if (frame >= timeoutFrames)
                throw new System.TimeoutException("Timed out waiting for server and clients to start and connect.");
        });

        [UnityTearDown]
        public IEnumerator TearDown() => UniTask.ToCoroutine(async () =>
        {
            // Restore default physics simulation behavior so other tests are not disrupted.
            Physics.autoSimulation = true;

            var managers = Object.FindObjectsOfType<NetworkManager>();
            foreach (var manager in managers)
            {
                if (manager.Server != null && manager.Server.Active)
                    manager.Server.Stop();
                
                Object.Destroy(manager.gameObject);
            }

            await UniTask.Yield();

            // Cleanly unload the additive scene instances that were spawned by DebugStart.
            // We retrieve the active target scene name from DebugStart to ensure all dynamically
            // loaded scenes are matched even if their names or paths are changed under relocation.
            var debugStart = Object.FindObjectOfType<DebugStart>();
            var targetSceneName = debugStart != null ? System.IO.Path.GetFileNameWithoutExtension(debugStart.scene) : null;

            var sceneCount = SceneManager.sceneCount;
            var scenesToUnload = new List<Scene>();
            for (var i = 0; i < sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                // Unloading must target any instance matching the target benchmark scene to prevent scene leakage.
                if (scene.name == targetSceneName || scene.name == "SpatialHashBenchmark" || scene.name == "November2021")
                    scenesToUnload.Add(scene);
            }

            foreach (var scene in scenesToUnload)
            {
                // Unload async only when the scene is loaded to prevent invalid state exceptions.
                if (scene.isLoaded)
                    await SceneManager.UnloadSceneAsync(scene).ToUniTask();
            }

            var bootstrapScene = SceneManager.GetSceneByPath(ScenePath);
            if (bootstrapScene.isLoaded)
                await SceneManager.UnloadSceneAsync(bootstrapScene).ToUniTask();
        });

        [UnityTest]
        [Performance]
        public IEnumerator SpatialHashBenchmark()
        {
            yield return Measure.Frames().MeasurementCount(MeasureCount).WarmupCount(Warmup).Run();
        }
    }
}
