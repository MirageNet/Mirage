using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Components
{
    /// <summary>
    /// NetworkSceneLoader handles simple scene loading and spawning the player character.
    /// For more complex use cases, it it best to create a copy of this script and modify it for your needs.
    /// </summary>
    [AddComponentMenu("Network/NetworkSceneLoader")]
    public class NetworkSceneLoader : MonoBehaviour
    {
        public NetworkServer Server;
        public NetworkClient Client;
        public ServerObjectManager ServerObjectManager;
        public ClientObjectManager ClientObjectManager;

        [Header("Character Spawning")]
        public NetworkIdentity PlayerPrefab;

        [Header("State")]
        public string TargetScene;
        public bool ServerLoading;

#if UNITY_EDITOR
        private void OnValidate()
        {
            void Check<T>(ref T field) where T : Component
            {
                if (field == null)
                {
                    TryGetComponent(out field);
                }

                if (field == null)
                {
                    Debug.LogError($"{typeof(T).Name} is missing on {name}", this);
                }
            }

            Check(ref Server);
            Check(ref Client);
            Check(ref ServerObjectManager);
            Check(ref ClientObjectManager);
        }
#endif

        private void Awake()
        {
            Server.Started.AddListener(OnServerStarted);
            Server.Authenticated.AddListener(OnServerAuthenticated);
            Client.Started.AddListener(OnClientStarted);
        }

        private void OnServerStarted()
        {
            Server.MessageHandler.RegisterHandler<SceneReadyMessage>(HandleSceneReadyMessage);
        }

        private void OnClientStarted()
        {
            Client.MessageHandler.RegisterHandler<SceneMessage>(HandleSceneMessage);
        }

        /// <summary>
        /// Starts a server-authoritative scene load.
        /// </summary>
        public async UniTask ServerLoadScene(string scenePath)
        {
            Debug.Assert(Server.Active);
            if (ServerLoading)
                throw new InvalidOperationException("Server is already loading a scene.");

            TargetScene = scenePath;
            ServerLoading = true;

            // Notify all players to load the scene
            // Note: SceneMessage can be replaced with a custom message if you need to send extra data.
            foreach (var player in Server.AuthenticatedPlayers)
            {
                player.SceneIsReady = false;
                player.Send(new SceneMessage { ScenePath = TargetScene });
            }

            // Load the scene on the server
            await SceneManager.LoadSceneAsync(TargetScene).ToUniTask();

            // If we are in host mode, the local player is already ready
            if (Server.IsHost)
            {
                ClientObjectManager.PrepareToSpawnSceneObjects();
                Server.LocalPlayer.SceneIsReady = true;
            }

            ServerLoading = false;
            ServerObjectManager.SpawnSceneObjects();

            // Spawn characters for everyone who is ready
            foreach (var player in Server.AuthenticatedPlayers)
            {
                if (player.SceneIsReady)
                    SpawnCharacterForPlayer(player);
            }
        }

        private void OnServerAuthenticated(INetworkPlayer player)
        {
            Debug.Assert(Server.Active);
            // Host player is handled by the server loading logic
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
            Debug.Assert(Server.Active);
            if (player.SceneIsReady)
                return;

            player.SceneIsReady = true;

            // Only spawn if the server has finished its own loading
            if (!ServerLoading)
                SpawnCharacterForPlayer(player);
        }

        private void SpawnCharacterForPlayer(INetworkPlayer player)
        {
            Debug.Assert(Server.Active);
            // This is where you would implement custom spawn handling, 
            // like selecting spawn points or using a persistent Player Proxy object.
            // Since this loader assumes scene changes destroy all non-persistent objects,
            // the previous character is automatically destroyed by the scene load,
            // clearing the player's identity reference before we spawn the new one.
            Debug.Assert(player.Identity == null, "Player already has a character spawned.");

            var character = Instantiate(PlayerPrefab);
            ServerObjectManager.AddCharacter(player, character.gameObject);
        }

        private void HandleSceneMessage(INetworkPlayer player, SceneMessage message)
        {
            Debug.Assert(Client.Active);
            // Skip if we are host, as server handles loading
            if (Client.IsHost)
                return;

            OnClientSceneMessageAsync(player, message).Forget();
        }

        private async UniTaskVoid OnClientSceneMessageAsync(INetworkPlayer player, SceneMessage message)
        {
            Debug.Assert(Client.Active);
            player.SceneIsReady = false;
            await SceneManager.LoadSceneAsync(message.ScenePath).ToUniTask();

            player.SceneIsReady = true;
            ClientObjectManager.PrepareToSpawnSceneObjects();
            player.Send(new SceneReadyMessage());
        }
    }
}
