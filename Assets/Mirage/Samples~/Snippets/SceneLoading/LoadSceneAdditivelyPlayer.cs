using UnityEngine;
using Mirage;

namespace Mirage.Snippets.SceneLoading
{
    public class LoadSceneAdditivelyPlayer : MonoBehaviour
    {
        public void Example(NetworkSceneManager sceneManager, INetworkPlayer Player)
        {
            // CodeEmbed-Start: load-scene-additively-player
            sceneManager.ServerLoadSceneAdditively("path to scene asset file.", Player);
            // CodeEmbed-End: load-scene-additively-player
        }

        public void ExampleNormal(NetworkSceneManager sceneManager, INetworkPlayer Player)
        {
            // CodeEmbed-Start: load-scene-additively-player-normal
            sceneManager.ServerLoadSceneAdditively("path to scene asset file.", Player, true);
            // CodeEmbed-End: load-scene-additively-player-normal
        }

        public void ExamplePhysics(NetworkSceneManager sceneManager, INetworkPlayer Player)
        {
            // CodeEmbed-Start: load-scene-additively-player-physics
            sceneManager.ServerLoadSceneAdditively("path to scene asset file.", Player, false, new UnityEngine.SceneManagement.LoadSceneParameters { loadSceneMode = UnityEngine.SceneManagement.LoadSceneMode.Additive, localPhysicsMode = UnityEngine.SceneManagement.LocalPhysicsMode.Physics2D });
            // CodeEmbed-End: load-scene-additively-player-physics
        }
    }
}
