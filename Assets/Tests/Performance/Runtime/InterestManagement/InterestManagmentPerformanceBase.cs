using System;
using System.Collections;
using Mirage.Components;
using Mirage.Examples.InterestManagement;
using Mirage.SocketLayer;
using Mirage.Sockets.Udp;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static UnityEngine.Object;

namespace Mirage.Tests.Performance.Runtime
{
    public class GlobalInterestManagementPerformance : InterestManagementPerformanceBase
    {
        #region Overrides of InterestManagementPerformanceBase

        /// <summary>
        /// Called after server starts
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        protected override IEnumerator SetupInterestManagement(NetworkServer server)
        {
            yield return null;
        }

        #endregion
    }

    public class MultiInterestManagementPerformance : InterestManagementPerformanceBase
    {
        #region Overrides of InterestManagementPerformanceBase

        /// <summary>
        /// Called after server starts
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        protected override IEnumerator SetupInterestManagement(NetworkServer server)
        {
            server.gameObject.AddComponent<NetworkSceneChecker>();
            server.gameObject.AddComponent<NetworkProximityChecker>();

            NetworkIdentity[] all = FindObjectsOfType<NetworkIdentity>();

            foreach (NetworkIdentity obj in all)
            {
                obj.gameObject.AddComponent<SceneVisibilitySettings>();
                obj.gameObject.AddComponent<NetworkProximitySettings>();
            }

            yield return null;
        }

        #endregion
    }

    public class SceneInterestManagementPerformance : InterestManagementPerformanceBase
    {
        #region Overrides of InterestManagementPerformanceBase

        /// <summary>
        /// Called after server starts
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        protected override IEnumerator SetupInterestManagement(NetworkServer server)
        {
            server.gameObject.AddComponent<NetworkSceneChecker>();

            NetworkIdentity[] all = FindObjectsOfType<NetworkIdentity>();

            foreach (NetworkIdentity obj in all)
            {
                obj.gameObject.AddComponent<SceneVisibilitySettings>();
            }

            yield return null;
        }

        #endregion
    }

    public class ProximityInterestManagerPerformance : InterestManagementPerformanceBase
    {
        #region Overrides of InterestManagementPerformanceBase

        /// <summary>
        /// Called after server starts
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        protected override IEnumerator SetupInterestManagement(NetworkServer server)
        {
            server.gameObject.AddComponent<NetworkProximityChecker>();

            NetworkIdentity[] all = FindObjectsOfType<NetworkIdentity>();

            foreach (NetworkIdentity obj in all)
            {
                obj.gameObject.AddComponent<NetworkProximitySettings>();
            }

            yield return null;
        }

        #endregion
    }

    [Category("Performance"), Category("InterestManagement")]
    public abstract class InterestManagementPerformanceBase
    {
        const string testScene = "Assets/Tests/Performance/Runtime/InterestManagement/InterestManagement/Scenes/AOI.unity";
        const string NpcSpawnerName = "World Floor";
        const int clientCount = 10;
        const int movingCount = 1000;

        private NetworkServer server;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(testScene, new LoadSceneParameters(LoadSceneMode.Single));

            // wait 1 frame for start to be called
            yield return null;

            EnemySpawner enemySpawner = GameObject.Find(NpcSpawnerName).GetComponent<EnemySpawner>();
            enemySpawner.NumberOfEnemiesSpawn = movingCount;

            server = FindObjectOfType<NetworkServer>();

            bool started = false;
            server.MaxConnections = clientCount;

            // wait frame for destroy
            yield return null;

            server.Started.AddListener(() => started = true);
            server.StartServer();

            // wait for start
            while (!started) { yield return null; }

            // wait for all enemies to spawn in.
            while(!enemySpawner.FinishedLoadingEnemies) { yield return null; }

            yield return SetupInterestManagement(server);

            for (int i = 0; i < clientCount; i++)
            {
                GameObject clientGo = new GameObject($"Client {i}", typeof(UdpSocketFactory));
                clientGo.SetActive(false);

                NetworkClient client = clientGo.AddComponent<NetworkClient>();
                ClientObjectManager objectManager = clientGo.AddComponent<ClientObjectManager>();
                CharacterSpawner spawner = clientGo.AddComponent<CharacterSpawner>();

                objectManager.Client = client;

                for (int j = 0; j < server.GetComponent<ClientObjectManager>().spawnPrefabs.Count; j++)
                {
                    objectManager.RegisterPrefab(server.GetComponent<ClientObjectManager>().spawnPrefabs[j]);
                }

                spawner.Client = client;
                spawner.ClientObjectManager = objectManager;
                spawner.PlayerPrefab = server.GetComponent<CharacterSpawner>().PlayerPrefab;

                client.SocketFactory = client.GetComponent<SocketFactory>();

                clientGo.SetActive(true);

                try
                {
                    client.Connect();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Called after server starts
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        protected abstract IEnumerator SetupInterestManagement(NetworkServer server);

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            server.Stop();

            DestroyImmediate(server.gameObject);

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
                new SampleGroup("OnSpawnInWorld", SampleUnit.Microsecond),
                new SampleGroup("Update", SampleUnit.Microsecond),
                new SampleGroup("Send", SampleUnit.Microsecond),
            };

            yield return Measure.Frames()
                .ProfilerMarkers(sampleGroups)
                .WarmupCount(5)
                .MeasurementCount(300)
                .Run();
        }
    }
}
