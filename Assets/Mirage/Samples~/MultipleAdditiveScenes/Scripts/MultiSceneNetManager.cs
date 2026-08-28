using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Examples.MultipleAdditiveScenes
{
    /// <summary>
    /// Loads multiple instances of the game scene on the server, and puts each player in one of them.
    /// <para>
    /// Scene loading is based on <see cref="Mirage.Components.NetworkSceneLoader"/>,
    /// but loads scenes additively and the client only loads a single instance.
    /// </para>
    /// </summary>
    [AddComponentMenu("")]
    public class MultiSceneNetManager : NetworkManager
    {
        [Header("MultiScene Setup")]
        public int instances = 3;

        [Scene]
        public string gameScene;

        [Header("Character Spawning")]
        [RequiredField]
        public NetworkIdentity PlayerPrefab;
        [Tooltip("Players are spawned at each point in turn")]
        public List<Transform> SpawnPoints = new List<Transform>();

        [Tooltip("Is server currently loading the sub scenes"), ReadOnlyInspector]
        public bool ServerLoading;

        private readonly List<Scene> _subScenes = new List<Scene>();
        // the one instance a (non-host) client loaded
        private Scene _clientSubScene;
        private int _nextPlayerId;
        // incremented on start and stop, so a load from a previous session knows to stop
        private int _serverSession;
        private bool _serverUnloading;
        private int _nextSpawnIndex;

        private void Awake()
        {
            Server.Started.AddListener(OnServerStarted);
            Server.Authenticated.AddListener(OnServerAuthenticated);
            Server.Stopped.AddListener(OnServerStopped);
            Client.Started.AddListener(OnClientStarted);
            Client.Disconnected.AddListener(OnClientDisconnected);
        }

        #region Server

        private void OnServerStarted()
        {
            Server.MessageHandler.RegisterHandler<SceneReadyMessage>(HandleSceneReadyMessage);
            _nextPlayerId = 0;
            _nextSpawnIndex = 0;

            ServerLoadSubScenes().Forget();
        }

        private async UniTaskVoid ServerLoadSubScenes()
        {
            var session = ++_serverSession;
            ServerLoading = true;

            // if server was restarted quickly, wait for the old scenes to unload,
            // otherwise SpawnSceneObjects could spawn objects in scenes that are about to be destroyed
            await UniTask.WaitUntil(() => !_serverUnloading);

            for (var index = 0; index < instances; index++)
            {
                if (session != _serverSession)
                    return;

                // each instance gets its own physics scene so that matches don't collide with each other
                // note: physics scenes do not auto simulate, see PhysicsSimulator
                var parameters = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
                await SceneManager.LoadSceneAsync(gameScene, parameters).ToUniTask();
                var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

                // server was stopped while loading, OnServerStopped already unloaded the scenes before this one
                if (session != _serverSession)
                {
                    SceneManager.UnloadSceneAsync(scene);
                    return;
                }

                _subScenes.Add(scene);
            }

            ServerLoading = false;
            ServerObjectManager.SpawnSceneObjects();

            // spawn characters for players (eg host) that were ready before sub scenes finished loading
            foreach (var player in Server.AuthenticatedPlayers)
            {
                if (player.SceneIsReady && !player.HasCharacter)
                    SpawnCharacterForPlayer(player);
            }
        }

        private void OnServerAuthenticated(INetworkPlayer player)
        {
            // host shares the server's scenes, so it is already ready
            if (player.IsHost)
            {
                player.SceneIsReady = true;
                if (!ServerLoading)
                    SpawnCharacterForPlayer(player);
                return;
            }

            // tell client to load its copy of the game scene, it will reply with SceneReadyMessage
            player.SceneIsReady = false;
            player.Send(new SceneMessage { ScenePath = gameScene });
        }

        private void HandleSceneReadyMessage(INetworkPlayer player, SceneReadyMessage message)
        {
            if (player.SceneIsReady)
                return;

            player.SceneIsReady = true;

            // Only spawn if the server has finished its own loading
            if (!ServerLoading)
                SpawnCharacterForPlayer(player);
        }

        private void SpawnCharacterForPlayer(INetworkPlayer player)
        {
            Debug.Assert(player.Identity == null, "Player already has a character spawned.");

            var spawnPoint = SpawnPoints[_nextSpawnIndex];
            _nextSpawnIndex = (_nextSpawnIndex + 1) % SpawnPoints.Count;
            var character = Instantiate(PlayerPrefab, spawnPoint.position, spawnPoint.rotation);

            // increment first, so first player has id=1
            _nextPlayerId++;
            var matchIndex = _nextPlayerId % _subScenes.Count;

            // Set SyncVars before AddCharacter so they are included in the spawn message
            var playerScore = character.GetComponent<PlayerScore>();
            playerScore.playerNumber = _nextPlayerId;
            playerScore.scoreIndex = _nextPlayerId / _subScenes.Count;
            playerScore.matchIndex = matchIndex;

            // Move to sub scene before spawning, so NetworkSceneChecker only shows it to players in the same match
            SceneManager.MoveGameObjectToScene(character.gameObject, _subScenes[matchIndex]);

            ServerObjectManager.AddCharacter(player, character.gameObject);
        }

        private void OnServerStopped()
        {
            _serverSession++;
            ServerLoading = false;
            ServerUnloadSubScenes().Forget();
        }

        private async UniTaskVoid ServerUnloadSubScenes()
        {
            // copy and clear first, so the list is ready if server is started again while unloading
            var subScenes = _subScenes.ToArray();
            _subScenes.Clear();

            _serverUnloading = true;
            try
            {
                foreach (var scene in subScenes)
                    await SceneManager.UnloadSceneAsync(scene).ToUniTask();
            }
            finally
            {
                _serverUnloading = false;
            }

            await Resources.UnloadUnusedAssets().ToUniTask();
        }

        #endregion

        #region Client

        private void OnClientStarted()
        {
            Client.MessageHandler.RegisterHandler<SceneMessage>(HandleSceneMessage);
        }

        private void HandleSceneMessage(INetworkPlayer player, SceneMessage message)
        {
            // Skip if we are host, as server handles loading
            if (Client.IsHost)
                return;

            ClientLoadSubScene(player, message).Forget();
        }

        private async UniTaskVoid ClientLoadSubScene(INetworkPlayer player, SceneMessage message)
        {
            // client only needs the instance its player is in, and doesn't run physics so it can use the default physics scene
            await SceneManager.LoadSceneAsync(message.ScenePath, LoadSceneMode.Additive).ToUniTask();
            _clientSubScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            ClientObjectManager.PrepareToSpawnSceneObjects();
            player.Send(new SceneReadyMessage());
        }

        private void OnClientDisconnected(ClientStoppedReason reason)
        {
            // host never loads a client sub scene, its scenes are unloaded by OnServerStopped
            if (!_clientSubScene.IsValid())
                return;

            SceneManager.UnloadSceneAsync(_clientSubScene);
            _clientSubScene = default;
        }

        #endregion
    }
}
