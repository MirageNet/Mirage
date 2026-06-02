using Mirage;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Snippets.GameObjects
{
    // CodeEmbed-Start: scene-object-filter-example
    public class MySceneManager : MonoBehaviour
    {
        public ServerObjectManager serverObjectManager;
        public ClientObjectManager clientObjectManager;
        public Scene myScene;

        // Set the scene to use for filtering
        public void SetScene(Scene scene) 
        {
            myScene = scene;
        }

        void Awake()
        {
            // Set the filter before spawning scene objects
            System.Func<NetworkIdentity, bool> filter = (NetworkIdentity identity) =>
            {
                return identity.gameObject.scene == myScene;
            };

            serverObjectManager.SceneObjectFilter = filter;
            clientObjectManager.SceneObjectFilter = filter;

            // Now when SpawnSceneObjects is called, it will only
            // consider objects from `myScene`.
            serverObjectManager.SpawnSceneObjects();
        }
    }
    // CodeEmbed-End: scene-object-filter-example
}
