using System;
using System.Collections;
using System.Collections.Generic;
using Mirage.Examples.InterestManagement;
using Mirage.SocketLayer;
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
        /// Called before server starts
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        protected override IEnumerator SetupInterestManagement(NetworkServer server)
        {
            yield return null;
        }

        #endregion
    }

    [Category("Performance"), Category("InterestManagement")]
    public abstract class InterestManagementPerformanceBase
    {
        const string testScene = "Assets/Tests/Performance/Runtime/InterestManagement/InterestManagement/Scenes/AOI.unity";
        const string LootSpawnerName = "";
        const string NpcSpawnerName = "World Floor";
        const int clientCount = 50;
        const int stationaryCount = 3500;
        const int movingCount = 500;


        private NetworkServer server;
        List<NetworkClient> clients;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(testScene, new LoadSceneParameters(LoadSceneMode.Single));

            // wait 1 frame for start to be called
            yield return null;

            var enemySpawner = GameObject.Find(NpcSpawnerName).GetComponent<EnemySpawner>();
            enemySpawner.NumberOfEnemiesSpawn = movingCount;


            server = FindObjectOfType<NetworkServer>();

            bool started = false;
            server.MaxConnections = clientCount;

            // wait frame for destroy
            yield return null;

            yield return SetupInterestManagement(server);

            server.Started.AddListener(() => started = true);
            server.StartServer();

            // wait for start
            while (!started) { yield return null; }

            // wait for all enemies to spawn in.
            while(!enemySpawner.FinishedLoadingEnemies) { yield return null; }

            // connect N clients
            clients = new List<NetworkClient>(clientCount);

            for (int i = 0; i < clientCount; i++)
            {
                GameObject clientGo = new GameObject($"Client {i}", server.SocketFactory.GetType());
                clientGo.SetActive(false);

                NetworkClient client = clientGo.AddComponent<NetworkClient>();
                ClientObjectManager objectManager = clientGo.AddComponent<ClientObjectManager>();
                CharacterSpawner spawner = clientGo.AddComponent<CharacterSpawner>();
                NetworkSceneManager networkSceneManager = clientGo.AddComponent<NetworkSceneManager>();
                networkSceneManager.Client = client;
                networkSceneManager.DontDestroy = false;

                objectManager.Client = client;
                objectManager.NetworkSceneManager = networkSceneManager;

                for (int j = 0; j < server.GetComponent<ClientObjectManager>().spawnPrefabs.Count; j++)
                {
                    objectManager.RegisterPrefab(server.GetComponent<ClientObjectManager>().spawnPrefabs[j]);
                }

                spawner.Client = client;
                spawner.ClientObjectManager = objectManager;
                spawner.SceneManager = networkSceneManager;
                spawner.PlayerPrefab = server.GetComponent<CharacterSpawner>().PlayerPrefab;

                client.SocketFactory = client.GetComponent<SocketFactory>();

                clientGo.SetActive(true);

                try
                {
                    client.Connect("localhost");

                    clients.Add(client);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
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
            foreach (NetworkClient client in clients)
            {
                client.Disconnect();
            }

            server.Stop();

            // open new scene so that old one is destroyed
            SceneManager.CreateScene("empty", new CreateSceneParameters(LocalPhysicsMode.None));
            yield return SceneManager.UnloadSceneAsync(testScene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
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
