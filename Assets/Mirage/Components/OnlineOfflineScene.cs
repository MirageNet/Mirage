using Mirage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Mirage
{
    public class OnlineOfflineScene : MonoBehaviour
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(OnlineOfflineScene));

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
        private void Start()
        {
            if (string.IsNullOrEmpty(OnlineScene))
                throw new MissingReferenceException("OnlineScene missing. Please assign to OnlineOfflineScene component.");

            if (string.IsNullOrEmpty(OfflineScene))
                throw new MissingReferenceException("OfflineScene missing. Please assign to OnlineOfflineScene component.");

            if (Server != null)
            {
                Server.Started.AddListener(OnServerStarted);
                Server.Stopped.AddListener(OnServerStopped);
            }
        }

        private void OnServerStarted()
        {
            NetworkSceneManager.ServerLoadSceneNormal(OnlineScene);
        }

        private void OnServerStopped()
        {
            Debug.Log("OnlineOfflineScene.OnServerStopped");
            NetworkSceneManager.ServerLoadSceneNormal(OfflineScene);
        }
    }
}
