using System;
using System.Collections;
using Mirage.SocketLayer;
using Mirage.Components;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static UnityEngine.Object;

namespace Mirage.Tests.Performance.Runtime
{
    [Ignore("NotImplemented")]
    public class SpatialHashInterestManagementPerformanceTest : InterestManagementPerformanceBase
    {
        protected override IEnumerator SetupInterestManagement(NetworkServer server)
        {
            throw new NotImplementedException();
            //server.gameObject.AddComponent<SpatialHashInterestManager>();

            // wait frame for setup
            yield return null;
        }
    }
    [Ignore("NotImplemented")]
    public class GridAndDistanceInterestManagementPerformanceTest : InterestManagementPerformanceBase
    {
        protected override IEnumerator SetupInterestManagement(NetworkServer server)
        {
            throw new NotImplementedException();
            //server.gameObject.AddComponent<SpatialHashInterestManager>();

            // wait frame for setup
            yield return null;
        }
    }

    [Ignore("NotImplemented")]
    public class QuadTreeInterestManagementPerformanceTest : InterestManagementPerformanceBase
    {
        protected override IEnumerator SetupInterestManagement(NetworkServer server)
        {
            throw new NotImplementedException();
            //server.gameObject.AddComponent<QuadTreeInterestManager>();

            // wait frame for setup
            yield return null;
        }
    }
    [Category("Performance"), Category("InterestManagement")]
    public abstract class InterestManagementPerformanceBase
    {
        const string testScene = "Assets/Examples/InterestManagement/Scenes/AOI.unity";
        const string NpcSpawnerName = "NpcSpawner";
        const string LootSpawnerName = "LootSpawner";
        const int clientCount = 100;
        const int stationaryCount = 3500;
        const int movingCount = 500;


        private NetworkServer server;
        private TestSocketFactory transport;
        IConnection[] clients;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(testScene, new LoadSceneParameters(LoadSceneMode.Single));

            // wait 1 frame for start to be called
            yield return null;

            server = FindObjectOfType<NetworkServer>();
            transport = server.gameObject.AddComponent<TestSocketFactory>();
            server.SocketFactory = transport;

            bool started = false;
            server.MaxConnections = clientCount;

            removeExistingIM();
            // wait frame for destroy
            yield return null;

            yield return SetupInterestManagement(server);

            server.Started.AddListener(() => started = true);
            server.StartServer();

            // wait for start
            while (!started) { yield return null; }

            // connect N clients
            clients = new IConnection[clientCount];
            for (int i = 0; i < clientCount; i++)
            {
                transport.CreateClientSocket().Connect(default);
            }
        }

        private void removeExistingIM()
        {
            BaseVisibilityInspector[] existing = server.GetComponents<BaseVisibilityInspector>();

            for (int i = 0; i < existing.Length; i++)
            {
                Destroy(existing[i]);
            }
        }

        /// <summary>
        /// Called before server starts
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        protected abstract IEnumerator SetupInterestManagement(NetworkServer server);


        [UnityTearDown]
        public IEnumerator TearDown()
        {
            server.Stop();

            // open new scene so that old one is destroyed
            SceneManager.CreateScene("empty", new CreateSceneParameters(LocalPhysicsMode.None));
            yield return SceneManager.UnloadSceneAsync(testScene);
        }

        [UnityTest]
        public IEnumerator RunsWithoutErrors()
        {
            yield return new WaitForSeconds(5);
        }

        [UnityTest, Performance]
        public IEnumerator FramePerformance()
        {
            SampleGroup[] sampleGroups =
            {
                new SampleGroup("Observers", SampleUnit.Microsecond),
                new SampleGroup("OnAuthenticated", SampleUnit.Microsecond),
                new SampleGroup("OnSpawned", SampleUnit.Microsecond),
            };

            yield return Measure.Frames()
                .ProfilerMarkers(sampleGroups)
                .WarmupCount(5)
                .MeasurementCount(300)
                .Run();
        }

    }
}
