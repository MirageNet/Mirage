using System;
using System.Collections.Generic;
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

        public readonly Dictionary<Scene, List<INetworkPlayer>> ServerSceneData = new Dictionary<Scene, List<INetworkPlayer>>();

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
            Client.MessageHandler.RegisterHandler<SceneMessage>(ClientStartSceneMessage);

            if (Client.IsLocalClient) return;

            Client.MessageHandler.RegisterHandler<SceneReadyMessage>(ClientFinishedLoadingSceneMessage);
            Client.MessageHandler.RegisterHandler<NotReadyMessage>(ClientNotReadyMessage);
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

            LoadSceneAsync(message.MainActivateScene, new[] {player}, message.SceneOperation).Forget();
        }

        /// <summary>
        ///     Received message from server that it has finished loading the scene.
        ///
        ///     <para>Default implementation will set AllowSceneActivation = true and invoke event handler.
        ///     If this is not good enough intended behavior then override this method.</para>
        /// </summary>
        /// <param name="player">The player who sent the message.</param>
        /// <param name="message">The message data coming back from server.</param>
        public virtual void ClientFinishedLoadingSceneMessage(INetworkPlayer player, SceneReadyMessage message)
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
        /// <param name="scenePath">Path of the scene that was just loaded</param>
        /// <param name="sceneOperation">Scene operation that was just  happen</param>
        internal void OnClientSceneLoadFinished(string scenePath, SceneOperation sceneOperation)
        {
            if (ClientPendingAdditiveSceneLoadingList.Count > 0 && Client && !Client.IsLocalClient)
            {
                LoadSceneAsync(ClientPendingAdditiveSceneLoadingList[0], new[] {Client.Player}, SceneOperation.LoadAdditive).Forget();

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
        public virtual void ClientNotReadyMessage(INetworkPlayer player, NotReadyMessage message)
        {
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
            if (!Client || !Client.Active)
                throw new InvalidOperationException("[NetworkSceneManager] - Ready() called with an null or disconnected client");

            if (logger.LogEnabled()) logger.Log("[NetworkSceneManager] - ClientScene.Ready() called.");

            // Set these before sending the ReadyMessage, otherwise host client
            // will fail in InternalAddPlayer with null readyConnection.
            Client.Player.SceneIsReady = true;

            // Tell server we're ready to have a player object spawned
            Client.Player.Send(new ReadyMessage());
        }

        #endregion

        #region Server Side

        /// <summary>
        ///     Allows server to fully load new scene or additive load in another scene.
        /// </summary>
        /// <param name="scenePath">The full path to the scenes files or the names of the scenes.</param>
        /// <param name="players">List of player's we want to send the new scene loading or unloading to.</param>
        /// <param name="shouldClientLoadOrUnloadNormally">Should client load or unload the scene in normal non additive way</param>
        /// <param name="sceneOperation">Choose type of scene loading we are doing <see cref="SceneOperation"/>.</param>
        private void ServerSceneLoading(string scenePath, INetworkPlayer[] players,
            bool shouldClientLoadOrUnloadNormally, SceneOperation sceneOperation = SceneOperation.Normal)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                throw new ArgumentNullException(nameof(scenePath),
                    "[NetworkSceneManager] - ServerChangeScene: " + nameof(scenePath) + " cannot be empty or null");
            }

            //if(players == null || players.Length == 0)
            //    throw new ArgumentNullException(nameof(scenePath),
            //        "[NetworkSceneManager] - list of player's cannot be null or no players.");


            if (logger.LogEnabled()) logger.Log("[NetworkSceneManager] - ServerChangeScene " + scenePath);

            // Let server prepare for scene change
            logger.Log("[NetworkSceneManager] - OnServerChangeScene");

            ServerStartedSceneChange?.Invoke(scenePath, sceneOperation);

            if (!Server.LocalClientActive)
                LoadSceneAsync(scenePath, players, sceneOperation).Forget();

            // notify all clients about the new scene
            if (players.Length > 0 && shouldClientLoadOrUnloadNormally)
                NetworkServer.SendToMany(players,
                    new SceneMessage {MainActivateScene = scenePath, SceneOperation = SceneOperation.Normal});
            else
            {
                NetworkServer.SendToMany(players,
                    new SceneMessage {MainActivateScene = scenePath, SceneOperation = sceneOperation});
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="players"></param>
        /// <param name="sceneOperation"></param>
        private void ServerSceneUnLoading(Scene scene, INetworkPlayer[] players, SceneOperation sceneOperation)
        {
            if(scene.handle == 0)
                throw new ArgumentNullException(nameof(scene),
                    "[NetworkSceneManager] - ServerChangeScene: " + nameof(scene) + " cannot be null");

            if (players == null || players.Length == 0)
                throw new ArgumentNullException(nameof(scene),
                    "[NetworkSceneManager] - list of player's cannot be null or no players.");

            if (logger.LogEnabled()) logger.Log("[NetworkSceneManager] - ServerChangeScene " + scene.name);

            // Let server prepare for scene change
            logger.Log("[NetworkSceneManager] - OnServerChangeScene");

            ServerStartedSceneChange?.Invoke(scene.path, sceneOperation);

            if (!Server.LocalClientActive)
                UnLoadSceneAsync(scene, sceneOperation).Forget();

            // notify all clients about the new scene
            if (players.Length > 0)
                NetworkServer.SendToMany(players,
                    new SceneMessage { MainActivateScene = scene.path, SceneOperation = sceneOperation });
        }

        /// <summary>
        ///     Allows server to fully load in a new scene and override current active scene.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        public void ServerLoadSceneNormal(string scenePath)
        {
            if(!Server && !Server.Active) return;

            var players = new INetworkPlayer[Server.Players.Count];
            Server.Players.CopyTo(players);

            ServerSceneLoading(scenePath, players, true);
        }

        /// <summary>
        ///     Allows server to fully load in another scene on top of current active scene.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="players">List of player's that are receiving the new scene load.</param>
        /// <param name="shouldClientLoadNormally">Should the clients load this additively too or load it full normal scene change.</param>
        public void ServerLoadSceneAdditively(string scenePath, INetworkPlayer[] players, bool shouldClientLoadNormally = false)
        {
            if (!Server && !Server.Active) return;

            ServerSceneLoading(scenePath, players, shouldClientLoadNormally, SceneOperation.LoadAdditive);
        }

        /// <summary>
        ///     Allows server to fully unload a scene additively.
        /// </summary>
        /// <param name="scene">The scene handle which we want to unload additively.</param>
        /// <param name="players">List of player's that are receiving the new scene unload.</param>
        /// <param name="shouldClientUnloadNormally">Should the clients unload this additively too or unload it full normal scene change.</param>
        public void ServerUnloadSceneAdditively(Scene scene, INetworkPlayer[] players, bool shouldClientUnloadNormally = false)
        {
            if (!Server && !Server.Active) return;

            ServerSceneUnLoading(scene, players, SceneOperation.UnloadAdditive);
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

            string[] scenesPaths = new string[SceneManager.sceneCount - 1];

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                if (SceneManager.GetActiveScene().buildIndex == sceneIndex) continue;

                Scene scene = SceneManager.GetSceneAt(sceneIndex);

                if (scene.name.Equals(SceneManager.GetActiveScene().name)) continue;

                scenesPaths[sceneIndex == 0 ? sceneIndex : sceneIndex - 1] = scene.path;
            }

            player.Send(new SceneMessage { MainActivateScene = ActiveScenePath, AdditiveScenes = scenesPaths });
            player.Send(new SceneReadyMessage());
        }

        /// <summary>
        ///     Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ChangeServerScene().
        /// </summary>
        /// <param name="scenePath">The name of the new scene.</param>
        /// <param name="operation">The type of scene loading we want to do.</param>
        internal void OnServerFinishedSceneLoad(string scenePath, SceneOperation operation)
        {
            logger.Log(" [NetworkSceneManager] - OnServerSceneChanged");

            Server.SendToAll(new SceneReadyMessage());

            ServerFinishedSceneChange?.Invoke(scenePath, operation);
        }

        /// <summary>
        ///     When player disconnects from server we will need to clean up info in scenes related to user.
        /// <para>Default implementation we loop through list of scenes and find where this player was in and removed them from list.</para>
        /// </summary>
        /// <param name="disconnectedPlayer"></param>
        protected virtual void OnServerPlayerDisconnected(INetworkPlayer disconnectedPlayer)
        {
            foreach (KeyValuePair<Scene, List<INetworkPlayer>> scene in ServerSceneData)
            {
                for (int index = 0; index < scene.Value.Count; index++)
                {
                    INetworkPlayer player = scene.Value[index];

                    if (player.Identity.NetId != disconnectedPlayer.Identity.NetId) continue;

                    scene.Value.Remove(disconnectedPlayer);

                    break;
                }
            }
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
            {
                Server.Authenticated.AddListener(OnServerAuthenticated);
                Server.Disconnected.AddListener(OnServerPlayerDisconnected);
            }
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
        ///     Finish loading the scene.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene that is finishing.</param>
        /// <param name="sceneOperation">Choose type of scene loading we are doing <see cref="SceneOperation"/>.</param>
        internal void CompleteLoadingScene(string scenePath, SceneOperation sceneOperation)
        {
            // If server mode call this to make sure scene finishes loading
            if (Server && Server.Active)
            {
                if (logger.LogEnabled())
                    logger.Log("[NetworkSceneManager] - Host: " + sceneOperation + " operation for scene: " + scenePath);

                // call OnServerSceneChanged
                OnServerFinishedSceneLoad(scenePath, sceneOperation);
            }

            // If client let's call this to finish client scene loading too
            if (Client && Client.Active)
            {
                if (logger.LogEnabled())
                    logger.Log("[NetworkSceneManager] - Client: " + sceneOperation + " operation for scene: " +
                               scenePath);

                OnClientSceneLoadFinished(scenePath, sceneOperation);
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
            return sceneOperation switch
            {
                SceneOperation.UnloadAdditive => UnLoadSceneAdditiveAsync(scenePath),
                _ => throw new InvalidEnumArgumentException(nameof(sceneOperation), (int)sceneOperation,
                    typeof(SceneOperation))
            };
        }

        /// <summary>
        ///     Internal usage to apply loading a scene correctly. Other api will be available to override things.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="players">List of player's we want to track which scene they are in.</param>
        /// <param name="sceneOperation">Choose type of scene loading we are doing <see cref="SceneOperation"/>.</param>
        private UniTask LoadSceneAsync(string scenePath, INetworkPlayer[] players, SceneOperation sceneOperation = SceneOperation.Normal)
        {
            return sceneOperation switch
            {
                SceneOperation.Normal => LoadSceneNormalAsync(scenePath),
                SceneOperation.LoadAdditive => LoadSceneAdditiveAsync(scenePath, players),
                SceneOperation.UnloadAdditive => UnLoadSceneAdditiveAsync(scenePath),
                _ => throw new InvalidEnumArgumentException(nameof(sceneOperation), (int)sceneOperation,
                    typeof(SceneOperation))
            };
        }

        /// <summary>
        ///     Load our scene up in a normal unity fashion.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        private async UniTask LoadSceneNormalAsync(string scenePath)
        {
            //Scene is already active.
            if (ActiveScenePath.Equals(scenePath))
            {
                CompleteLoadingScene(scenePath, SceneOperation.Normal);
            }
            else
            {
                SceneLoadingAsyncOperationInfo = SceneManager.LoadSceneAsync(scenePath);

                Scene scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

                //If non host client. Wait for server to finish scene change
                if (Client && Client.Active && !Client.IsLocalClient)
                {
                    SceneLoadingAsyncOperationInfo.allowSceneActivation = false;
                }
                else if (Server && Server.Active)
                {
                    // Check to make sure this scene was not already loaded. If it was let's clear old data on it.
                    if (ServerSceneData.ContainsKey(scene))
                    {
                        logger.Log($"[NetworkSceneManager] - Scene load operation: {SceneOperation.Normal}. Scene was already loaded once before. Clearing scene related data in {ServerSceneData}.");

                        ServerSceneData.Remove(scene);

                        ServerSceneData.Add(scene, new List<INetworkPlayer>(Server.Players));
                    }
                }

                await SceneLoadingAsyncOperationInfo;

                logger.Assert(scenePath == ActiveScenePath, "[NetworkSceneManager] - Scene being loaded was not the active scene");

                CompleteLoadingScene(ActiveScenePath, SceneOperation.Normal);
            }
        }

        /// <summary>
        ///     Load our scene additively.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="players">The list of players we want to track to know what scene they are on.</param>
        private async UniTask LoadSceneAdditiveAsync(string scenePath, INetworkPlayer[] players)
        {
            // Ensure additive scene is not already loaded
            if (SceneManager.GetSceneByPath(GetSceneByPathOrName(scenePath).path).IsValid())
            {
                logger.LogWarning($"[NetworkSceneManager] - Scene {scenePath} is already loaded");
            }
            else
            {
                await SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

                Scene scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

                if (Server && Server.Active)
                {
                    ServerSceneData.Add(scene, new List<INetworkPlayer>(players));
                }

                CompleteLoadingScene(scenePath, SceneOperation.LoadAdditive);
            }
        }

        /// <summary>
        ///     Unload our scene additively.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <returns></returns>
        private async UniTask UnLoadSceneAdditiveAsync(string scenePath)
        {
            // Ensure additive scene is actually loaded
            if (SceneManager.GetSceneByPath(scenePath).IsValid())
            {
                await SceneManager.UnloadSceneAsync(scenePath, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

                CompleteLoadingScene(scenePath, SceneOperation.UnloadAdditive);
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
                await SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

                CompleteLoadingScene(scene.path, SceneOperation.UnloadAdditive);
            }
            else
            {
                logger.LogWarning($"[NetworkSceneManager] - Cannot unload {scene} with UnloadAdditive operation");
            }

            if (Server && Server.Active)
            {
                ServerSceneData.Remove(scene);
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
