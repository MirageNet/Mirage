using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.SocketLayer;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Mirage.Tests.Performance.Runtime
{
    [Category("Performance")]
    [Category("Benchmark")]
    public class MultipleClients
    {
        private const string ScenePath = "Assets/Tests/Performance/Runtime/MultipleClients/Scenes/Scene.unity";
        private const string MonsterPath = "Assets/Tests/Performance/Runtime/MultipleClients/Prefabs/Monster.prefab";
        private const int Warmup = 50;
        private const int MeasureCount = 256;
        private const int ClientCount = 10;
        private const int MonsterCount = 10;

        public NetworkServer Server;
        public ServerObjectManager ServerObjectManager;
        public SocketFactory socketFactory;

        public NetworkIdentity MonsterPrefab;


        [UnitySetUp]
        public IEnumerator SetUp() => UniTask.ToCoroutine(async () =>
        {
#if UNITY_EDITOR
            await EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive }).ToUniTask();
#else
            throw new System.NotSupportedException("Test not supported in player");
#endif
            var scene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(scene);

#if UNITY_EDITOR
            MonsterPrefab = AssetDatabase.LoadAssetAtPath<NetworkIdentity>(MonsterPath);
#else
            throw new System.NotSupportedException("Test not supported in player");
#endif

            // load host
            Server = Object.FindObjectOfType<NetworkServer>();
            ServerObjectManager = Object.FindObjectOfType<ServerObjectManager>();

            Server.Authenticated.AddListener(conn => ServerObjectManager.SpawnVisibleObjects(conn, true));

            var started = new UniTaskCompletionSource();
            Server.Started.AddListener(() => started.TrySetResult());

            // wait 1 frame before Starting server to give time for Unity to call "Start"
            await UniTask.Yield();
            Server.StartServer();

            await started.Task;

            socketFactory = Server.GetComponent<SocketFactory>();
            Debug.Assert(socketFactory != null, "Could not find socket factory for test");

            // connect from a bunch of clients
            for (var i = 0; i < ClientCount; i++)
                await StartClient(i, socketFactory);

            // spawn a bunch of monsters
            for (var i = 0; i < MonsterCount; i++)
                SpawnMonster(i);

            while (Object.FindObjectsOfType<MonsterBehavior>().Count() < MonsterCount * (ClientCount + 1))
                await UniTask.Delay(10);
        });

        private IEnumerator StartClient(int i, SocketFactory socketFactory)
        {
            var clientGo = new GameObject($"Client {i}", typeof(NetworkClient), typeof(ClientObjectManager));
            var client = clientGo.GetComponent<NetworkClient>();
            var objectManager = clientGo.GetComponent<ClientObjectManager>();
            client.ObjectManager = objectManager;
            client.SocketFactory = socketFactory;

            objectManager.RegisterPrefab(MonsterPrefab);
            client.Connect("localhost");
            while (!client.IsConnected)
                yield return null;
        }

        private void SpawnMonster(int i)
        {
            var monster = Object.Instantiate(MonsterPrefab);

            monster.GetComponent<MonsterBehavior>().MonsterId = i;
            monster.gameObject.name = $"Monster {i}";
            ServerObjectManager.Spawn(monster.gameObject);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // shutdown
            Server.Stop();
            yield return null;

            // unload scene
            var scene = SceneManager.GetSceneByPath(ScenePath);
            yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        [Performance]
        public IEnumerator SyncMonsters()
        {
            yield return Measure.Frames().MeasurementCount(MeasureCount).WarmupCount(Warmup).Run();
        }
    }
}

