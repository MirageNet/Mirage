using UnityEngine;
using UnityEngine.SceneManagement;
using Mirage;

namespace Mirage.Snippets.SceneLoading
{
    public class LoadSceneNormalParams : MonoBehaviour
    {
        public void Example(NetworkSceneManager sceneManager)
        {
            // CodeEmbed-Start: load-scene-normal-params
            sceneManager.ServerLoadSceneNormal("path to scene asset file.", new LoadSceneParameters { loadSceneMode = LoadSceneMode.Single, localPhysicsMode = LocalPhysicsMode.Physics2D });
            // CodeEmbed-End: load-scene-normal-params
        }
    }
}
