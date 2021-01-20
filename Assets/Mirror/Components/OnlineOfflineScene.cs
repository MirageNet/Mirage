using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirror
{
    public class OnlineOfflineScene : MonoBehaviour
    {
        public NetworkClient client;
        public NetworkServer server;

        [Scene]
        [Tooltip("Assign the OfflineScene to load for this zone")]
        public string OfflineScene;

        [Scene]
        [Tooltip("Assign the OnlineScene to load for this zone")]
        public string OnlineScene;

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
            SceneManager.LoadSceneAsync(OfflineScene);
        }

        void OnServerStarted()
        {
            SceneManager.LoadSceneAsync(OnlineScene);
        }

        void OnServerStopped()
        {
            SceneManager.LoadSceneAsync(OfflineScene);
        }
    }
}
