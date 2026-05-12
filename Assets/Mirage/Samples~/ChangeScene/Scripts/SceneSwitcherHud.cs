using Cysharp.Threading.Tasks;
using Mirage.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mirage.Examples.SceneChange
{
    public class SceneSwitcherHud : MonoBehaviour
    {
        [NetworkMessage]
        public struct AdditiveSceneMessage
        {
            public string ScenePath;
            public bool Unload;
        }

        public NetworkSceneLoader SceneLoader;
        public Text AdditiveButtonText;
        private bool additiveLoaded;
        private Scene _additiveLoadedScene;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (SceneLoader == null) SceneLoader = GetComponent<NetworkSceneLoader>();
        }
#endif

        private void Start()
        {
            SceneLoader.Client.Started.AddListener(OnClientStarted);
        }

        private void OnClientStarted()
        {
            SceneLoader.Client.MessageHandler.RegisterHandler<AdditiveSceneMessage>(HandleAdditiveSceneMessage);
        }

        private void HandleAdditiveSceneMessage(AdditiveSceneMessage msg)
        {
            if (msg.Unload)
            {
                SceneManager.UnloadSceneAsync(msg.ScenePath);
            }
            else
            {
                SceneManager.LoadSceneAsync(msg.ScenePath, LoadSceneMode.Additive);
            }
        }



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
            SceneLoader.ServerLoadScene("Room1").Forget();
            additiveLoaded = false;
        }

        public void Room2ButtonHandler()
        {
            SceneLoader.ServerLoadScene("Room2").Forget();
            additiveLoaded = false;
        }

        public void AdditiveButtonHandler()
        {
            var players = SceneLoader.Server.AllPlayers;

            if (additiveLoaded)
            {
                additiveLoaded = false;

                // Manual additive unloading as shown in the guides
                foreach (var player in players)
                {
                    player.Send(new AdditiveSceneMessage
                    {
                        ScenePath = _additiveLoadedScene.name,
                        Unload = true
                    });
                }
                SceneManager.UnloadSceneAsync(_additiveLoadedScene);
            }
            else
            {
                additiveLoaded = true;

                // Manual additive loading as shown in the guides
                foreach (var player in players)
                {
                    player.Send(new AdditiveSceneMessage
                    {
                        ScenePath = "Additive",
                        Unload = false
                    });
                }
                SceneManager.LoadSceneAsync("Additive", LoadSceneMode.Additive).completed += (_) =>
                {
                    _additiveLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                };
            }
        }
    }
}
