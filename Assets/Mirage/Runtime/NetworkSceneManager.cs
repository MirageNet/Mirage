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
    [DisallowMultipleComponent]
    public class NetworkSceneManager : MonoBehaviour, INetworkSceneManager
    {
        #region Fields

        static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkSceneManager));

        [Header("Setup Settings")]

        [FormerlySerializedAs("client")]
        public NetworkClient Client;

        [FormerlySerializedAs("server")]
        public NetworkServer Server;

        /// <summary>
        ///     Sets the NetworksSceneManagers GameObject to DontDestroyOnLoad. Default = true.
        /// </summary>
        public bool DontDestroy = true;

        protected readonly Dictionary<Scene, List<INetworkPlayer>> ServerSceneData = new Dictionary<Scene, List<INetworkPlayer>>();

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
        internal readonly List<string> ClientPendingAdditiveSceneLoadingList = new List<string>();

        /// <summary>
        ///     Information on any scene that is currently being loaded.
        /// </summary>
        public AsyncOperation SceneLoadingAsyncOperationInfo;

        #endregion

        #region Events

        [Header("Events")]

        [FormerlySerializedAs("ClientChangeScene")]
        [SerializeField] SceneChangeEvent _clientStartedSceneChange = new SceneChangeEvent();

        [FormerlySerializedAs("ClientSceneChanged")]
        [SerializeField] SceneChangeEvent _clientFinishedSceneChange = new SceneChangeEvent();

        [FormerlySerializedAs("ServerChangeScene")]
        [SerializeField] SceneChangeEvent _serverStartedSceneChange = new SceneChangeEvent();

        [FormerlySerializedAs("ServerSceneChanged")]
        [SerializeField] SceneChangeEvent _serverFinishedSceneChange = new SceneChangeEvent();

        /// <summary>
        /// Event fires when the Client starts changing scene.
        /// </summary>
        public SceneChangeEvent ClientStartedSceneChange => _clientStartedSceneChange;

        /// <summary>
        /// Event fires after the Client has completed its scene change.
        /// </summary>
        public SceneChangeEvent ClientFinishedSceneChange => _clientFinishedSceneChange;

        /// <summary>
        /// Event fires before Server changes scene.
        /// </summary>
        public SceneChangeEvent ServerStartedSceneChange => _serverStartedSceneChange;

        /// <summary>
        /// Event fires after Server has completed scene change.
        /// </summary>
        public SceneChangeEvent ServerFinishedSceneChange => _serverFinishedSceneChange;

        #endregion

        #region Client Side

        /// <summary>
        ///     Register incoming client messages.
        /// </summary>
        private void RegisterClientMessages()
        {
            Client.MessageHandler.RegisterHandler<SceneLoadStartedMessage>(ClientStartSceneMessage);

            if (Client.IsLocalClient) return;

            Client.MessageHandler.RegisterHandler<SceneLoadFinishedMessage>(ClientFinishedLoadingSceneMessage);
            Client.MessageHandler.RegisterHandler<PlayerNotReadyMessage>(ClientNotReadyMessage);
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
        public virtual void ClientStartSceneMessage(INetworkPlayer player, SceneLoadStartedMessage message)
        {
            if (!Client.IsConnected)
                throw new InvalidOperationException("[NetworkSceneManager] - SceneLoadStartedMessage: cannot change network scene while client is disconnected");

            if (string.IsNullOrEmpty(message.MainActivateScene))
                throw new ArgumentNullException(nameof(message.MainActivateScene), $"[NetworkSceneManager] - SceneLoadStartedMessage: {nameof(message.MainActivateScene)} cannot be empty or null");

            if (logger.LogEnabled()) logger.Log($"[NetworkSceneManager] - SceneLoadStartedMessage: changing scenes from: {ActiveScenePath} to: {message.MainActivateScene}");

            //Additive are scenes loaded on server and this client is not a host client
            if (message.AdditiveScenes != null && message.AdditiveScenes.Length > 0 && Client && !Client.IsLocalClient)
            {
                for (int index = message.AdditiveScenes.Length - 1; index >= 0; index--)
                {
                    string scene = message.AdditiveScenes[index];

                    ClientPendingAdditiveSceneLoadingList.Add(scene);
                }
            }

            // Notify others that client has started to change scenes.
            ClientStartedSceneChange?.Invoke(message.MainActivateScene, message.SceneOperation);

            LoadSceneAsync(message.MainActivateScene, message.MovePlayer, message.SceneOperation).Forget();
        }

        /// <summary>
        ///     Received message from server that it has finished loading the scene.
        ///
        ///     <para>Default implementation will set AllowSceneActivation = true and invoke event handler.
        ///     If this is not good enough intended behavior then override this method.</para>
        /// </summary>
        /// <param name="player">The player who sent the message.</param>
        /// <param name="message">The message data coming back from server.</param>
        public virtual void ClientFinishedLoadingSceneMessage(INetworkPlayer player, SceneLoadFinishedMessage message)
        {
            logger.Log("[NetworkSceneManager] - ClientSceneReadyMessage");

            //Server has finished changing scene. Allow the client to finish.
            if (SceneLoadingAsyncOperationInfo != null)
                SceneLoadingAsyncOperationInfo.allowSceneActivation = true;

            ClientFinishedSceneChange.Invoke(message.ScenePath, message.SceneOperation);
        }

        /// <summary>
        /// Called on clients when a scene has completed loading, when the scene load was initiated by the server.
        /// <para>Non-Additive Scene changes will cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkSceneManager is to add a player object for the connection if no player object exists.</para>
        /// </summary>
        /// <param name="scenePath">Path of the scene that was just loaded</param>
        /// <param name="movePlayer">Whether or not we should move the player.</param>
        /// <param name="sceneOperation">Scene operation that was just  happen</param>
        internal void OnClientSceneLoadFinished(string scenePath, bool movePlayer, SceneOperation sceneOperation)
        {
            if (ClientPendingAdditiveSceneLoadingList.Count > 0 && Client && !Client.IsLocalClient)
            {
                LoadSceneAsync(ClientPendingAdditiveSceneLoadingList[0], movePlayer, SceneOperation.LoadAdditive).Forget();

                ClientPendingAdditiveSceneLoadingList.RemoveAt(0);
                return;
            }

            //set ready after scene change has completed
            if (!Client.Player.SceneIsReady)
                SetSceneIsReady();

            //Call event once all scene related actions (subscenes and ready) are done.
            ClientFinishedSceneChange?.Invoke(scenePath, sceneOperation);
        }

        /// <summary>
        ///     Received message that player is not ready.
        ///
        ///     <para>Default implementation is to set player to not ready.</para>
        /// </summary>
        /// <param name="player">The player that is currently not ready.</param>
        /// <param name="message">The message data coming in.</param>
        protected virtual void ClientNotReadyMessage(INetworkPlayer player, PlayerNotReadyMessage message)
        {
            logger.Log("[NetworkSceneManager] - OnClientNotReadyMessageInternal");

            Client.Player.SceneIsReady = false;
        }

        /// <summary>
        /// Signal that the client connection is ready to enter the game.
        /// <para>This could be for example when a client enters an ongoing game and has finished loading the current scene. The server should respond to the message with an appropriate handler which instantiates the players object for example.</para>
        /// </summary>
        /// <exception cref="InvalidOperationException">When called with an null or disconnected client</exception>
        public void SetSceneIsReady( )
        {
            if (!Client || !Client.Active)
                throw new InvalidOperationException("[NetworkSceneManager] - Ready() called with an null or disconnected client");

            if (logger.LogEnabled()) logger.Log("[NetworkSceneManager] - ClientScene.Ready() called.");

            // Set these before sending the ReadyMessage, otherwise host client
            // will fail in InternalAddPlayer with null readyConnection.
            Client.Player.SceneIsReady = true;

            // Tell server we're ready to have a player object spawned
            Client.Player.Send(new SceneLoadFinishedMessage());
        }

        #endregion

        #region Server Side

        /// <summary>
        ///     Creates a new physics scene on server and transfer specific player's to that scene.
        /// </summary>
        /// <param name="scenePath">The scene to use as base for the new physics scene.</param>
        /// <param name="sceneParameters">The scene parameter data we want to use.</param>
        public virtual async void CreatePhysicsScene(string scenePath, LoadSceneParameters sceneParameters, IEnumerable<INetworkPlayer> players)
        {
            await SceneManager.LoadSceneAsync(scenePath, sceneParameters);

            // Since this is new scene loading this should be count -1 to get newest scene?
            IEnumerable<INetworkPlayer> networkPlayers = players as INetworkPlayer[] ?? players.ToArray();
            Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            ServerSceneData.Add(SceneManager.GetSceneAt(SceneManager.sceneCount - 1),
                new List<INetworkPlayer>(networkPlayers));

            NetworkServer.SendToMany(networkPlayers, new SceneLoadStartedMessage {MainActivateScene = newScene.path, SceneOperation = SceneOperation.LoadAdditive});
        }

        /// <summary>
        ///     Creates a new physics scene on server but does not transfer any player's to it.
        /// </summary>
        /// <param name="scenePath">The scene to use as base for the new physics scene.</param>
        /// <param name="sceneParameters">The scene parameter data we want to use.</param>
        public void CreatePhysicsScene(string scenePath, LoadSceneParameters sceneParameters)
        {
            SceneManager.LoadSceneAsync(scenePath, sceneParameters);
        }

        /// <summary>
        ///     Allows server to fully load new scene or additive load in another scene.
        /// </summary>
        /// <param name="scenePath">The full path to the scenes files or the names of the scenes.</param>
        /// <param name="sceneOperation">Choose type of scene loading we are doing <see cref="SceneOperation"/>.</param>
        public void ChangeServerScene(string scenePath, SceneOperation sceneOperation = SceneOperation.Normal)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                throw new ArgumentNullException(nameof(scenePath), "[NetworkSceneManager] - ServerChangeScene: " + nameof(scenePath) + " cannot be empty or null");
            }

            if (logger.LogEnabled()) logger.Log("[NetworkSceneManager] - ServerChangeScene " + scenePath);

            // Let server prepare for scene change
            logger.Log("[NetworkSceneManager] - OnServerChangeScene");

            ServerStartedSceneChange?.Invoke(scenePath, sceneOperation);

            if (!Server.LocalClientActive)
                LoadSceneAsync(scenePath, false, sceneOperation).Forget();

            // notify all clients about the new scene
            Server.SendToAll(new SceneLoadStartedMessage { MainActivateScene = scenePath, SceneOperation = sceneOperation });

            // Server is loading full new scene let's clear scene data.
            ServerSceneData.Clear();

            // Add all player's to new scene data player's should be part of.
            ServerSceneData.Add(GetSceneByPathOrName(scenePath), new List<INetworkPlayer>(Server.Players));
        }

        /// <summary>
        ///     When player authenticates to server we send a message to them to load up main scene and
        ///     any other scenes that are loaded on server.
        ///
        ///     <para>Default implementation takes main activate scene as main and any other loaded scenes and sends it to player's
        ///     Please override this function if this is not intended behavior for you.</para>
        /// </summary>
        /// <param name="player">The current player that finished authenticating.</param>
        public virtual void OnServerAuthenticated(INetworkPlayer player)
        {
            logger.Log("[NetworkSceneManager] - OnServerAuthenticated");

            string[] scenesPaths = new string[SceneManager.sceneCount];

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);

                scenesPaths[sceneIndex] = scene.path;

                if (ServerSceneData.ContainsKey(scene))
                {
                    if (!ServerSceneData[scene].Contains(player))
                        ServerSceneData[scene].Add(player);
                }
                else
                    ServerSceneData.Add(scene, new List<INetworkPlayer> { player });
            }

            player.Send(new SceneLoadStartedMessage { MainActivateScene = ActiveScenePath, AdditiveScenes = scenesPaths });
            player.Send(new PlayerReadyMessage());
        }

        /// <summary>
        ///     Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ChangeServerScene().
        /// </summary>
        /// <param name="scenePath">The name of the new scene.</param>
        /// <param name="operation">The type of scene loading we want to do.</param>
        internal void OnServerFinishedSceneLoad(string scenePath, SceneOperation operation)
        {
            logger.Log(" [NetworkSceneManager] - OnServerSceneChanged");

            Server.SendToAll(new SceneLoadFinishedMessage());

            ServerFinishedSceneChange?.Invoke(scenePath, operation);
        }

        #endregion

        #region Unity Methods

        public virtual void Start()
        {
            if (DontDestroy)
                DontDestroyOnLoad(gameObject);

            if (Client != null)
                Client.Started.AddListener(RegisterClientMessages);

            if (Server != null)
                Server.Authenticated.AddListener(OnServerAuthenticated);
        }

        public virtual void OnDestroy()
        {
            if (Client != null)
                Client.Started.RemoveListener(RegisterClientMessages);

            if (Server != null)
                Server.Authenticated.RemoveListener(OnServerAuthenticated);
        }

        #endregion

        #region Scene Operations

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scenePath"></param>
        /// <param name="sceneOperation"></param>
        internal void CompleteLoadingScene(string scenePath, bool movePlayer, SceneOperation sceneOperation)
        {
            if (movePlayer)
            {
                switch (sceneOperation)
                {
                    case SceneOperation.LoadAdditive:
                    case SceneOperation.Normal:
                        if (GetSceneByPathOrName(scenePath) != SceneManager.GetActiveScene() && Client)
                            SceneManager.MoveGameObjectToScene(Client.Player.Identity.gameObject,
                                GetSceneByPathOrName(scenePath));
                        break;
                    case SceneOperation.UnloadAdditive:
                        if (Server && Server.Active)
                        {
                            ServerSceneData.Remove(GetSceneByPathOrName(scenePath));
                        }
                        else
                        {
                            // Send player to active scene due to unloading of this scene.
                            SceneManager.MoveGameObjectToScene(Client.Player.Identity.gameObject, SceneManager.GetActiveScene());
                        }
                        break;
                }
            }

            // host mode?
            if (Client && Client.IsLocalClient)
            {
                if (logger.LogEnabled())
                    logger.Log("[NetworkSceneManager] - Host: " + sceneOperation + " operation for scene: " +
                               scenePath);

                // call OnServerSceneChanged
                OnServerFinishedSceneLoad(scenePath, sceneOperation);

                if (Client.IsConnected)
                {
                    // let client know that we changed scene
                    OnClientSceneLoadFinished(scenePath, movePlayer, sceneOperation);
                }
            }
            // server-only mode?
            else if (Server && Server.Active)
            {
                if (logger.LogEnabled())
                    logger.Log("[NetworkSceneManager] - Server: " + sceneOperation + " operation for scene: " +
                               scenePath);

                OnServerFinishedSceneLoad(scenePath, sceneOperation);
            }
            // client-only mode?
            else if (Client && Client.Active && !Client.IsLocalClient)
            {
                if (logger.LogEnabled())
                    logger.Log("[NetworkSceneManager] - Client: " + sceneOperation + " operation for scene: " +
                               scenePath);

                OnClientSceneLoadFinished(scenePath, movePlayer, sceneOperation);
            }
        }

        /// <summary>
        ///     Internal usage to apply loading a scene correctly. Other api will be available to override things.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="movePlayer">Whether or not we should move the player.</param>
        /// <param name="sceneOperation">Choose type of scene loading we are doing <see cref="SceneOperation"/>.</param>
        private UniTask LoadSceneAsync(string scenePath, bool movePlayer, SceneOperation sceneOperation = SceneOperation.Normal)
        {
            return sceneOperation switch
            {
                SceneOperation.Normal => LoadSceneNormalAsync(scenePath, movePlayer),
                SceneOperation.LoadAdditive => LoadSceneAdditiveAsync(scenePath, movePlayer),
                SceneOperation.UnloadAdditive => UnLoadSceneAdditiveAsync(scenePath, movePlayer),
                _ => throw new InvalidEnumArgumentException(nameof(sceneOperation), (int)sceneOperation,
                    typeof(SceneOperation))
            };
        }

        /// <summary>
        ///     Load our scene up in a normal unity fashion.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="movePlayer">Whether or not we should move the player.</param>
        /// <param name="sceneOperation">Choose type of scene loading we are doing <see cref="SceneOperation"/>.</param>
        public virtual async UniTask LoadSceneNormalAsync(string scenePath, bool movePlayer, SceneOperation sceneOperation = SceneOperation.Normal)
        {
            //Scene is already active.
            if (ActiveScenePath.Equals(scenePath))
            {
                CompleteLoadingScene(scenePath, movePlayer, SceneOperation.Normal);
            }
            else
            {
                SceneLoadingAsyncOperationInfo = SceneManager.LoadSceneAsync(scenePath);

                //If non host client. Wait for server to finish scene change
                if (Client && Client.Active && !Client.IsLocalClient)
                {
                    SceneLoadingAsyncOperationInfo.allowSceneActivation = false;
                }

                await SceneLoadingAsyncOperationInfo;

                logger.Assert(scenePath == ActiveScenePath, "[NetworkSceneManager] - Scene being loaded was not the active scene");

                CompleteLoadingScene(ActiveScenePath, movePlayer, SceneOperation.Normal);
            }
        }

        /// <summary>
        ///     Load our scene additively.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="movePlayer">Whether or not we should move the player.</param>
        /// <param name="sceneOperation">Choose type of scene loading we are doing <see cref="SceneOperation"/>.</param>
        public virtual async UniTask LoadSceneAdditiveAsync(string scenePath, bool movePlayer, SceneOperation sceneOperation = SceneOperation.Normal)
        {
            // Ensure additive scene is not already loaded
            if (SceneManager.GetSceneByPath(scenePath).IsValid())
            {
                logger.LogWarning($"[NetworkSceneManager] - Scene {scenePath} is already loaded");
            }
            else
            {
                await SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

                ClientPendingAdditiveSceneLoadingList.Add(scenePath);

                CompleteLoadingScene(scenePath, movePlayer, SceneOperation.LoadAdditive);
            }
        }

        /// <summary>
        ///     Unload our scene additively.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="movePlayer">Whether or not we should move the player.</param>
        /// <param name="sceneOperation">Choose type of scene loading we are doing <see cref="SceneOperation"/>.</param>
        public virtual async UniTask UnLoadSceneAdditiveAsync(string scenePath, bool movePlayer, SceneOperation sceneOperation = SceneOperation.Normal)
        {
            // Ensure additive scene is actually loaded
            if (SceneManager.GetSceneByPath(scenePath).IsValid())
            {
                await SceneManager.UnloadSceneAsync(scenePath, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

                ClientPendingAdditiveSceneLoadingList.Remove(scenePath);

                CompleteLoadingScene(scenePath, movePlayer, SceneOperation.UnloadAdditive);
            }
            else
            {
                logger.LogWarning($"[NetworkSceneManager] - Cannot unload {scenePath} with UnloadAdditive operation");
            }
        }

        /// <summary>
        ///     Let's us get scene by full path or by its name.
        /// </summary>
        /// <param name="scenePath">The path or name representing the scene.</param>
        /// <returns>Returns back correct scene data.</returns>
        public Scene GetSceneByPathOrName(string scenePath)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);

            return !string.IsNullOrEmpty(scene.name) ? scene : SceneManager.GetSceneByName(scenePath);
        }

        #endregion
    }
}
