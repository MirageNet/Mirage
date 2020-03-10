using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Mirror.Examples.Additive
{
    [AddComponentMenu("")]
    public class AdditiveNetworkManager : NetworkManager
    {
        [Scene]
        [FormerlySerializedAs("subScenes")]
        [Tooltip("Add all sub-scenes to this list")]
        public string[] SubScenes;

        protected override void OnStartServer()
        {
            base.OnStartServer();

            // load all subscenes on the server only
            StartCoroutine(LoadSubScenes());
        }

        IEnumerator LoadSubScenes()
        {
            if (LogFilter.Debug)
                Debug.Log("Loading Scenes");

            foreach (string sceneName in SubScenes)
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

                if (LogFilter.Debug)
                    Debug.Log($"Loaded {sceneName}");
            }
        }

        protected override void OnStopServer()
        {
            StartCoroutine(UnloadScenes());
        }

        protected override void OnStopClient()
        {
            StartCoroutine(UnloadScenes());
        }

        IEnumerator UnloadScenes()
        {
            if (LogFilter.Debug)
                Debug.Log("Unloading Subscenes");

            foreach (string sceneName in SubScenes)
                if (SceneManager.GetSceneByName(sceneName).IsValid())
                {
                    yield return SceneManager.UnloadSceneAsync(sceneName);

                    if (LogFilter.Debug)
                        Debug.Log($"Unloaded {sceneName}");
                }

            yield return Resources.UnloadUnusedAssets();
        }
    }
}
