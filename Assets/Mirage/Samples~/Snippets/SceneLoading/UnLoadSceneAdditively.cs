using UnityEngine;
using UnityEngine.SceneManagement;
using Mirage;

namespace Mirage.Snippets.SceneLoading
{
#pragma warning disable CS0618 // Type or member is obsolete
    // CodeEmbed-Start: unload-scene-additively
    public class UnLoadSceneAdditively : MonoBehaviour
    {
        public void Start()
        {
            NetworkSceneManager sceneManager = GetComponent<NetworkSceneManager>();

            Scene scene = SceneManager.GetSceneByPath("path to scene asset file.");
            sceneManager.ServerUnloadSceneAdditively(scene, sceneManager.Server.Players);
        }
    }
    // CodeEmbed-End: unload-scene-additively
#pragma warning restore CS0618
}
