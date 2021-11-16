using System;
using System.Collections;
using Mirage.Components;
using Mirage.Examples.InterestManagement;
using Mirage.SocketLayer;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static UnityEngine.Object;

namespace Mirage.Tests.Performance.Runtime.AOI
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
            //NOOP
            yield return null;
        }

        protected override void SetupPrefab(NetworkIdentity prefab)
        {
            //NOOP
        }
        protected override void CleanPrefab(NetworkIdentity prefab)
        {
            //NOOP
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
            AddFactoryToServer<SceneVisibilityFactory>(server);
            AddFactoryToServer<DistanceVisibilityFactory>(server);
            yield return null;
        }

        protected override void SetupPrefab(NetworkIdentity prefab)
        {
            AddComponentToPrefab<SceneVisibilitySettings>(prefab);
            AddComponentToPrefab<NetworkProximitySettings>(prefab);
        }
        protected override void CleanPrefab(NetworkIdentity prefab)
        {
            RemoveComponentToPrefab<SceneVisibilitySettings>(prefab);
            RemoveComponentToPrefab<NetworkProximitySettings>(prefab);
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
            AddFactoryToServer<SceneVisibilityFactory>(server);

            yield return null;
        }

        protected override void SetupPrefab(NetworkIdentity prefab)
        {
            AddComponentToPrefab<SceneVisibilitySettings>(prefab);
        }
        protected override void CleanPrefab(NetworkIdentity prefab)
        {
            RemoveComponentToPrefab<SceneVisibilitySettings>(prefab);
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
            AddFactoryToServer<DistanceVisibilityFactory>(server);

            yield return null;
        }

        protected override void SetupPrefab(NetworkIdentity prefab)
        {
            AddComponentToPrefab<NetworkProximitySettings>(prefab);
        }
        protected override void CleanPrefab(NetworkIdentity prefab)
        {
            RemoveComponentToPrefab<NetworkProximitySettings>(prefab);
        }

        #endregion
    }

    [Category("Performance"), Category("InterestManagement")]
    public abstract class InterestManagementPerformanceBase
    {
        const string testScene = "Assets/Tests/Performance/Runtime/InterestManagement/InterestManagement/Scenes/AOI.unity";
        const string MonsterPath = "Assets/Tests/Performance/Runtime/InterestManagement/InterestManagement/Prefabs/Enemy.prefab";
        const string PlayerPath = "Assets/Tests/Performance/Runtime/InterestManagement/InterestManagement/Prefabs/Player.prefab";

        const string NpcSpawnerName = "World Floor";
        const int ClientCount = 2;
        const int MonsterCount = 50;
        const int Warmup = 5;
        const int MeasureCount = 300;

        private NetworkServer Server;
        private NetworkIdentity PlayerPrefab;
        private NetworkIdentity MonsterPrefab;

        /// <summary>
        /// Called after server starts
        /// </summary>
        protected abstract IEnumerator SetupInterestManagement(NetworkServer server);

        /// <summary>
        /// Setsup prefab before it is used to spawn
        /// </summary>
        /// <param name="prefab"></param>
        protected abstract void SetupPrefab(NetworkIdentity prefab);
        /// <summary>
        /// Cleans up prefab so it doesn't effect next test
        /// </summary>
        /// <param name="prefab"></param>
        protected abstract void CleanPrefab(NetworkIdentity prefab);



        [UnitySetUp]
        public IEnumerator Setup()
        {
            // load scene
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(testScene, new LoadSceneParameters(LoadSceneMode.Single));

            MonsterPrefab = AssetDatabase.LoadAssetAtPath<NetworkIdentity>(MonsterPath);
            PlayerPrefab = AssetDatabase.LoadAssetAtPath<NetworkIdentity>(PlayerPath);
            SetupPrefab(MonsterPrefab);
            SetupPrefab(PlayerPrefab);

            EnemySpawner enemySpawner = GameObject.Find(NpcSpawnerName).GetComponent<EnemySpawner>();
            enemySpawner.NumberOfEnemiesSpawn = MonsterCount;

            // load host
            Server = FindObjectOfType<NetworkServer>();
            Server.MaxConnections = ClientCount;

            // wait 1 frame before Starting server to give time for Unity to call "Start"
            yield return null;

            yield return SetupInterestManagement(Server);

            Server.StartServer();

            // set names for existing (we have to call this just incase any are spawned inside Server.Started event
            foreach (NetworkIdentity ni in Server.World.SpawnedIdentities) SetIdentityName(ni);
            // set names for new
            Server.World.onSpawn += SetIdentityName;

            // wait for all enemies to spawn in.
            while (!enemySpawner.FinishedLoadingEnemies) { yield return null; }


            // connect from a bunch of clients
            for (int i = 0; i < ClientCount; i++)
                yield return StartClient(i, Server.GetComponent<SocketFactory>());

            // wait 10 frames for all clients to full setup
            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }
        }

        private void SetIdentityName(NetworkIdentity ni)
        {
            ni.name += $" [netId:{ni.NetId}]";
        }

        private IEnumerator StartClient(int i, SocketFactory socketFactory)
        {
            Scene scene = SceneManager.CreateScene($"Client {i}", new CreateSceneParameters { localPhysicsMode = LocalPhysicsMode.Physics3D });
            var clientGo = new GameObject($"Client {i}");
            // disable object so awake isn't called on new components till we enable it
            clientGo.SetActive(false);
            SceneManager.MoveGameObjectToScene(clientGo, scene);

            NetworkClient client = clientGo.AddComponent<NetworkClient>();
            client.SocketFactory = clientGo.AddComponent(socketFactory.GetType()) as SocketFactory;

            ClientObjectManager objectManager = clientGo.AddComponent<ClientObjectManager>();
            objectManager.Client = client;

            CharacterSpawner spawner = clientGo.AddComponent<CharacterSpawner>();
            spawner.Client = client;
            spawner.ClientObjectManager = objectManager;
            spawner.PlayerPrefab = PlayerPrefab;

            objectManager.RegisterPrefab(MonsterPrefab);
            objectManager.RegisterPrefab(PlayerPrefab);

            // enable so awake is called
            clientGo.SetActive(true);
            // yield so start is called
            yield return null;
            client.Connect("localhost");
            client.World.onSpawn += (ni) =>
            {
                SetIdentityName(ni);
                // move any NI spawned in on client to the scene for that client
                SceneManager.MoveGameObjectToScene(ni.gameObject, scene);
            };
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            CleanPrefab(MonsterPrefab);
            CleanPrefab(PlayerPrefab);

            Server.Stop();

            // wait for all clients to stop
            yield return null;
            yield return null;

            // make sure server object is destroyed
            DestroyImmediate(Server.gameObject);

            // get all scenes, Server+N*clients
            var scenes = new Scene[SceneManager.sceneCount];
            for (int i = 0; i < scenes.Length; i++)
            {
                scenes[i] = SceneManager.GetSceneAt(i);
            }

            // open new scene so that old one is destroyed
            SceneManager.CreateScene("empty", new CreateSceneParameters(LocalPhysicsMode.None));
            for (int i = 0; i < scenes.Length; i++)
            {
                // unload all old scenes
                yield return SceneManager.UnloadSceneAsync(scenes[i]);
            }
        }

        protected static void AddFactoryToServer<TFactory>(NetworkServer server) where TFactory : VisibilitySystemFactory
        {
            // disable, so we can set field before awake
            server.gameObject.SetActive(false);
            TFactory factory = server.gameObject.AddComponent<TFactory>();
            factory.Server = server;
            // triggers awake
            server.gameObject.SetActive(true);
        }

        protected static void AddComponentToPrefab<TSettings>(NetworkIdentity prefab)
        {
            prefab.gameObject.AddComponent<NetworkProximitySettings>();
        }

        protected static void RemoveComponentToPrefab<TSettings>(NetworkIdentity prefab)
        {
            DestroyImmediate(prefab.gameObject.GetComponent<NetworkProximitySettings>(), true);

        }

        [Explicit]
        [UnityTest]
        public IEnumerator RunsWithoutErrors()
        {
            float end = Time.time + 5;
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            long lastFrame = stopwatch.ElapsedMilliseconds;
            float lastFrameTime = Time.time;
            while (Time.time < end)
            {
                yield return null;
                UnityEngine.Debug.Log($"Frame, Time {(Time.time - lastFrameTime) * 1000:0}, SW:{stopwatch.ElapsedMilliseconds - lastFrame:0}");
                lastFrame = stopwatch.ElapsedMilliseconds;
                lastFrameTime = Time.time;
            }
        }

        [Explicit]
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

            // collect GC from setup before we start measuring
            GC.Collect();

            yield return Measure.Frames()
                .ProfilerMarkers(sampleGroups)
                .WarmupCount(Warmup)
                .MeasurementCount(MeasureCount)
                .Run();
        }
    }
}
