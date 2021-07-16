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
        static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkSceneManager));

        [Header("Setup Settings")]

        [FormerlySerializedAs("client")]
        public NetworkClient Client;

        [FormerlySerializedAs("server")]
        public NetworkServer Server;

        /// <summary>
        /// Sets the NetworksSceneManagers GameObject to DontDestroyOnLoad. Default = true.
        /// </summary>
        public bool DontDestroy = true;

        protected readonly Dictionary<Scene, List<INetworkPlayer>> ServerSceneData = new Dictionary<Scene, List<INetworkPlayer>>();

        #region Events

        [Header("Events")]

        [FormerlySerializedAs("ClientChangeScene")]
        [SerializeField] SceneChangeEvent _clientChangeScene = new SceneChangeEvent();

        [FormerlySerializedAs("ClientSceneChanged")]
        [SerializeField] SceneChangeEvent _clientSceneChanged = new SceneChangeEvent();

        [FormerlySerializedAs("ServerChangeScene")]
        [SerializeField] SceneChangeEvent _serverChangeScene = new SceneChangeEvent();

        [FormerlySerializedAs("ServerSceneChanged")]
        [SerializeField] SceneChangeEvent _serverSceneChanged = new SceneChangeEvent();

        /// <summary>
        /// Event fires when the Client starts changing scene.
        /// </summary>
        public SceneChangeEvent ClientChangeScene => _clientChangeScene;

        /// <summary>
        /// Event fires after the Client has completed its scene change.
        /// </summary>
        public SceneChangeEvent ClientSceneChanged => _clientSceneChanged;

        /// <summary>
        /// Event fires before Server changes scene.
        /// </summary>
        public SceneChangeEvent ServerChangeScene => _serverChangeScene;

        /// <summary>
        /// Event fires after Server has completed scene change.
        /// </summary>
        public SceneChangeEvent ServerSceneChanged => _serverSceneChanged;

        #endregion

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
        /// Used by the server to track all additive scenes. To notify clients upon connection 
        /// </summary>
        internal readonly List<string> ClientAdditiveSceneLoadingList = new List<string>();

        /// <summary>
        /// Used by the client to load the full additive scene list that the server has upon connection
        /// </summary>
        internal readonly List<string> ClientPendingAdditiveSceneLoadingList = new List<string>();

        private AsyncOperation _clientLoadingOperation;

        #region Unity Methods

        public void Start()
        {
            if (DontDestroy)
                DontDestroyOnLoad(gameObject);

            if (Client != null)
            {
                Client.Started.AddListener(RegisterClientMessages);
            }

            if (Server != null)
            {
                Server.Authenticated.AddListener(OnServerAuthenticated);
            }
        }

        void OnDestroy()
        {
            if (Client != null)
                Client.Started.RemoveListener(RegisterClientMessages);
        }

        #endregion

        #region Client

        private void RegisterClientMessages()
        {
            Client.MessageHandler.RegisterHandler<SceneMessage>(ClientSceneMessage);

            if (!Client.IsLocalClient)
            {
                Client.MessageHandler.RegisterHandler<SceneReadyMessage>(ClientSceneReadyMessage);
                Client.MessageHandler.RegisterHandler<NotReadyMessage>(ClientNotReadyMessage);
            }
        }

        internal void ClientSceneMessage(INetworkPlayer player, SceneMessage msg)
        {
            if (!Client.IsConnected)
            {
                throw new InvalidOperationException("[NetworkSceneManager] - ClientSceneMessage: cannot change network scene while client is disconnected");
            }

            if (string.IsNullOrEmpty(msg.scenePath))
            {
                throw new ArgumentNullException(nameof(msg.scenePath), $"[NetworkSceneManager] - ClientSceneMessage: {nameof(msg.scenePath)} cannot be empty or null");
            }

            if (logger.LogEnabled()) logger.Log($"[NetworkSceneManager] - ClientSceneMessage: changing scenes from: {ActiveScenePath} to: {msg.scenePath}");

            //Additive are scenes loaded on server and this client is not a host client
            if (msg.additiveScenes != null && msg.additiveScenes.Length > 0 && Client && !Client.IsLocalClient)
            {
                foreach (string scene in msg.additiveScenes)
                {
                    ClientPendingAdditiveSceneLoadingList.Add(scene);
                }
            }

            // Let client prepare for scene change
            OnClientChangeScene(msg.scenePath, msg.sceneOperation);

            ApplyOperationAsync(msg.scenePath, player, msg.sceneOperation).Forget();
        }

        internal void ClientSceneReadyMessage(INetworkPlayer player, SceneReadyMessage msg)
        {
            logger.Log("[NetworkSceneManager] - ClientSceneReadyMessage");

            //Server has finished changing scene. Allow the client to finish.
            if (_clientLoadingOperation != null)
                _clientLoadingOperation.allowSceneActivation = true;
        }

        internal void ClientNotReadyMessage(INetworkPlayer player, NotReadyMessage msg)
        {
            logger.Log("[NetworkSceneManager] - OnClientNotReadyMessageInternal");

            Client.Player.IsReady = false;
        }

        /// <summary>
        /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
        /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="scenePath">Path of the scene that's about to be loaded</param>
        /// <param name="sceneOperation">Scene operation that's about to happen</param>
        internal void OnClientChangeScene(string scenePath, SceneOperation sceneOperation)
        {
            ClientChangeScene?.Invoke(scenePath, sceneOperation);
        }

        /// <summary>
        /// Called on clients when a scene has completed loading, when the scene load was initiated by the server.
        /// <para>Non-Additive Scene changes will cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkSceneManager is to add a player object for the connection if no player object exists.</para>
        /// </summary>
        /// <param name="scenePath">Path of the scene that was just loaded</param>
        /// <param name="sceneOperation">Scene operation that was just  happen</param>
        internal void OnClientSceneChanged(string scenePath, SceneOperation sceneOperation)
        {
            if (ClientPendingAdditiveSceneLoadingList.Count > 0 && Client && !Client.IsLocalClient)
            {
                ApplyOperationAsync(ClientPendingAdditiveSceneLoadingList[0], Client.Player, SceneOperation.LoadAdditive).Forget();
                ClientPendingAdditiveSceneLoadingList.RemoveAt(0);
                return;
            }

            //set ready after scene change has completed
            if (!Client.Player.IsReady)
                SetClientReady();

            //Call event once all scene related actions (subscenes and ready) are done.
            ClientSceneChanged?.Invoke(scenePath, sceneOperation);
        }

        /// <summary>
        /// Signal that the client connection is ready to enter the game.
        /// <para>This could be for example when a client enters an ongoing game and has finished loading the current scene. The server should respond to the message with an appropriate handler which instantiates the players object for example.</para>
        /// </summary>
        /// <exception cref="InvalidOperationException">When called with an null or disconnected client</exception>
        public void SetClientReady()
        {
            if (!Client || !Client.Active)
                throw new InvalidOperationException("[NetworkSceneManager] - Ready() called with an null or disconnected client");

            if (logger.LogEnabled()) logger.Log("[NetworkSceneManager] - ClientScene.Ready() called.");

            // Set these before sending the ReadyMessage, otherwise host client
            // will fail in InternalAddPlayer with null readyConnection.
            Client.Player.IsReady = true;

            // Tell server we're ready to have a player object spawned
            Client.Player.Send(new ReadyMessage());
        }

        #endregion

        #region Server

        // called after successful authentication
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
                    ServerSceneData.Add(scene, new List<INetworkPlayer> {player});
            }

            player.Send(new SceneMessage { scenePath = ActiveScenePath, additiveScenes = scenesPaths });
            player.Send(new SceneReadyMessage());
        }

        /// <summary>
        /// This causes the server to switch scenes and sets the ActiveScenePath.
        /// <para>Clients that connect to this server will automatically switch to this scene. This automatically sets clients to be not-ready. The clients must call Ready() again to participate in the new scene.</para>
        /// </summary>
        /// <param name="scenePath"></param>
        /// <param name="operation"></param>
        /// <exception cref="ArgumentNullException">Scene path was not valid.</exception>
        public void ChangeServerScene(string scenePath, SceneOperation sceneOperation = SceneOperation.Normal)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                throw new ArgumentNullException(nameof(scenePath), "[NetworkSceneManager] - ServerChangeScene: " + nameof(scenePath) + " cannot be empty or null");
            }

            if (logger.LogEnabled()) logger.Log("[NetworkSceneManager] - ServerChangeScene " + scenePath);

            // Let server prepare for scene change
            OnServerChangeScene(scenePath, sceneOperation);

            if (!Server.LocalClientActive)
                ApplyOperationAsync(scenePath, null, sceneOperation).Forget();

            // notify all clients about the new scene
            Server.SendToAll(new SceneMessage { scenePath = scenePath, sceneOperation = sceneOperation });

            // Server is loading full new scene let's clear scene data.
            ServerSceneData.Clear();

            // Add all player's to new scene data player's should be part of.
            ServerSceneData.Add(GetSceneByPathOrName(scenePath), new List<INetworkPlayer>(Server.Players));
        }

        /// <summary>
        ///     Creates a new physics scene on server and transfer specific player's to that scene.
        /// </summary>
        /// <param name="scenePath">The scene to use as base for the new physics scene.</param>
        /// <param name="sceneParameters">The scene parameter data we want to use.</param>
        public void CreatePhysicsScene(string scenePath, LoadSceneParameters sceneParameters, IEnumerable<INetworkPlayer> players)
        {
            SceneManager.LoadSceneAsync(scenePath, sceneParameters);

            // Since this is new scene loading this should be count -1 to get newest scene?
            ServerSceneData.Add(SceneManager.GetSceneAt(SceneManager.sceneCount - 1),
                new List<INetworkPlayer>(players));

            NetworkServer.SendToMany(players, new SceneMessage());
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
        ///     Let's us get scene by full path or by its name.
        /// </summary>
        /// <param name="scenePath">The path or name representing the scene.</param>
        /// <returns>Returns back correct scene data.</returns>
        private Scene GetSceneByPathOrName(string scenePath)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);

            return scene != default ? scene : SceneManager.GetSceneByName(scenePath);
        }

        /// <summary>
        /// Called from ChangeServerScene immediately before NetworkSceneManager's LoadSceneAsync is executed
        /// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="scenePath">Path of the scene that's about to be loaded</param>
        internal void OnServerChangeScene(string scenePath, SceneOperation operation)
        {
            logger.Log("[NetworkSceneManager] - OnServerChangeScene");

            ServerChangeScene?.Invoke(scenePath, operation);
        }

        /// <summary>
        /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ChangeServerScene().
        /// </summary>
        /// <param name="scenePath">The name of the new scene.</param>
        internal void OnServerSceneChanged(string scenePath, SceneOperation operation)
        {
            logger.Log(" [NetworkSceneManager] - OnServerSceneChanged");

            Server.SendToAll(new SceneReadyMessage());

            ServerSceneChanged?.Invoke(scenePath, operation);
        }

        #endregion

        #region Scene Operations

        private UniTask ApplyOperationAsync(string scenePath, INetworkPlayer player, SceneOperation sceneOperation = SceneOperation.Normal)
        {
            switch (sceneOperation)
            {
                case SceneOperation.Normal: return ApplyNormalOperationAsync(scenePath, player);
                case SceneOperation.LoadAdditive: return ApplyAdditiveLoadOperationAsync(scenePath, player);
                case SceneOperation.UnloadAdditive: return ApplyUnloadAdditiveOperationAsync(scenePath, player);
                default:
                    // Should never happen
                    throw new InvalidEnumArgumentException(nameof(sceneOperation), (int)sceneOperation, typeof(SceneOperation));
            }
        }

        private async UniTask ApplyNormalOperationAsync(string scenePath, INetworkPlayer player)
        {
            //Scene is already active.
            if (ActiveScenePath.Equals(scenePath))
            {
                FinishLoadScene(scenePath, player, SceneOperation.Normal);
            }
            else
            {
                _clientLoadingOperation = SceneManager.LoadSceneAsync(scenePath);

                //If non host client. Wait for server to finish scene change
                if (Client && Client.Active && !Client.IsLocalClient)
                {
                    _clientLoadingOperation.allowSceneActivation = false;
                }

                await _clientLoadingOperation;

                logger.Assert(scenePath == ActiveScenePath, "[NetworkSceneManager] - Scene being loaded was not the active scene");
                FinishLoadScene(ActiveScenePath, player, SceneOperation.Normal);
            }
        }

        private async UniTask ApplyAdditiveLoadOperationAsync(string scenePath, INetworkPlayer player)
        {
            // Ensure additive scene is not already loaded
            if (SceneManager.GetSceneByPath(scenePath).IsValid())
            {
                logger.LogWarning($"[NetworkSceneManager] - Scene {scenePath} is already loaded");
            }
            else
            {
                await SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
                ClientAdditiveSceneLoadingList.Add(scenePath);
                FinishLoadScene(scenePath, player, SceneOperation.LoadAdditive);
            }
        }

        private async UniTask ApplyUnloadAdditiveOperationAsync(string scenePath, INetworkPlayer player)
        {
            // Ensure additive scene is actually loaded
            if (SceneManager.GetSceneByPath(scenePath).IsValid())
            {
                // Send player
                SceneManager.MoveGameObjectToScene(player.Identity.gameObject, SceneManager.GetActiveScene());

                await SceneManager.UnloadSceneAsync(scenePath, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
                ClientAdditiveSceneLoadingList.Remove(scenePath);
                FinishLoadScene(scenePath, player, SceneOperation.UnloadAdditive);
            }
            else
            {
                logger.LogWarning($"[NetworkSceneManager] - Cannot unload {scenePath} with UnloadAdditive operation");
            }
        }

        internal void FinishLoadScene(string scenePath, INetworkPlayer player, SceneOperation sceneOperation)
        {
            switch (sceneOperation)
            {
                case SceneOperation.LoadAdditive:
                case SceneOperation.Normal:
                    if (SceneManager.GetSceneByPath(scenePath) != SceneManager.GetActiveScene() && player != null)
                        SceneManager.MoveGameObjectToScene(player.Identity.gameObject, GetSceneByPathOrName(scenePath));
                    break;
                case SceneOperation.UnloadAdditive:
                    if (Server && Server.Active)
                        ServerSceneData.Remove(GetSceneByPathOrName(scenePath));
                    break;
            }

            // host mode?
            if (Client && Client.IsLocalClient)
            {
                if (logger.LogEnabled()) logger.Log("[NetworkSceneManager] - Host: " + sceneOperation + " operation for scene: " + scenePath);

                // call OnServerSceneChanged
                OnServerSceneChanged(scenePath, sceneOperation);

                if (Client.IsConnected)
                {
                    // let client know that we changed scene
                    OnClientSceneChanged(scenePath, sceneOperation);
                }
            }
            // server-only mode?
            else if (Server && Server.Active)
            {
                if (logger.LogEnabled()) logger.Log("[NetworkSceneManager] - Server: " + sceneOperation + " operation for scene: " + scenePath);

                OnServerSceneChanged(scenePath, sceneOperation);
            }
            // client-only mode?
            else if (Client && Client.Active && !Client.IsLocalClient)
            {
                if (logger.LogEnabled()) logger.Log("[NetworkSceneManager] - Client: " + sceneOperation + " operation for scene: " + scenePath);

                OnClientSceneChanged(scenePath, sceneOperation);
            }
        }

        #endregion
    }
}
