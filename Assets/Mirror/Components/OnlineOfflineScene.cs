using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirror
{
    public class OnlineOfflineScene : MonoBehaviour
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(OnlineOfflineScene));

        public NetworkClient client;
        public NetworkServer server;

        [Scene]
        [Tooltip("Assign the OnlineScene to load for this zone")]
        public string OnlineScene;

        [Scene]
        [Tooltip("Assign the OfflineScene to load for this zone")]
        public string OfflineScene;

        // Start is called before the first frame update
        void Start()
        {
            if (string.IsNullOrEmpty(OnlineScene))
                logger.LogWarning("OnlineScene missing. Please assign to OnlineOfflineScene component.");

            if (string.IsNullOrEmpty(OfflineScene))
                logger.LogWarning("OfflineScene missing. Please assign to OnlineOfflineScene component.");

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
            if(!string.IsNullOrEmpty(OfflineScene))
                SceneManager.LoadSceneAsync(OfflineScene);
        }

        void OnServerStarted()
        {
            if (!string.IsNullOrEmpty(OnlineScene))
                SceneManager.LoadSceneAsync(OnlineScene);
        }

        void OnServerStopped()
        {
            if (!string.IsNullOrEmpty(OfflineScene))
                SceneManager.LoadSceneAsync(OfflineScene);
        }
    }
}
