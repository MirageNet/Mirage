using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mirage.Examples.SceneChange
{
    public class SceneSwitcherHud : MonoBehaviour
    {
        public NetworkSceneManager sceneManager;
        public Text AdditiveButtonText;
        private bool additiveLoaded;
        private Scene _additiveLoadedScene;

        public void Update()
        {
            if (additiveLoaded)
            {
                AdditiveButtonText.text = "Additive Unload";
            }
            else
            {
                AdditiveButtonText.text = "Additive Load";
            }
        }

        public void Room1ButtonHandler()
        {
            sceneManager.ServerLoadSceneNormal("Room1");
            additiveLoaded = false;
        }

        public void Room2ButtonHandler()
        {
            sceneManager.ServerLoadSceneNormal("Room2");
            additiveLoaded = false;
        }

        public void AdditiveButtonHandler()
        {
            var players = sceneManager.Server.AllPlayers;

            if (additiveLoaded)
            {
                additiveLoaded = false;

                sceneManager.ServerUnloadSceneAdditively(_additiveLoadedScene, players);
            }
            else
            {
                additiveLoaded = true;
                sceneManager.ServerLoadSceneAdditively("Additive", players);
                _additiveLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            }
        }
    }
}
