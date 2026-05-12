using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Examples.Basic
{
    /// <summary>
    /// A self-contained spawner example using the "Join Any Time" pattern.
    /// This demonstrates how to implement custom spawning logic (like parenting and naming) 
    /// without relying on inheritance from a helper.
    /// </summary>
    public class CanvasCharacterSpawner : MonoBehaviour
    {
        public NetworkServer Server;
        public NetworkClient Client;
        public ServerObjectManager ServerObjectManager;
        public ClientObjectManager ClientObjectManager;

        [Header("Character Spawning")]
        public NetworkIdentity PlayerPrefab;
        public Transform Parent;

        [Header("State")]
        public string TargetScene;
        public bool ServerLoading;

#if UNITY_EDITOR
        private void OnValidate()
        {
            void Check<T>(ref T field) where T : Component
            {
                if (field == null) TryGetComponent(out field);
                if (field == null) Debug.LogError($"{typeof(T).Name} is missing on {name}", this);
            }

            Check(ref Server);
            Check(ref Client);
            Check(ref ServerObjectManager);
            Check(ref ClientObjectManager);
        }
#endif

        private int _playerCounter;

        private void Awake()
        {
            Server.Started.AddListener(OnServerStarted);
            Server.Authenticated.AddListener(OnServerAuthenticated);
            Client.Started.AddListener(OnClientStarted);
        }

        private void OnServerStarted()
        {
            _playerCounter = 0;
            Server.MessageHandler.RegisterHandler<SceneReadyMessage>(HandleSceneReadyMessage);
        }

        private void OnClientStarted()
        {
            Client.MessageHandler.RegisterHandler<SceneMessage>(HandleSceneMessage);
        }

        public async UniTask ServerLoadScene(string sceneName)
        {
            if (ServerLoading)
                throw new InvalidOperationException("Server is already loading a scene.");

            TargetScene = sceneName;
            ServerLoading = true;

            foreach (var player in Server.AllPlayers)
            {
                player.SceneIsReady = false;
                player.Send(new SceneMessage { ScenePath = TargetScene });
            }

            await SceneManager.LoadSceneAsync(TargetScene);

            if (Server.IsHost)
            {
                ClientObjectManager.PrepareToSpawnSceneObjects();
                Server.LocalPlayer.SceneIsReady = true;
            }

            ServerLoading = false;
            ServerObjectManager.SpawnSceneObjects();

            foreach (var player in Server.AllPlayers)
            {
                if (player.SceneIsReady)
                    SpawnCharacterForPlayer(player);
            }
        }

        private void OnServerAuthenticated(INetworkPlayer player)
        {
            if (player.IsHost)
                return;

            if (!string.IsNullOrEmpty(TargetScene))
            {
                player.SceneIsReady = false;
                player.Send(new SceneMessage { ScenePath = TargetScene });
            }
        }

        private void HandleSceneReadyMessage(INetworkPlayer player, SceneReadyMessage message)
        {
            if (player.SceneIsReady)
                return;

            player.SceneIsReady = true;

            if (!ServerLoading)
                SpawnCharacterForPlayer(player);
        }

        private void SpawnCharacterForPlayer(INetworkPlayer player)
        {
            Debug.Assert(player.Identity == null, "Player already has a character spawned.");

            var character = Instantiate(PlayerPrefab, Parent);

            var basicPlayer = character.GetComponent<BasicPlayer>();
            basicPlayer.playerNo = ++_playerCounter;

            ServerObjectManager.AddCharacter(player, character.gameObject);
        }

        private void HandleSceneMessage(INetworkPlayer player, SceneMessage message)
        {
            if (Client.IsHost)
                return;

            OnClientSceneMessageAsync(player, message).Forget();
        }

        private async UniTaskVoid OnClientSceneMessageAsync(INetworkPlayer player, SceneMessage message)
        {
            player.SceneIsReady = false;
            await SceneManager.LoadSceneAsync(message.ScenePath);

            player.SceneIsReady = true;
            ClientObjectManager.PrepareToSpawnSceneObjects();
            player.Send(new SceneReadyMessage());
        }
    }
}
