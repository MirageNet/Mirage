using Cysharp.Threading.Tasks;
using Mirage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Mirage.Components
{
    public class OnlineOfflineScene : MonoBehaviour
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(OnlineOfflineScene));

        [FormerlySerializedAs("server")]
        public NetworkServer Server;

        public NetworkSceneLoader SceneLoader;

        [Scene]
        [Tooltip("Assign the OnlineScene to load for this zone")]
        public string OnlineScene;

        [Scene]
        [Tooltip("Assign the OfflineScene to load for this zone")]
        public string OfflineScene;

#if UNITY_EDITOR
        private void OnValidate()
        {
            void Check<T>(ref T field) where T : Component
            {
                if (field == null) TryGetComponent(out field);
                if (field == null) Debug.LogError($"{typeof(T).Name} is missing on {name}", this);
            }

            Check(ref Server);
            Check(ref SceneLoader);
        }
#endif

        private void Start()
        {
            if (string.IsNullOrEmpty(OnlineScene))
                throw new MissingReferenceException("OnlineScene missing. Please assign to OnlineOfflineScene component.");

            if (string.IsNullOrEmpty(OfflineScene))
                throw new MissingReferenceException("OfflineScene missing. Please assign to OnlineOfflineScene component.");

            Server.Started.AddListener(OnServerStarted);
            Server.Stopped.AddListener(OnServerStopped);
        }

        private void OnServerStarted()
        {
            SceneLoader.ServerLoadScene(OnlineScene).Forget();
        }

        private void OnServerStopped()
        {
            if (logger.LogEnabled()) logger.Log("OnlineOfflineScene.OnServerStopped: Loading OfflineScene");

            // Server has stopped, so we just load the scene locally
            SceneManager.LoadSceneAsync(OfflineScene);
        }
    }
}
