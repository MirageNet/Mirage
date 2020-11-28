using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirror
{
    public class OnlineOfflineScene : MonoBehaviour
    {
        public NetworkClient client;
        public NetworkServer server;

        public Scene OfflineScene;
        public Scene OnlineScene;

        // Start is called before the first frame update
        void Start()
        {
            if (client != null)
            {
                client.Disconnected.AddListener(OnClientDisconnected);
            }
            if (server != null)
            {
                server.Started.AddListener(OnServerStarted);
                server.Stopped.AddListener(OnServerStopped);
            }
        }

        void OnClientDisconnected()
        {
            SceneManager.LoadSceneAsync(OfflineScene.path);
        }

        void OnServerStarted()
        {
            SceneManager.LoadSceneAsync(OnlineScene.path);
        }

        void OnServerStopped()
        {
            SceneManager.LoadSceneAsync(OfflineScene.path);
        }
    }
}
