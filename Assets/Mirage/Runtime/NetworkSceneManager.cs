using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using InvalidEnumArgumentException = System.ComponentModel.InvalidEnumArgumentException;

namespace Mirage
{
    /// <summary>
    /// NetworkSceneManager is an optional component that helps keep scene in sync between server and client.
    /// <para>The <see cref="NetworkClient">NetworkClient</see> loads scenes as instructed by the <see cref="NetworkServer">NetworkServer</see>.</para>
    /// <para>The <see cref="NetworkServer">NetworkServer</see> controls the currently active Scene and any additive Load/Unload.</para>
    /// <para>when a client connect NetworkSceneManager will send a message telling the new client to load the scene that is active on the server</para>
    /// </summary>
    [AddComponentMenu("Network/NetworkSceneManager")]
    [HelpURL("https://miragenet.github.io/Mirage/docs/components/network-scene-manager")]
    [DisallowMultipleComponent]
    public class NetworkSceneManager : MonoBehaviour
    {
        #region Fields

        private static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkSceneManager));

        [Header("Setup Settings")]

        public NetworkClient Client;
        public NetworkServer Server;
        public ServerObjectManager ServerObjectManager;
        public ClientObjectManager ClientObjectManager;

        [Tooltip("Should server send all additive scenes to new clients when they join?")]
        public bool SendAdditiveScenesOnAuthenticate = true;

        /// <summary>
        ///     Sets the NetworksSceneManagers GameObject to DontDestroyOnLoad. Default = true.
        /// </summary>
        public bool DontDestroy = true;

        private readonly Dictionary<Scene, HashSet<INetworkPlayer>> _serverSceneData = new Dictionary<Scene, HashSet<INetworkPlayer>>();

        /// <summary>
        /// The path of the current active scene.
        /// <para>If using additive scenes this will be the first scene.</para>
        /// <para>Value from  <see cref="SceneManager.GetActiveScene()"/> </para>
        /// </summary>
        /// <remarks>
        /// <para>New clients that connect to a server will automatically load this scene.</para>
        /// <para>This is used to make sure that all scene changes are initialized by Mirage.</para>
        /// </remarks>
        public string ActiveScenePath => SceneManager.GetActiveScene().path;

        /// <summary>
        /// Used by the client to load the full additive scene list that the server has upon connection
        /// </summary>
        internal readonly List<string> _clientPendingAdditiveSceneLoadingList = new List<string>();

        /// <summary>
        ///     Information on any scene that is currently being loaded.
        /// </summary>
        public AsyncOperation SceneLoadingAsyncOperationInfo;


        /// <summary>
        ///     Collection of scenes and which player's are in those scenes.
        /// </summary>
        public IReadOnlyDictionary<Scene, HashSet<INetworkPlayer>> ServerSceneData => _serverSceneData;

        #endregion

        #region Events

        [Header("Events")]

        [FormerlySerializedAs("ClientChangeScene")]
        [SerializeField] private SceneChangeStartedEvent _onClientStartedSceneChange = new SceneChangeStartedEvent();

        [FormerlySerializedAs("ClientSceneChanged")]
        [SerializeField] private SceneChangeFinishedEvent _onClientFinishedSceneChange = new SceneChangeFinishedEvent();

        [FormerlySerializedAs("ServerChangeScene")]
        [SerializeField] private SceneChangeStartedEvent _onServerStartedSceneChange = new SceneChangeStartedEvent();

        [FormerlySerializedAs("ServerSceneChanged")]
        [SerializeField] private SceneChangeFinishedEvent _onServerFinishedSceneChange = new SceneChangeFinishedEvent();

        [SerializeField] private PlayerSceneChangeEvent _onPlayerSceneReady = new PlayerSceneChangeEvent();

        /// <summary>
        /// Event fires when the Client starts changing scene.
        /// </summary>
        public SceneChangeStartedEvent OnClientStartedSceneChange => _onClientStartedSceneChange;

        /// <summary>
        /// Event fires after the Client has completed its scene change.
        /// </summary>
        public SceneChangeFinishedEvent OnClientFinishedSceneChange => _onClientFinishedSceneChange;

        /// <summary>
        /// Event fires before Server changes scene.
        /// </summary>
        public SceneChangeStartedEvent OnServerStartedSceneChange => _onServerStartedSceneChange;

        /// <summary>
        /// Event fires after Server has completed scene change.
        /// </summary>
        public SceneChangeFinishedEvent OnServerFinishedSceneChange => _onServerFinishedSceneChange;

        /// <summary>
        /// Event fires On the server, after Client sends <see cref="SceneReadyMessage"/> to the server
        /// </summary>
        public PlayerSceneChangeEvent OnPlayerSceneReady => _onPlayerSceneReady;

        #endregion

        #region Unity Methods

        public virtual void Start()
        {
            if (DontDestroy)
                DontDestroyOnLoad(gameObject);

            if (Client != null)
                Client.Started.AddListener(RegisterClientMessages);

            if (Server != null)
            {
                Server.Started.AddListener(RegisterServerMessages);
                Server.Authenticated.AddListener(OnServerAuthenticated);
                Server.Disconnected.AddListener(OnServerPlayerDisconnected);
            }
        }

        public virtual void OnDestroy()
        {
            if (Client != null)
                Client.Started.RemoveListener(RegisterClientMessages);

            if (Server != null)
            {
                Server.Started.RemoveListener(RegisterServerMessages);
                Server.Authenticated.RemoveListener(OnServerAuthenticated);
                Server.Disconnected.RemoveListener(OnServerPlayerDisconnected);
            }
        }

        #endregion

        #region Scene Data Methods

        /// <summary>
        ///     Check whether or not the player is in a specific scene or not.
        /// </summary>
        /// <param name="scene">The scene we want to check in.</param>
        /// <param name="player">The player we want to check for.</param>
        /// <returns>Returns true or false if the player is in the scene specified.</returns>
        public bool IsPlayerInScene(Scene scene, INetworkPlayer player)
        {
            if (!scene.IsValid())
                throw new ArgumentException("Scene is not valid", nameof(scene));

            if (!ServerSceneData.TryGetValue(scene, out var players))
            {
                throw new KeyNotFoundException($"Could not find player list for scene:{scene}");
            }

            return players.Contains(player);
        }

        /// <summary>
        ///     What scene is this specific player currently in.
        /// </summary>
        /// <param name="player">The player we want to check against.</param>
        /// <returns>Returns back a array of scene's the player is currently in.</returns>
        public Scene[] ScenesPlayerIsIn(INetworkPlayer player)
        {
            var data = new List<Scene>();
            ScenesPlayerIsInNonAlloc(player, data);
            return data.ToArray();
        }
        public void ScenesPlayerIsInNonAlloc(INetworkPlayer player, List<Scene> scenes)
        {
            foreach (var scene in _serverSceneData)
            {
                if (scene.Value.Contains(player))
                    scenes.Add(scene.Key);
            }
        }

        #endregion

        #region Client Side

        /// <summary>
        ///     Register incoming client messages.
        /// </summary>
        private void RegisterClientMessages()
        {
            Client.MessageHandler.RegisterHandler<SceneMessage>(ClientStartSceneMessage);
            Client.MessageHandler.RegisterHandler<SceneReadyMessage>(ClientFinishedLoadingSceneMessage);
            Client.MessageHandler.RegisterHandler<SceneNotReadyMessage>(ClientNotReadyMessage);
        }

        /// <summary>
        ///     Received message from server to start loading scene or scenes.
        ///
        ///     <para>Default implementation is to load main activate scene server is using and load any
        ///     other additive scenes in background and notify event handler. If this is not intended
        ///     behavior you need please override this function.</para>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="message"></param>
        public virtual void ClientStartSceneMessage(INetworkPlayer player, SceneMessage message)
        {
            ThrowIfNotClient();

            // server should load the scene instead of sending scene message
            if (Client.IsLocalClient)
                throw new InvalidOperationException("Host client should not be sent scene message");

            if (string.IsNullOrEmpty(message.MainActivateScene))
                throw new ArgumentException($"[NetworkSceneManager] - SceneLoadStartedMessage: {nameof(message.MainActivateScene)} cannot be empty or null", nameof(message));

            if (logger.LogEnabled()) logger.Log($"[NetworkSceneManager] - SceneLoadStartedMessage: changing scenes from: {ActiveScenePath} to: {message.MainActivateScene}");

            //Additive are scenes loaded on server and this client is not a host client
            if (message.AdditiveScenes != null && message.AdditiveScenes.Count > 0 && Client)
            {
                for (var sceneIndex = 0; sceneIndex < message.AdditiveScenes.Count; sceneIndex++)
                {
                    if (string.IsNullOrEmpty(message.AdditiveScenes[sceneIndex])) continue;

                    _clientPendingAdditiveSceneLoadingList.Add(message.AdditiveScenes[sceneIndex]);
                }
            }

            // Notify others that client has started to change scenes.
            OnClientStartedSceneChange?.Invoke(message.MainActivateScene, message.SceneOperation);

            LoadSceneAsync(message.MainActivateScene, sceneOperation: message.SceneOperation).Forget();
        }

        /// <summary>
        ///     Received message from server that it has finished loading the scene.
        ///
        ///     <para>Default implementation will set AllowSceneActivation = true and invoke event handler.
        ///     If this is not good enough intended behavior then override this method.</para>
        /// </summary>
        /// <param name="player">The player who sent the message.</param>
        /// <param name="message">The message data coming back from server.</param>
        protected internal virtual void ClientFinishedLoadingSceneMessage(INetworkPlayer player, SceneReadyMessage message)
        {
            logger.Log("[NetworkSceneManager] - ClientSceneReadyMessage");

            //Server has finished changing scene. Allow the client to finish.
            if (SceneLoadingAsyncOperationInfo != null)
                SceneLoadingAsyncOperationInfo.allowSceneActivation = true;
        }

        /// <summary>
        /// Called on clients when a scene has completed loading, when the scene load was initiated by the server.
        /// <para>Non-Additive Scene changes will cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkSceneManager is to add a player object for the connection if no player object exists.</para>
        /// </summary>
        /// <param name="scene">The scene that was just loaded</param>
        /// <param name="sceneOperation">Scene operation that was just  happen</param>
        internal void OnClientSceneLoadFinished(Scene scene, SceneOperation sceneOperation)
        {
            if (_clientPendingAdditiveSceneLoadingList.Count > 0 && Client && !Client.IsLocalClient)
            {
                if (string.IsNullOrEmpty(_clientPendingAdditiveSceneLoadingList[0]))
                    throw new ArgumentNullException("ClientPendingAdditiveSceneLoadingList[0]", "Some how a null scene path has been entered.");

                LoadSceneAsync(_clientPendingAdditiveSceneLoadingList[0], new[] { Client.Player }, SceneOperation.LoadAdditive).Forget();

                _clientPendingAdditiveSceneLoadingList.RemoveAt(0);

                return;
            }

            //set ready after scene change has completed
            if (!Client.Player.SceneIsReady)
                SetSceneIsReady();

            //Call event once all scene related actions (sub-scenes and ready) are done.
            Client.World.RemoveDestroyedObjects();
            ClientObjectManager.PrepareToSpawnSceneObjects();
            OnClientFinishedSceneChange?.Invoke(scene, sceneOperation);
        }

        /// <summary>
        ///     Received message that player is not ready.
        ///
        ///     <para>Default implementation is to set player to not ready.</para>
        /// </summary>
        /// <param name="player">The player that is currently not ready.</param>
        /// <param name="message">The message data coming in.</param>
        protected internal virtual void ClientNotReadyMessage(INetworkPlayer player, SceneNotReadyMessage message)
        {
            if (logger.LogEnabled())
                logger.Log("[NetworkSceneManager] - OnClientNotReadyMessageInternal");

            Client.Player.SceneIsReady = false;
        }

        /// <summary>
        /// Signal that the client connection is ready to enter the game.
        /// <para>This could be for example when a client enters an ongoing game and has finished loading the current scene. The server should respond to the message with an appropriate handler which instantiates the players object for example.</para>
        /// </summary>
        /// <exception cref="InvalidOperationException">When called with an null or disconnected client</exception>
        public void SetSceneIsReady()
        {
            ThrowIfNotClient();

            if (logger.LogEnabled()) logger.Log("[NetworkSceneManager] - Scene is loaded and ready has been called.");

            // Set these before sending the ReadyMessage, otherwise host client
            // will fail in InternalAddPlayer with null readyConnection.
            Client.Player.SceneIsReady = true;

            // Tell server we're ready to have a player object spawned
            Client.Player.Send(new SceneReadyMessage());
        }

        private void ThrowIfNotClient()
        {
            if (Client == null || !Client.IsConnected) { throw new InvalidOperationException("Method can only be called if client is active"); }
        }

        #endregion

        #region Server Side

        /// <summary>
        ///     Register incoming client messages.
        /// </summary>
        private void RegisterServerMessages()
        {
            Server.MessageHandler.RegisterHandler<SceneReadyMessage>(HandlePlayerSceneReady);
        }

        /// <summary>
        /// Loads a scene on players but NOT on server
        /// <para>Note: does not load for Host player, they should be loaded using server methods instead</para>
        /// </summary>
        /// <param name="scenePath"></param>
        /// <param name="players"></param>
        /// <param name="shouldClientLoadOrUnloadNormally"></param>
        /// <param name="sceneOperation"></param>
        /// <param name="loadSceneParameters"></param>
        public void ServerLoadForPlayers(string scenePath, IEnumerable<INetworkPlayer> players, bool shouldClientLoadOrUnloadNormally, SceneOperation sceneOperation = SceneOperation.Normal)
        {
            ThrowIfScenePathEmpty(scenePath);

            if (logger.LogEnabled()) logger.Log("[NetworkSceneManager] - ServerLoadForPlayers " + scenePath);
            SetAllClientsNotReady(players);

            SendSceneMessage(players, scenePath, shouldClientLoadOrUnloadNormally ? SceneOperation.Normal : sceneOperation);
        }

        /// <summary>
        ///     Allows server to fully load new scene or additive load in another scene.
        /// </summary>
        /// <param name="scenePath">The full path to the scenes files or the names of the scenes.</param>
        /// <param name="players">List of player's we want to send the new scene loading or unloading to.</param>
        /// <param name="shouldClientLoadOrUnloadNormally">Should client load or unload the scene in normal non additive way</param>
        /// <param name="sceneOperation">Choose type of scene loading we are doing <see cref="SceneOperation"/>.</param>
        private UniTask ServerSceneLoading(string scenePath, IEnumerable<INetworkPlayer> players, bool shouldClientLoadOrUnloadNormally, SceneOperation sceneOperation = SceneOperation.Normal, LoadSceneParameters? loadSceneParameters = null)
        {
            ThrowIfScenePathEmpty(scenePath);

            if (logger.LogEnabled()) logger.Log("[NetworkSceneManager] - ServerChangeScene " + scenePath);

            // Let server prepare for scene change
            logger.Log("[NetworkSceneManager] - OnServerChangeScene");

            SetAllClientsNotReady(players);

            OnServerStartedSceneChange?.Invoke(scenePath, sceneOperation);

            if (Server.LocalClientActive)
            {
                // notify host client that scene loading is about to start
                OnClientStartedSceneChange?.Invoke(scenePath, sceneOperation);
            }

            // Coburn, 2023-01-29: Patched the following to not use Forget UniTask functionality.
            // ALWAYS load the scene, even if we're the host.
            // return the task after sending the message
            var loadTask = LoadSceneAsync(scenePath, players, sceneOperation, loadSceneParameters);

            // Send out the message to do the change. This'll notify all clients.
            SendSceneMessage(players, scenePath, shouldClientLoadOrUnloadNormally ? SceneOperation.Normal : sceneOperation);

            return loadTask;
        }

        private void SendSceneMessage(IEnumerable<INetworkPlayer> players, string scenePath, SceneOperation sceneOperation)
        {
            // if only player if host, then return early to avoid serializing message to no one
            if (players.Count() == 1 && players.First() == Server.LocalPlayer)
                return;

            var message = new SceneMessage { MainActivateScene = scenePath, SceneOperation = sceneOperation };
            // send to players, excluding host
            Server.SendToMany(players, message, excludeLocalPlayer: true);
        }

        /// <summary>
        ///     Unload a specific scene from server and clients
        /// </summary>
        /// <param name="scene">What scene do we want to tell server and clients to unload.</param>
        /// <param name="players">The players we want to tell to unload the scene.</param>
        public void ServerSceneUnLoading(Scene scene, IEnumerable<INetworkPlayer> players)
        {
            ServerSceneUnLoadingAsync(scene, players).Forget();
        }

        /// <summary>
        ///     Unload a specific scene from server and clients
        /// </summary>
        /// <param name="scene">What scene do we want to tell server and clients to unload.</param>
        /// <param name="players">The players we want to tell to unload the scene.</param>
        public UniTask ServerSceneUnLoadingAsync(Scene scene, IEnumerable<INetworkPlayer> players)
        {
            if (!scene.IsValid())
            {
                throw new ArgumentNullException(nameof(scene),
                    "[NetworkSceneManager] - ServerChangeScene: " + nameof(scene) + " cannot be null");
            }

            if (players == null || !players.Any())
            {
                throw new ArgumentNullException(nameof(players),
                    "[NetworkSceneManager] - list of player's cannot be null or no players.");
            }

            if (logger.LogEnabled()) logger.Log("[NetworkSceneManager] - ServerChangeScene " + scene.name);

            // Let server prepare for scene change
            if (logger.LogEnabled()) logger.Log("[NetworkSceneManager] - OnServerChangeScene");

            SetAllClientsNotReady(players);

            // Notify interested references that we're doing a scene operation...
            OnServerStartedSceneChange?.Invoke(scene.path, SceneOperation.UnloadAdditive);

            // return the task after sending the message
            var unloadTask = UnLoadSceneAsync(scene, SceneOperation.UnloadAdditive);

            // notify all clients about the new scene
            SendSceneMessage(players, scene.path, SceneOperation.UnloadAdditive);

            return unloadTask;
        }

        /// <summary>
        ///     Allows server to fully load in a new scene and override current active scene.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="sceneLoadParameters">What settings should we be using for physics scene loading.</param>
        public void ServerLoadSceneNormal(string scenePath, LoadSceneParameters? sceneLoadParameters = null)
        {
            ServerLoadSceneNormalAsync(scenePath, sceneLoadParameters).Forget();
        }

        /// <summary>
        ///     Allows server to fully load in a new scene and override current active scene.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="sceneLoadParameters">What settings should we be using for physics scene loading.</param>
        public UniTask ServerLoadSceneNormalAsync(string scenePath, LoadSceneParameters? sceneLoadParameters = null)
        {
            ThrowIfNotServer();

            return ServerSceneLoading(scenePath, Server.Players, true, SceneOperation.Normal, sceneLoadParameters);
        }

        /// <summary>
        ///     Allows server to fully load in another scene on top of current active scene.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="players">Collection of player's that are receiving the new scene load.</param>
        /// <param name="shouldClientLoadNormally">Should the clients load this additively too or load it full normal scene change.</param>
        /// <param name="sceneLoadParameters">What settings should we be using for physics scene loading.</param>
        public void ServerLoadSceneAdditively(string scenePath, IEnumerable<INetworkPlayer> players, bool shouldClientLoadNormally = false, LoadSceneParameters? sceneLoadParameters = null)
        {
            ServerLoadSceneAdditivelyAsync(scenePath, players, shouldClientLoadNormally, sceneLoadParameters).Forget();
        }

        /// <summary>
        ///     Allows server to fully load in another scene on top of current active scene.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="players">Collection of player's that are receiving the new scene load.</param>
        /// <param name="shouldClientLoadNormally">Should the clients load this additively too or load it full normal scene change.</param>
        /// <param name="sceneLoadParameters">What settings should we be using for physics scene loading.</param>
        public UniTask ServerLoadSceneAdditivelyAsync(string scenePath, IEnumerable<INetworkPlayer> players, bool shouldClientLoadNormally = false, LoadSceneParameters? sceneLoadParameters = null)
        {
            ThrowIfNotServer();
            ThrowIfPlayersNull(players);

            return ServerSceneLoading(scenePath, players, shouldClientLoadNormally, SceneOperation.LoadAdditive, sceneLoadParameters);
        }

        /// <summary>
        ///     Allows server to fully unload a scene additively.
        /// </summary>
        /// <param name="scene">The scene handle which we want to unload additively.</param>
        /// <param name="players">Collection of player's that are receiving the new scene unload.</param>
        public void ServerUnloadSceneAdditively(Scene scene, IEnumerable<INetworkPlayer> players)
        {
            ServerUnloadSceneAdditivelyAsync(scene, players).Forget();
        }

        /// <summary>
        ///     Allows server to fully unload a scene additively.
        /// </summary>
        /// <param name="scene">The scene handle which we want to unload additively.</param>
        /// <param name="players">Collection of player's that are receiving the new scene unload.</param>
        public UniTask ServerUnloadSceneAdditivelyAsync(Scene scene, IEnumerable<INetworkPlayer> players)
        {
            ThrowIfNotServer();

            return ServerSceneUnLoadingAsync(scene, players);
        }

        /// <summary>
        ///     When player authenticates to server we send a message to them to load up main scene and
        ///     any other scenes that are loaded on server.
        ///
        ///     <para>Default implementation takes main activate scene as main and any other loaded scenes and sends it to player's
        ///     Please override this function if this is not intended behavior for you.</para>
        /// </summary>
        /// <param name="player">The current player that finished authenticating.</param>
        protected internal virtual void OnServerAuthenticated(INetworkPlayer player)
        {
            logger.Log("[NetworkSceneManager] - OnServerAuthenticated");

            // dont need to load scenes for host player (they already have them loadeed becuase they are the serve)
            if (Server.LocalPlayer == player)
            {
                HostPlayerAuthenticated();
                return;
            }

            SetClientNotReady(player);

            SceneMessage sceneMessage;
            if (SendAdditiveScenesOnAuthenticate)
            {
                //Client should receive all open additive scenes, as server will have multiple for them
                var additiveScenes = GetAdditiveScenes();
                sceneMessage = new SceneMessage { MainActivateScene = ActiveScenePath, AdditiveScenes = additiveScenes };
            }
            else
            {
                //Client will not be receiving additive scenes, no need to allocate garbage via GetAdditiveScenes list
                sceneMessage = new SceneMessage { MainActivateScene = ActiveScenePath };
            }

            player.Send(sceneMessage);
            player.Send(new SceneReadyMessage());
        }

        /// <summary>
        /// for host, server loads scene not client, so we need special handling for host client so that other components like CharacterSpawner work correctly
        /// </summary>
        private void HostPlayerAuthenticated()
        {
            // nornal client invokes start, then finish, even if scene is already loaded
            OnClientStartedSceneChange?.Invoke(ActiveScenePath, SceneOperation.Normal);

            // server server and client copy of hoost player as ready
            Server.LocalClient.Player.SceneIsReady = true;
            Server.LocalPlayer.SceneIsReady = true;

            OnClientFinishedSceneChange?.Invoke(SceneManager.GetActiveScene(), SceneOperation.Normal);
        }

        private static List<string> GetAdditiveScenes()
        {
            var additiveScenes = new List<string>(SceneManager.sceneCount - 1);

            // add all scenes except active to additive list
            var activeScene = SceneManager.GetActiveScene();
            for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                var scene = SceneManager.GetSceneAt(sceneIndex);
                if (scene != activeScene)
                {
                    additiveScenes.Add(scene.path);
                }
            }

            return additiveScenes;
        }

        /// <summary>
        ///     Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ChangeServerScene().
        /// </summary>
        /// <param name="scene">The new scene.</param>
        /// <param name="operation">The type of scene loading we want to do.</param>
        internal void OnServerFinishedSceneLoad(Scene scene, SceneOperation operation)
        {
            logger.Log(" [NetworkSceneManager] - OnServerSceneChanged");

            // todo this might cause issues if only some players are loading scene
            // send to all, excluding host
            Server.SendToAll(new SceneReadyMessage(), excludeLocalPlayer: true);

            // clean up and invoke server functions before user events
            Server.World.RemoveDestroyedObjects();
            ServerObjectManager.SpawnOrActivate();

            OnServerFinishedSceneChange?.Invoke(scene, operation);
        }

        /// <summary>
        ///     When player disconnects from server we will need to clean up info in scenes related to user.
        /// <para>Default implementation we loop through list of scenes and find where this player was in and removed them from list.</para>
        /// </summary>
        /// <param name="disconnectedPlayer"></param>
        protected internal virtual void OnServerPlayerDisconnected(INetworkPlayer disconnectedPlayer)
        {
            foreach (var playersInScene in _serverSceneData.Values)
            {
                playersInScene.Remove(disconnectedPlayer);
            }
        }

        /// <summary>
        /// default ready handler. called on server
        /// </summary>
        /// <param name="player"></param>
        /// <param name="msg"></param>
        private void HandlePlayerSceneReady(INetworkPlayer player, SceneReadyMessage msg)
        {
            if (logger.LogEnabled()) logger.Log("Default handler for ready message from " + player);

            player.SceneIsReady = true;

            // invoke server functions before user events
            ServerObjectManager.SpawnVisibleObjects(player, false, (HashSet<NetworkIdentity>)null);
            OnPlayerSceneReady.Invoke(player);
        }

        /// <summary>
        /// Marks all connected clients as no longer ready.
        /// <para>
        ///     All clients will no longer be sent state synchronization updates.
        ///     The player's clients can call ClientManager.Ready() again to re-enter the ready state.
        ///     This is useful when switching scenes.
        /// </para>
        /// </summary>
        public void SetAllClientsNotReady(IEnumerable<INetworkPlayer> players = null)
        {
            ThrowIfNotServer();

            foreach (var player in players ?? Server.Players)
            {
                SetClientNotReady(player);
            }
        }

        /// <summary>
        /// Sets a player as not ready and removes all visible objects
        /// <para>Players that are not ready will not be sent spawn message or state updates.</para>
        /// <para>Players that are not ready do not receive spawned objects or state synchronization updates. They client can be made ready again by calling SetClientReady().</para>
        /// </summary>
        /// <param name="player">The player to make not ready.</param>
        public void SetClientNotReady(INetworkPlayer player)
        {
            ThrowIfNotServer();

            if (player.SceneIsReady)
            {
                if (logger.LogEnabled()) logger.Log("PlayerNotReady " + player);
                player.SceneIsReady = false;
                player.RemoveAllVisibleObjects();

                player.Send(new SceneNotReadyMessage());
            }
        }

        private void ThrowIfScenePathEmpty(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                throw new ArgumentNullException(nameof(scenePath),
                    "[NetworkSceneManager] - ServerChangeScene: " + nameof(scenePath) + " cannot be empty or null");
            }
        }

        private void ThrowIfNotServer()
        {
            if (Server == null || !Server.Active)
                throw new InvalidOperationException("Method can only be called if server is active");
        }

        private void ThrowIfPlayersNull(IEnumerable<INetworkPlayer> players)
        {
            if (players == null)
                throw new ArgumentNullException(nameof(players), "No player's were added to send for information");
        }

        #endregion

        #region Scene Operations

        /// <summary>
        ///     Finish loading the scene.
        /// </summary>
        /// <param name="scene">The scene that is finishing.</param>
        /// <param name="sceneOperation">Choose type of scene loading we are doing <see cref="SceneOperation"/>.</param>
        /// <param name="players">List of players we are adding to this scene.</param>
        internal void CompleteLoadingScene(Scene scene, SceneOperation sceneOperation, IEnumerable<INetworkPlayer> players = null)
        {
            // If server mode call this to make sure scene finishes loading
            if (Server && Server.Active)
            {
                if (logger.LogEnabled())
                {
                    logger.Log("[NetworkSceneManager] - Host: " + sceneOperation + " operation for scene: " +
                               scene.path);
                }

                // call OnServerSceneChanged
                OnServerFinishedSceneLoad(scene, sceneOperation);

                if (_serverSceneData.ContainsKey(scene))
                {
                    // Check to make sure this scene was not already loaded. If it was let's clear old data on it.
                    logger.Log(
                        $"[NetworkSceneManager] - Scene load operation: {SceneOperation.Normal}. Scene was already loaded once before. Clearing scene related data in {_serverSceneData}.");

                    _serverSceneData.Remove(scene);
                }

                _serverSceneData.Add(scene, new HashSet<INetworkPlayer>(players ?? Server.Players));
            }

            // If client let's call this to finish client scene loading too
            if (Client && Client.Active)
            {
                if (logger.LogEnabled())
                {
                    logger.Log("[NetworkSceneManager] - Client: " + sceneOperation + " operation for scene: " +
                               scene.path);
                }

                OnClientSceneLoadFinished(scene, sceneOperation);
            }
        }

        /// <summary>
        ///     Internal usage to apply unloading a scene correctly. Other api will be available to override things.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="sceneOperation">Choose type of scene loading we are doing <see cref="SceneOperation"/>.</param>
        /// <returns></returns>
        private UniTask UnLoadSceneAsync(Scene scenePath, SceneOperation sceneOperation)
        {
            switch (sceneOperation)
            {
                case SceneOperation.UnloadAdditive:
                    return UnLoadSceneAdditiveAsync(scenePath);
                default:
                    throw new InvalidEnumArgumentException(nameof(sceneOperation), (int)sceneOperation,
                        typeof(SceneOperation));
            }
        }

        /// <summary>
        ///     Internal usage to apply loading a scene correctly. Other api will be available to override things.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="players">List of player's we want to track which scene they are in.</param>
        /// <param name="sceneOperation">Choose type of scene loading we are doing <see cref="SceneOperation"/>.</param>
        /// <param name="sceneLoadParameters">What settings should we be using for physics scene loading.</param>
        private UniTask LoadSceneAsync(string scenePath, IEnumerable<INetworkPlayer> players = null, SceneOperation sceneOperation = SceneOperation.Normal, LoadSceneParameters? sceneLoadParameters = null)
        {
            switch (sceneOperation)
            {
                case SceneOperation.Normal:
                    return LoadSceneNormalAsync(scenePath, sceneLoadParameters);
                case SceneOperation.LoadAdditive:
                    return LoadSceneAdditiveAsync(scenePath, players, sceneLoadParameters);
                case SceneOperation.UnloadAdditive:
                    return UnLoadSceneAdditiveAsync(scenePath);
                default:
                    throw new InvalidEnumArgumentException(nameof(sceneOperation), (int)sceneOperation,
                        typeof(SceneOperation));
            }
        }

        /// <summary>
        ///     Load our scene up in a normal unity fashion.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="sceneLoadParameters">What settings should we be using for physics scene loading.</param>
        private async UniTask LoadSceneNormalAsync(string scenePath, LoadSceneParameters? sceneLoadParameters = null)
        {
            //Scene is already active.
            if (ActiveScenePath.Equals(scenePath))
            {
                CompleteLoadingScene(SceneManager.GetActiveScene(), SceneOperation.Normal);
            }
            else
            {
                SceneLoadingAsyncOperationInfo = sceneLoadParameters.HasValue
                    ? SceneManager.LoadSceneAsync(scenePath, sceneLoadParameters.Value)
                    : SceneManager.LoadSceneAsync(scenePath);

                if (SceneLoadingAsyncOperationInfo == null)
                    throw new InvalidOperationException($"SceneManager failed to load scene with path: {scenePath}");

                //If non host client. Wait for server to finish scene change
                if (Client && Client.Active && !Client.IsLocalClient)
                {
                    SceneLoadingAsyncOperationInfo.allowSceneActivation = false;
                }

                await SceneLoadingAsyncOperationInfo.ToUniTask();
                AssertSceneIsActive(scenePath);

                CompleteLoadingScene(SceneManager.GetActiveScene(), SceneOperation.Normal);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void AssertSceneIsActive(string scenePath)
        {
            // equal to path or name of active scene
            logger.Assert(scenePath == ActiveScenePath || scenePath == SceneManager.GetActiveScene().name, "[NetworkSceneManager] - Scene being loaded was not the active scene");
        }

        /// <summary>
        ///     Load our scene additively.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="players">The list of players we want to track to know what scene they are on.</param>
        /// <param name="sceneLoadParameters">What settings should we be using for physics scene loading.</param>
        private async UniTask LoadSceneAdditiveAsync(string scenePath, IEnumerable<INetworkPlayer> players = null, LoadSceneParameters? sceneLoadParameters = null)
        {
            SceneLoadingAsyncOperationInfo = sceneLoadParameters.HasValue
                ? SceneManager.LoadSceneAsync(scenePath, sceneLoadParameters.Value)
                : SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

            await SceneLoadingAsyncOperationInfo.ToUniTask();

            var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            if (Server && Server.Active)
            {
                _serverSceneData.Add(scene, new HashSet<INetworkPlayer>(players ?? Server.Players));
            }

            CompleteLoadingScene(scene, SceneOperation.LoadAdditive, players);
        }

        /// <summary>
        ///     Unload our scene additively.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <returns></returns>
        private async UniTask UnLoadSceneAdditiveAsync(string scenePath)
        {
            // Ensure additive scene is actually loaded
            var scene = SceneManager.GetSceneByPath(scenePath);
            if (scene.IsValid())
            {
                await SceneManager.UnloadSceneAsync(scenePath, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects).ToUniTask();

                CompleteLoadingScene(scene, SceneOperation.UnloadAdditive);
            }
            else
            {
                logger.LogWarning($"Cannot unload {scenePath} with UnloadAdditive operation");
            }
        }

        /// <summary>
        ///     Unload our scene additively.
        /// </summary>
        /// <param name="scene">The scene data handle to know which scene to unload in case of multiple scenes of same name / path.</param>
        private async UniTask UnLoadSceneAdditiveAsync(Scene scene)
        {
            // Ensure additive scene is actually loaded
            if (scene.IsValid())
            {
                await SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects).ToUniTask();

                CompleteLoadingScene(scene, SceneOperation.UnloadAdditive);
            }
            else
            {
                logger.LogWarning($"[NetworkSceneManager] - Cannot unload {scene} with UnloadAdditive operation");
            }

            if (Server && Server.Active)
            {
                _serverSceneData.Remove(scene);
            }
        }

        /// <summary>
        ///     Let's us get scene by full path or by its name.
        /// </summary>
        /// <param name="scenePath">The path or name representing the scene.</param>
        /// <returns>Returns back correct scene data.</returns>
        public Scene GetSceneByPathOrName(string scenePath)
        {
            var scene = SceneManager.GetSceneByPath(scenePath);

            return !string.IsNullOrEmpty(scene.name) ? scene : SceneManager.GetSceneByName(scenePath);
        }

        #endregion
    }
}
