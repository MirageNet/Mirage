using Mirage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Mirage
{
    public class OnlineOfflineScene : MonoBehaviour
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(OnlineOfflineScene));

        [FormerlySerializedAs("client")]
        public NetworkClient Client;
        [FormerlySerializedAs("server")]
        public NetworkServer Server;

        public NetworkSceneManager NetworkSceneManager;

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
                throw new MissingReferenceException("OnlineScene missing. Please assign to OnlineOfflineScene component.");

            if (string.IsNullOrEmpty(OfflineScene))
                throw new MissingReferenceException("OfflineScene missing. Please assign to OnlineOfflineScene component.");

            if (Client != null)
            {
                Client.Disconnected.AddListener(OnClientDisconnected);
            }
            if (Server != null)
            {
                Server.Started.AddListener(OnServerStarted);
                Server.Stopped.AddListener(OnServerStopped);
            }
        }

        void OnClientDisconnected(ClientStoppedReason reason)
        {
            SceneManager.LoadSceneAsync(OfflineScene);
        }

        void OnServerStarted()
        {
            // use NetworkSceneManager so that server tell client to also load the scene
            NetworkSceneManager.ChangeServerScene(OnlineScene);
        }

        void OnServerStopped()
        {
            SceneManager.LoadSceneAsync(OfflineScene);
        }
    }
}
