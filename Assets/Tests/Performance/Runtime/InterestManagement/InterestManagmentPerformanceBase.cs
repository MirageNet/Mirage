using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
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

namespace Mirage.Tests.Performance.Runtime
{
    [Ignore("UnFinished")]
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

        /// <summary>
        ///     Called after each player connects.
        /// </summary>
        /// <param name="networkIdentity"></param>
        protected override void OnSpawned(NetworkIdentity networkIdentity)
        {
            //NOOP
        }

        #endregion
    }

    [Ignore("UnFinished")]
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

            yield return null;
        }

        /// <summary>
        ///     Called after each player connects.
        /// </summary>
        /// <param name="networkIdentity"></param>
        protected override void OnSpawned(NetworkIdentity networkIdentity)
        {
            networkIdentity.gameObject.AddComponent<SceneVisibilitySettings>();
            networkIdentity.gameObject.AddComponent<NetworkProximitySettings>();
        }

        #endregion
    }

    [Ignore("UnFinished")]
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

            yield return null;
        }

        /// <summary>
        ///     Called after each player connects.
        /// </summary>
        /// <param name="networkIdentity"></param>
        protected override void OnSpawned(NetworkIdentity networkIdentity)
        {
            networkIdentity.gameObject.AddComponent<SceneVisibilitySettings>();
        }

        #endregion
    }

    [Ignore("UnFinished")]
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

            yield return null;
        }

        /// <summary>
        ///     Called after each player connects.
        /// </summary>
        /// <param name="networkIdentity"></param>
        protected override void OnSpawned(NetworkIdentity networkIdentity)
        {
            networkIdentity.gameObject.AddComponent<NetworkProximitySettings>();
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
        const int ClientCount = 50;
        const int MonsterCount = 500;
        const int Warmup = 5;
        const int MeasureCount = 300;

        private NetworkServer Server;
        private NetworkIdentity PlayerPrefab;
        private NetworkIdentity MonsterPrefab;

        [UnitySetUp]
        public IEnumerator Setup() => UniTask.ToCoroutine(async () =>
        {
            // load scene
            await EditorSceneManager.LoadSceneAsyncInPlayMode(testScene, new LoadSceneParameters(LoadSceneMode.Single));

            MonsterPrefab = AssetDatabase.LoadAssetAtPath<NetworkIdentity>(MonsterPath);
            PlayerPrefab = AssetDatabase.LoadAssetAtPath<NetworkIdentity>(PlayerPath);

            EnemySpawner enemySpawner = GameObject.Find(NpcSpawnerName).GetComponent<EnemySpawner>();
            enemySpawner.NumberOfEnemiesSpawn = MonsterCount;

            // load host
            Server = FindObjectOfType<NetworkServer>();
            Server.MaxConnections = ClientCount;

            var started = new UniTaskCompletionSource();
            Server.Started.AddListener(() => started.TrySetResult());

            // wait 1 frame before Starting server to give time for Unity to call "Start"
            await UniTask.Yield();
            Server.StartServer();

            Server.World.onSpawn += OnSpawned;

            await started.Task;

            await SetupInterestManagement(Server);

            // connect from a bunch of clients
            for (int i = 0; i < ClientCount; i++)
                await StartClient(i, Server.GetComponent<SocketFactory>());

            while (FindObjectsOfType<MonsterBehavior>().Count() < MonsterCount * (ClientCount + 1))
                await UniTask.Delay(10);
        });

        private IEnumerator StartClient(int i, SocketFactory socketFactory)
        {
            var clientGo = new GameObject($"Client {i}", typeof(NetworkClient), typeof(ClientObjectManager));
            clientGo.SetActive(false);
            NetworkClient client = clientGo.GetComponent<NetworkClient>();
            ClientObjectManager objectManager = clientGo.GetComponent<ClientObjectManager>();
            objectManager.Client = client;
            objectManager.Start();

            client.SocketFactory = socketFactory;

            CharacterSpawner spawner = clientGo.AddComponent<CharacterSpawner>();
            spawner.Client = client;
            spawner.ClientObjectManager = objectManager;
            spawner.PlayerPrefab = PlayerPrefab;

            objectManager.RegisterPrefab(MonsterPrefab);
            objectManager.RegisterPrefab(PlayerPrefab);

            clientGo.SetActive(true);
            client.Connect("localhost");

            yield return null;
        }

        /// <summary>
        /// Called after server starts
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        protected abstract IEnumerator SetupInterestManagement(NetworkServer server);

        /// <summary>
        ///     Called after each player connects.
        /// </summary>
        /// <param name="networkIdentity"></param>
        protected abstract void OnSpawned(NetworkIdentity networkIdentity);

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Server.Stop();

            yield return null;

            // open new scene so that old one is destroyed
            SceneManager.CreateScene("empty", new CreateSceneParameters(LocalPhysicsMode.None));
            yield return SceneManager.UnloadSceneAsync(testScene);

            DestroyImmediate(Server.gameObject);
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
                .WarmupCount(Warmup)
                .MeasurementCount(MeasureCount)
                .Run();
        }
    }
}
