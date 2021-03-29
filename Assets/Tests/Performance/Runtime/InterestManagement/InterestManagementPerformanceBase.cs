using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static UnityEngine.Object;

namespace Mirage.Tests.Performance.Runtime
{
    [Category("Performance"), Category("InterestManagement")]
    public class InterestManagementPerformanceBase
    {
        const string testScene = "Assets/Examples/InterestManagement/Scenes/Scene.unity";
        const int clientCount = 10;

        private NetworkServer server;
        private LoopbackTransport transport;
        private UniTask serverTask;
        IConnection[] clients;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(testScene, new LoadSceneParameters(LoadSceneMode.Single));

            // wait 1 frame for start to be called
            yield return null;

            server = FindObjectOfType<NetworkServer>();
            transport = server.gameObject.AddComponent<LoopbackTransport>();
            server.Transport = transport;

            bool started = false;
            server.MaxConnections = clientCount;

            yield return SetupInterestManagement(server);

            server.Started.AddListener(() => started = true);
            serverTask = server.ListenAsync();

            // wait for start
            while (!started) { yield return null; }

            // connect N clients
            clients = new IConnection[clientCount];
            for (int i = 0; i < clientCount; i++)
            {
                UniTask<IConnection>.Awaiter task = transport.ConnectAsync(default).GetAwaiter();
                // wait for connect
                while (!task.IsCompleted) { yield return null; }
                clients[i] = task.GetResult();
            }
        }

        /// <summary>
        /// Called before server starts
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        protected virtual IEnumerator SetupInterestManagement(NetworkServer server) { yield return null; }


        [UnityTearDown]
        public IEnumerator TearDown()
        {
            server.Disconnect();
            yield return serverTask.ToCoroutine();

            // open new scene so that old one is destroyed
            SceneManager.CreateScene("empty", new CreateSceneParameters(LocalPhysicsMode.None));
            yield return EditorSceneManager.UnloadSceneAsync(testScene);

            yield return new WaitForSeconds(5);

        }

        [UnityTest]
        public IEnumerator RunsWithoutErrors()
        {
            yield return new WaitForSeconds(5);
        }
    }
}
