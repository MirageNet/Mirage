using UnityEngine;
using Mirage;

namespace Mirage.Snippets.SceneLoading
{
    // CodeEmbed-Start: load-scene-additively
    public class LoadSceneAdditively : MonoBehaviour
    {
        public void Start()
        {
            NetworkSceneManager sceneManager = GetComponent<NetworkSceneManager>();

#pragma warning disable CS0618 // Type or member is obsolete
            sceneManager.ServerLoadSceneAdditively("path to scene asset file.", sceneManager.Server.Players);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
    // CodeEmbed-End: load-scene-additively
}
