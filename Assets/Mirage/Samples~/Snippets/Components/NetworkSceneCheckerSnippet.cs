using Mirage;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Snippets.Components
{
    // Mock structures to ensure snippet compiles
    public enum SceneOperation
    {
        LoadAdditive,
        UnloadAdditive
    }

    [NetworkMessage]
    public struct SceneMessage
    {
        public string sceneName;
        public SceneOperation sceneOperation;
    }

    public class NetworkSceneCheckerSnippet : NetworkBehaviour
    {
        public void LoadScene(string subScene)
        {
            // CodeEmbed-Start: load-scene-async
            SceneManager.LoadSceneAsync(subScene, LoadSceneMode.Additive);
            // CodeEmbed-End: load-scene-async
        }

        public void SendSceneMessage(string subScene)
        {
            // CodeEmbed-Start: send-scene-message
            SceneMessage msg = new SceneMessage
            {
                sceneName = subScene,
                sceneOperation = SceneOperation.LoadAdditive
            };

            Owner.Send(msg);
            // CodeEmbed-End: send-scene-message
        }

        public void MovePlayerToScene(GameObject player, Scene subScene)
        {
            // CodeEmbed-Start: move-player-to-scene
            // Get the SceneVisibilityChecker component
            SceneVisibilityChecker sceneChecker = player.GetComponent<SceneVisibilityChecker>();
            if (sceneChecker != null)
            {
                // Position the character object in world space first (if needed)
                // This assumes it has a NetworkTransform component that will update clients
                player.transform.position = new Vector3(100, 1, 100);

                // Then move the character object to the subscene using the checker's method
                sceneChecker.MoveToScene(subScene);
            }
            // CodeEmbed-End: move-player-to-scene
        }
    }
}
