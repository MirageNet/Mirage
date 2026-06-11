using UnityEngine;
using Mirage;

namespace Mirage.Snippets.SceneLoading
{
    // CodeEmbed-Start: load-scene-normal
    public class LoadScene : MonoBehaviour
    {
        public void Start()
        {
            NetworkSceneManager sceneManager = GetComponent<NetworkSceneManager>();

            sceneManager.ServerLoadSceneNormal("path to scene asset file.");
        }
    }
    // CodeEmbed-End: load-scene-normal
}
