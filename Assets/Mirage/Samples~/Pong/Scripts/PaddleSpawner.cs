using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Examples.Pong
{
    /// <summary>
    /// A self-contained spawner example for the Pong demo.
    /// This demonstrates how to implement specialized spawning logic (like position selection and ball spawning)
    /// using the "Join Any Time" pattern.
    /// </summary>
    public class PaddleSpawner : MonoBehaviour
    {
        public NetworkServer Server;
        public NetworkClient Client;
        public ServerObjectManager ServerObjectManager;
        public ClientObjectManager ClientObjectManager;

        [Header("Character Spawning")]
        public NetworkIdentity PlayerPrefab;
        public Transform leftRacketSpawn;
        public Transform rightRacketSpawn;
        public GameObject ballPrefab;

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

        private GameObject ball;

        private void Awake()
        {
            Server.Started.AddListener(OnServerStarted);
            Server.Authenticated.AddListener(OnServerAuthenticated);
            Server.Disconnected.AddListener(OnServerDisconnect);
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

            // Add player at correct spawn position
            var start = Server.AllPlayers.Count(x => x.HasCharacter) == 0 ? leftRacketSpawn : rightRacketSpawn;
            var character = Instantiate(PlayerPrefab, start.position, start.rotation);
            ServerObjectManager.AddCharacter(player, character.gameObject);

            // Spawn ball if two players are ready
            if (Server.AllPlayers.Count(x => x.HasCharacter) == 2)
            {
                ball = Instantiate(ballPrefab);
                ServerObjectManager.Spawn(ball);
            }
        }

        public void OnServerDisconnect(INetworkPlayer _)
        {
            if (ball != null)
                ServerObjectManager.Destroy(ball);
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
