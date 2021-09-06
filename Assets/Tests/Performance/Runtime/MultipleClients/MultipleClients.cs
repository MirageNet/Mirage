using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.SocketLayer;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Mirage.Tests.Performance.Runtime
{
    [Category("Performance")]
    [Category("Benchmark")]
    public class MultipleClients
    {
        const string ScenePath = "Assets/Tests/Performance/Runtime/MultipleClients/Scenes/Scene.unity";
        const string MonsterPath = "Assets/Tests/Performance/Runtime/MultipleClients/Prefabs/Monster.prefab";
        const int Warmup = 50;
        const int MeasureCount = 256;

        const int ClientCount = 10;
        const int MonsterCount = 10;

        public NetworkServer Server;
        public ServerObjectManager ServerObjectManager;
        public SocketFactory socketFactory;

        public NetworkIdentity MonsterPrefab;


        [UnitySetUp]
        public IEnumerator SetUp() => UniTask.ToCoroutine(async () =>
        {
            // load scene
            await EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive });
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(scene);

            MonsterPrefab = AssetDatabase.LoadAssetAtPath<NetworkIdentity>(MonsterPath);
            // load host
            Server = Object.FindObjectOfType<NetworkServer>();
            ServerObjectManager = Object.FindObjectOfType<ServerObjectManager>();

            Server.Authenticated.AddListener(conn => ServerObjectManager.SpawnVisibleObjects(conn));

            var started = new UniTaskCompletionSource();
            Server.Started.AddListener(() => started.TrySetResult());

            // wait 1 frame before Starting server to give time for Unity to call "Start"
            await UniTask.Yield();
            Server.StartServer();

            await started.Task;

            socketFactory = Server.GetComponent<SocketFactory>();
            Debug.Assert(socketFactory != null, "Could not find socket factory for test");

            // connect from a bunch of clients
            for (int i = 0; i < ClientCount; i++)
                await StartClient(i, socketFactory);

            // spawn a bunch of monsters
            for (int i = 0; i < MonsterCount; i++)
                SpawnMonster(i);

            while (Object.FindObjectsOfType<MonsterBehavior>().Count() < MonsterCount * (ClientCount + 1))
                await UniTask.Delay(10);
        });

        private IEnumerator StartClient(int i, SocketFactory socketFactory)
        {
            var clientGo = new GameObject($"Client {i}", typeof(NetworkClient), typeof(ClientObjectManager));
            NetworkClient client = clientGo.GetComponent<NetworkClient>();
            ClientObjectManager objectManager = clientGo.GetComponent<ClientObjectManager>();
            objectManager.Client = client;
            objectManager.Start();
            client.SocketFactory = socketFactory;

            objectManager.RegisterPrefab(MonsterPrefab);
            client.Connect("localhost");
            while (!client.IsConnected)
                yield return null;
        }

        private void SpawnMonster(int i)
        {
            NetworkIdentity monster = Object.Instantiate(MonsterPrefab);

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
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
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

