using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirage;

namespace Mirage.Snippets.Components
{
    public class NetworkSceneManagerSnippet : MonoBehaviour
    {
        public void SceneChangeExamples(NetworkSceneManager sceneManager, IEnumerable<INetworkPlayer> players, Scene scene)
        {
            // CodeEmbed-Start: scene-change-examples
            // For normal scene changes
            sceneManager.ServerLoadSceneNormal("Assets/GameScene.unity");

            // For additive scene loading
            sceneManager.ServerLoadSceneAdditively("Assets/AdditiveScene.unity", players);

            // For additive scene unloading
            sceneManager.ServerUnloadSceneAdditively(scene, players);
            // CodeEmbed-End: scene-change-examples
        }
    }
}
