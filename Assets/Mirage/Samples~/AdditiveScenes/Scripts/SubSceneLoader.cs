using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Examples.Additive
{
    /// <summary>
    /// Loads the sub scene on the server, and on each client when they enter the zone.
    /// <para>
    /// Based on <see cref="Mirage.Components.NetworkSceneLoader"/>, but loads a scene additively per player
    /// instead of changing the scene for everyone.
    /// </para>
    /// <para>
    /// Sub scene objects use <see cref="SubSceneProximityChecker"/>, which asks this class if a player has loaded the sub scene,
    /// so that the server never sends spawn messages for objects the client can't find.
    /// </para>
    /// </summary>
    public class SubSceneLoader : MonoBehaviour
    {
        [NetworkMessage]
        public struct SubSceneMessage
        {
            /// <summary>true to load, false to unload</summary>
            public bool Load;
            /// <summary>Sent back in <see cref="SubSceneLoadedMessage"/>, so server can ignore replies to old requests</summary>
            public int RequestId;
        }

        [NetworkMessage]
        public struct SubSceneLoadedMessage
        {
            public int RequestId;
        }

        [Header("References")]
        [RequiredField(autoFind: true)]
        public NetworkServer Server;
        [RequiredField(autoFind: true)]
        public NetworkClient Client;
        [RequiredField(autoFind: true)]
        public ServerObjectManager ServerObjectManager;
        [RequiredField(autoFind: true)]
        public ClientObjectManager ClientObjectManager;

        [Scene]
        [RequiredField]
        public string SubScene;

        // server state
        private Scene _serverSubScene;
        // players that were told to load but have not replied yet, and the id of that request
        private readonly Dictionary<INetworkPlayer, int> _loadingPlayers = new Dictionary<INetworkPlayer, int>();
        private int _nextRequestId;
        private readonly HashSet<INetworkPlayer> _loadedPlayers = new HashSet<INetworkPlayer>();

        // client state
        private bool _clientWantsSubScene;
        private int _clientRequestId;
        private bool _clientHasSubScene;
        private bool _clientBusy;

        private void Awake()
        {
            Server.Started.AddListener(OnServerStarted);
            Server.Disconnected.AddListener(OnServerDisconnected);
            Server.Stopped.AddListener(OnServerStopped);
            Client.Started.AddListener(OnClientStarted);
            Client.Disconnected.AddListener(OnClientDisconnected);
        }

        #region Server

        private void OnServerStarted()
        {
            Server.MessageHandler.RegisterHandler<SubSceneLoadedMessage>(HandleSubSceneLoadedMessage);
            ServerLoadSubScene().Forget();
        }

        private async UniTaskVoid ServerLoadSubScene()
        {
            // server always has the sub scene loaded, so that its objects exist and can be shown to players that enter the zone
            await SceneManager.LoadSceneAsync(SubScene, LoadSceneMode.Additive).ToUniTask();
            var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            // server was stopped while loading, OnServerStopped had nothing to unload yet
            if (!Server.Active)
            {
                SceneManager.UnloadSceneAsync(scene);
                return;
            }

            _serverSubScene = scene;
            ServerObjectManager.SpawnSceneObjects();
        }

        /// <summary>
        /// Can the player see objects in the sub scene?
        /// </summary>
        public bool HasLoadedSubScene(INetworkPlayer player)
        {
            // host shares the server's scenes, so always has it loaded
            return player.IsHost || _loadedPlayers.Contains(player);
        }

        public bool IsInSubScene(GameObject obj)
        {
            return obj.scene == _serverSubScene;
        }

        /// <summary>
        /// Tells the player to load the sub scene. Objects in it will be shown once the client says it has finished loading.
        /// </summary>
        public void ServerShowSubScene(INetworkPlayer player)
        {
            if (player.IsHost)
                return;

            // already loaded, eg trigger enter called twice
            if (_loadedPlayers.Contains(player))
                return;

            _nextRequestId++;
            _loadingPlayers[player] = _nextRequestId;
            player.Send(new SubSceneMessage { Load = true, RequestId = _nextRequestId });
        }

        /// <summary>
        /// Hides the sub scene objects from the player, then tells them to unload the sub scene.
        /// </summary>
        public void ServerHideSubScene(INetworkPlayer player)
        {
            if (player.IsHost)
                return;

            _loadingPlayers.Remove(player);

            // hide objects before unloading, so the client doesn't destroy objects the server still thinks it can see
            if (_loadedPlayers.Remove(player))
                RebuildSubSceneObservers();

            player.Send(new SubSceneMessage { Load = false });
        }

        private void HandleSubSceneLoadedMessage(INetworkPlayer player, SubSceneLoadedMessage message)
        {
            // ignore if server didn't ask the player to load, or if the reply is for an older request.
            // a quick exit and re-enter could otherwise mark the player loaded while their client is still unloading
            if (!_loadingPlayers.TryGetValue(player, out var requestId) || requestId != message.RequestId)
                return;

            _loadingPlayers.Remove(player);

            _loadedPlayers.Add(player);

            // show objects now, instead of waiting for next proximity check
            RebuildSubSceneObservers();
        }

        private void RebuildSubSceneObservers()
        {
            foreach (var identity in Server.World.SpawnedIdentities)
            {
                if (IsInSubScene(identity.gameObject))
                    identity.RebuildObservers(false);
            }
        }

        private void OnServerDisconnected(INetworkPlayer player)
        {
            _loadingPlayers.Remove(player);
            _loadedPlayers.Remove(player);
        }

        private void OnServerStopped()
        {
            _loadingPlayers.Clear();
            _loadedPlayers.Clear();

            if (_serverSubScene.IsValid())
                SceneManager.UnloadSceneAsync(_serverSubScene);
            _serverSubScene = default;
        }

        #endregion

        #region Client

        private void OnClientStarted()
        {
            Client.MessageHandler.RegisterHandler<SubSceneMessage>(HandleSubSceneMessage);
        }

        private void HandleSubSceneMessage(SubSceneMessage message)
        {
            Debug.Assert(!Client.IsHost, "Server should not send SubSceneMessage to host");

            _clientWantsSubScene = message.Load;
            if (message.Load)
                _clientRequestId = message.RequestId;

            // if busy, the running loop will pick up the new value when the current load/unload finishes
            if (!_clientBusy)
                ClientUpdateSubScene().Forget();
        }

        private void OnClientDisconnected(ClientStoppedReason reason)
        {
            _clientWantsSubScene = false;
            if (!_clientBusy)
                ClientUpdateSubScene().Forget();
        }

        /// <summary>
        /// Loads or unloads until the scene matches what the server last asked for.
        /// <para>Server can send load and unload before the first one finishes, and unity can't unload a scene while it is still loading</para>
        /// </summary>
        private async UniTaskVoid ClientUpdateSubScene()
        {
            _clientBusy = true;
            try
            {
                while (_clientHasSubScene != _clientWantsSubScene)
                {
                    if (_clientWantsSubScene)
                    {
                        await SceneManager.LoadSceneAsync(SubScene, LoadSceneMode.Additive).ToUniTask();
                        _clientHasSubScene = true;

                        // only reply if server still wants us to have it (and client is still connected),
                        // otherwise loop will unload it
                        if (_clientWantsSubScene)
                        {
                            // register the new scene objects so the server can spawn them
                            ClientObjectManager.PrepareToSpawnSceneObjects();
                            Client.Send(new SubSceneLoadedMessage { RequestId = _clientRequestId });
                        }
                    }
                    else
                    {
                        await SceneManager.UnloadSceneAsync(SubScene).ToUniTask();
                        _clientHasSubScene = false;
                    }
                }
            }
            finally
            {
                // reset even if something throws, otherwise loader would never run again
                _clientBusy = false;
            }
        }

        #endregion
    }
}
