using System;
using System.Collections.Generic;
using System.Linq;
using Mirage.Logging;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirage
{
    public static class GameobjectExtension
    {
        /// <summary>
        /// Gets <see cref="NetworkIdentity"/> on a <see cref="GameObject"/> and throws <see cref="InvalidOperationException"/> if the GameObject does not have one.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>attached NetworkIdentity</returns>
        /// <exception cref="InvalidOperationException">Throws when <paramref name="gameObject"/> does not have a NetworkIdentity attached</exception>
        public static NetworkIdentity GetNetworkIdentity(this GameObject gameObject)
        {
            if (!gameObject.TryGetComponent(out NetworkIdentity identity))
            {
                throw new InvalidOperationException($"Gameobject {gameObject.name} doesn't have NetworkIdentity.");
            }
            return identity;
        }
    }

    /// <summary>
    /// The ServerObjectManager.
    /// </summary>
    /// <remarks>
    /// <para>The set of networked objects that have been spawned is managed by ServerObjectManager.
    /// Objects are spawned with ServerObjectManager.Spawn() which adds them to this set, and makes them be created on clients.
    /// Spawned objects are removed automatically when they are destroyed, or than they can be removed from the spawned set by calling ServerObjectManager.UnSpawn() - this does not destroy the object.</para>
    /// </remarks>
    [AddComponentMenu("Network/ServerObjectManager")]
    [DisallowMultipleComponent]
    public class ServerObjectManager : MonoBehaviour, IServerObjectManager
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(ServerObjectManager));

        [FormerlySerializedAs("server")]
        public NetworkServer Server;
        [FormerlySerializedAs("networkSceneManager")]
        public NetworkSceneManager NetworkSceneManager;

        uint nextNetworkId = 1;
        uint GetNextNetworkId() => checked(nextNetworkId++);

        public SyncVarSender SyncVarSender { get; private set; }

        public void Start()
        {
            if (Server != null)
            {
                Server.Started.AddListener(OnServerStarted);
                Server.OnStartHost.AddListener(StartedHost);
                Server.Authenticated.AddListener(OnAuthenticated);
                Server.Stopped.AddListener(OnServerStopped);

                if (NetworkSceneManager != null)
                {
                    NetworkSceneManager.ServerChangeScene.AddListener(OnServerChangeScene);
                    NetworkSceneManager.ServerSceneChanged.AddListener(OnServerSceneChanged);
                }
            }
        }

        // The user should never need to pump the update loop manually
        internal void Update()
        {
            SyncVarSender?.Update();
        }

        internal void RegisterMessageHandlers(INetworkPlayer player)
        {
            player.RegisterHandler<ReadyMessage>(OnClientReadyMessage);
            player.RegisterHandler<ServerRpcMessage>(OnServerRpcMessage);
        }

        void OnAuthenticated(INetworkPlayer player)
        {
            RegisterMessageHandlers(player);
        }

        void OnServerStarted()
        {
            SyncVarSender = new SyncVarSender();
            SpawnOrActivate();
        }

        void OnServerStopped()
        {
            foreach (NetworkIdentity obj in Server.World.SpawnedIdentities.Reverse())
            {
                if (obj.AssetId != Guid.Empty)
                    DestroyObject(obj, true);
            }

            Server.World.ClearSpawnedObjects();
            SyncVarSender = null;
            // reset so ids stay small in each session
            nextNetworkId = 1;
        }

        void OnServerChangeScene(string scenePath, SceneOperation sceneOperation)
        {
            SetAllClientsNotReady();
        }

        void OnServerSceneChanged(string scenePath, SceneOperation sceneOperation)
        {
            SpawnOrActivate();
        }

        void SpawnOrActivate()
        {
            if (Server && Server.Active)
            {
                SpawnObjects();

                // host mode?
                if (Server.LocalClientActive)
                {
                    StartHostClientObjects();
                }
            }
        }

        /// <summary>
        /// Loops spawned collection for NetworkIdentieis that are not IsClient and calls StartClient().
        /// </summary>
        void StartHostClientObjects()
        {
            foreach (NetworkIdentity identity in Server.World.SpawnedIdentities)
            {
                if (!identity.IsClient)
                {
                    if (logger.LogEnabled()) logger.Log("ActivateHostScene " + identity.NetId + " " + identity);

                    identity.StartClient();
                }
            }
        }

        void StartedHost()
        {
            if (TryGetComponent(out ClientObjectManager ClientObjectManager))
            {
                ClientObjectManager.ServerObjectManager = this;
            }
        }

        /// <summary>
        /// This replaces the player object for a connection with a different player object. The old player object is not destroyed.
        /// <para>If a connection already has a player object, this can be used to replace that object with a different player object. This does NOT change the ready state of the connection, so it can safely be used while changing scenes.</para>
        /// </summary>
        /// <param name="player">Connection which is adding the player.</param>
        /// <param name="client">Client associated to the player.</param> 
        /// <param name="character">Player object spawned for the player.</param>
        /// <param name="assetId"></param>
        /// <param name="keepAuthority">Does the previous player remain attached to this connection?</param>
        /// <returns></returns>
        public bool ReplaceCharacter(INetworkPlayer player, INetworkClient client, GameObject character, Guid assetId, bool keepAuthority = false)
        {
            NetworkIdentity identity = character.GetNetworkIdentity();
            identity.AssetId = assetId;
            return InternalReplacePlayerForConnection(player, client, character, keepAuthority);
        }

        /// <summary>
        /// This replaces the player object for a connection with a different player object. The old player object is not destroyed.
        /// <para>If a connection already has a player object, this can be used to replace that object with a different player object. This does NOT change the ready state of the connection, so it can safely be used while changing scenes.</para>
        /// </summary>
        /// <param name="player">Connection which is adding the player.</param>
        /// <param name="client">Client associated to the player.</param> 
        /// <param name="character">Player object spawned for the player.</param>
        /// <param name="keepAuthority">Does the previous player remain attached to this connection?</param>
        /// <returns></returns>
        public bool ReplaceCharacter(INetworkPlayer player, INetworkClient client, GameObject character, bool keepAuthority = false)
        {
            return InternalReplacePlayerForConnection(player, client, character, keepAuthority);
        }

        void SpawnObserversForConnection(INetworkPlayer player)
        {
            if (logger.LogEnabled()) logger.Log("Spawning " + Server.World.SpawnedIdentities.Count + " objects for conn " + player);

            if (!player.IsReady)
            {
                // client needs to finish initializing before we can spawn objects
                // otherwise it would not find them.
                return;
            }

            // add connection to each nearby NetworkIdentity's observers, which
            // internally sends a spawn message for each one to the connection.
            foreach (NetworkIdentity identity in Server.World.SpawnedIdentities)
            {
                if (identity.gameObject.activeSelf)
                {
                    if (logger.LogEnabled()) logger.Log("Sending spawn message for current server objects name='" + identity.name + "' netId=" + identity.NetId + " sceneId=" + identity.sceneId);

                    bool visible = identity.OnCheckObserver(player);
                    if (visible)
                    {
                        identity.AddObserver(player);
                    }
                }
            }
        }

        /// <summary>
        /// <para>When an <see cref="AddCharacterMessage"/> message handler has received a request from a player, the server calls this to associate the player object with the connection.</para>
        /// <para>When a player is added for a connection, the client for that connection is made ready automatically. The player object is automatically spawned, so you do not need to call NetworkServer.Spawn for that object. This function is used for "adding" a player, not for "replacing" the player on a connection. If there is already a player on this playerControllerId for this connection, this will fail.</para>
        /// </summary>
        /// <param name="player">Connection which is adding the player.</param>
        /// <param name="client">Client associated to the player.</param> 
        /// <param name="character">Player object spawned for the player.</param>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public bool AddCharacter(INetworkPlayer player, GameObject character, Guid assetId)
        {
            NetworkIdentity identity = character.GetNetworkIdentity();
            identity.AssetId = assetId;
            return AddCharacter(player, character);
        }

        /// <summary>
        /// <para>When an <see cref="AddCharacterMessage"/> message handler has received a request from a player, the server calls this to associate the player object with the connection.</para>
        /// <para>When a player is added for a connection, the client for that connection is made ready automatically. The player object is automatically spawned, so you do not need to call NetworkServer.Spawn for that object. This function is used for "adding" a player, not for "replacing" the player on a connection. If there is already a player on this playerControllerId for this connection, this will fail.</para>
        /// </summary>
        /// <param name="player">Connection which is adding the player.</param>
        /// <param name="client">Client associated to the player.</param>
        /// <param name="character">Player object spawned for the player.</param>
        /// <returns></returns>
        public bool AddCharacter(INetworkPlayer player, GameObject character)
        {
            NetworkIdentity identity = character.GetComponent<NetworkIdentity>();
            if (identity is null)
            {
                logger.Log("AddPlayer: playerGameObject has no NetworkIdentity. Please add a NetworkIdentity to " + character);
                return false;
            }

            // cannot have a player object in "Add" version
            if (player.Identity != null)
            {
                logger.Log("AddPlayer: player object already exists");
                return false;
            }

            // make sure we have a controller before we call SetClientReady
            // because the observers will be rebuilt only if we have a controller
            player.Identity = identity;

            // set server to the NetworkIdentity
            identity.Server = Server;
            identity.ServerObjectManager = this;
            identity.Client = Server.LocalClient;

            // Set the connection on the NetworkIdentity on the server, NetworkIdentity.SetLocalPlayer is not called on the server (it is on clients)
            identity.SetClientOwner(player);

            // special case,  we are in host mode,  set hasAuthority to true so that all overrides see it
            if (player == Server.LocalPlayer)
            {
                identity.HasAuthority = true;
                Server.LocalClient.Player.Identity = identity;
            }

            // set ready if not set yet
            SetClientReady(player);

            if (logger.LogEnabled()) logger.Log("Adding new playerGameObject object netId: " + identity.NetId + " asset ID " + identity.AssetId);

            Respawn(identity);
            return true;
        }

        void Respawn(NetworkIdentity identity)
        {
            if (identity.NetId == 0)
            {
                // If the object has not been spawned, then do a full spawn and update observers
                Spawn(identity.gameObject, identity.ConnectionToClient);
            }
            else
            {
                // otherwise just replace his data
                SendSpawnMessage(identity, identity.ConnectionToClient);
            }
        }

        internal bool InternalReplacePlayerForConnection(INetworkPlayer player, INetworkClient client, GameObject character, bool keepAuthority)
        {
            NetworkIdentity identity = character.GetComponent<NetworkIdentity>();
            if (identity is null)
            {
                logger.LogError("ReplacePlayer: playerGameObject has no NetworkIdentity. Please add a NetworkIdentity to " + character);
                return false;
            }

            if (identity.ConnectionToClient != null && identity.ConnectionToClient != player)
            {
                logger.LogError("Cannot replace player for connection. New player is already owned by a different connection" + character);
                return false;
            }

            //NOTE: there can be an existing player
            logger.Log("NetworkServer ReplacePlayer");

            NetworkIdentity previousPlayer = player.Identity;

            player.Identity = identity;
            identity.Client = client;

            // Set the connection on the NetworkIdentity on the server, NetworkIdentity.SetLocalPlayer is not called on the server (it is on clients)
            identity.SetClientOwner(player);

            // special case,  we are in host mode,  set hasAuthority to true so that all overrides see it
            if (player == Server.LocalPlayer)
            {
                identity.HasAuthority = true;
                Server.LocalClient.Player.Identity = identity;
            }

            // add connection to observers AFTER the playerController was set.
            // by definition, there is nothing to observe if there is no player
            // controller.
            //
            // IMPORTANT: do this in AddCharacter & ReplaceCharacter!
            SpawnObserversForConnection(player);

            if (logger.LogEnabled()) logger.Log("Replacing playerGameObject object netId: " + character.GetComponent<NetworkIdentity>().NetId + " asset ID " + character.GetComponent<NetworkIdentity>().AssetId);

            Respawn(identity);

            if (!keepAuthority)
                previousPlayer.RemoveClientAuthority();

            return true;
        }

        internal void ShowForConnection(NetworkIdentity identity, INetworkPlayer player)
        {
            if (player.IsReady)
                SendSpawnMessage(identity, player);
        }

        internal void HideForConnection(NetworkIdentity identity, INetworkPlayer player)
        {
            player.Send(new ObjectHideMessage { netId = identity.NetId });
        }

        /// <summary>
        /// Removes the player object from the connection
        /// </summary>
        /// <param name="player">The connection of the client to remove from</param>
        /// <param name="destroyServerObject">Indicates whether the server object should be destroyed</param>
        public void RemovePlayerForConnection(INetworkPlayer player, bool destroyServerObject = false)
        {
            if (player.Identity != null)
            {
                if (destroyServerObject)
                    Destroy(player.Identity.gameObject);
                else
                    UnSpawn(player.Identity.gameObject);

                player.Identity = null;
            }
            else
            {
                throw new InvalidOperationException("Received remove player message but connection has no player");
            }
        }

        /// <summary>
        /// Handle ServerRpc from specific player, this could be one of multiple players on a single client
        /// </summary>
        /// <param name="player"></param>
        /// <param name="msg"></param>
        void OnServerRpcMessage(INetworkPlayer player, ServerRpcMessage msg)
        {
            if (!Server.World.TryGetIdentity(msg.netId, out NetworkIdentity identity) || identity is null)
            {
                if (logger.WarnEnabled()) logger.LogWarning("Spawned object not found when handling ServerRpc message [netId=" + msg.netId + "]");
                return;
            }
            Skeleton skeleton = RemoteCallHelper.GetSkeleton(msg.functionHash);

            if (skeleton.invokeType != RpcInvokeType.ServerRpc)
            {
                throw new MethodInvocationException($"Invalid ServerRpc for id {msg.functionHash}");
            }

            // ServerRpcs can be for player objects, OR other objects with client-authority
            // -> so if this connection's controller has a different netId then
            //    only allow the ServerRpc if clientAuthorityOwner
            if (skeleton.cmdRequireAuthority && identity.ConnectionToClient != player)
            {
                if (logger.WarnEnabled()) logger.LogWarning("ServerRpc for object without authority [netId=" + msg.netId + "]");
                return;
            }

            if (logger.LogEnabled()) logger.Log("OnServerRpcMessage for netId=" + msg.netId + " conn=" + player);

            using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(msg.payload))
            {
                networkReader.ObjectLocator = Server.World;
                identity.HandleRemoteCall(skeleton, msg.componentIndex, networkReader, player, msg.replyId);
            }
        }

        internal void SpawnObject(GameObject obj, INetworkPlayer ownerPlayer)
        {
            if (!Server || !Server.Active)
            {
                throw new InvalidOperationException("SpawnObject for " + obj + ", NetworkServer is not active. Cannot spawn objects without an active server.");
            }

            NetworkIdentity identity = obj.GetComponent<NetworkIdentity>();
            if (identity is null)
            {
                throw new InvalidOperationException("SpawnObject " + obj + " has no NetworkIdentity. Please add a NetworkIdentity to " + obj);
            }

            identity.ConnectionToClient = ownerPlayer;
            identity.Server = Server;
            identity.ServerObjectManager = this;
            identity.World = Server.World;
            identity.Client = Server.LocalClient;

            // special case to make sure hasAuthority is set
            // on start server in host mode
            if (ownerPlayer == Server.LocalPlayer)
                identity.HasAuthority = true;

            if (identity.NetId == 0)
            {
                // the object has not been spawned yet
                identity.NetId = GetNextNetworkId();
                identity.StartServer();
                Server.World.AddIdentity(identity.NetId, identity);
            }

            if (logger.LogEnabled()) logger.Log("SpawnObject instance ID " + identity.NetId + " asset ID " + identity.AssetId);

            identity.RebuildObservers(true);
        }

        internal void SendSpawnMessage(NetworkIdentity identity, INetworkPlayer player)
        {
            // for easier debugging
            if (logger.LogEnabled()) logger.Log("Server SendSpawnMessage: name=" + identity.name + " sceneId=" + identity.sceneId.ToString("X") + " netid=" + identity.NetId);

            // one writer for owner, one for observers
            using (PooledNetworkWriter ownerWriter = NetworkWriterPool.GetWriter(), observersWriter = NetworkWriterPool.GetWriter())
            {
                bool isOwner = identity.ConnectionToClient == player;

                ArraySegment<byte> payload = CreateSpawnMessagePayload(isOwner, identity, ownerWriter, observersWriter);

                player.Send(new SpawnMessage
                {
                    netId = identity.NetId,
                    isLocalPlayer = player.Identity == identity,
                    isOwner = isOwner,
                    sceneId = identity.sceneId,
                    assetId = identity.AssetId,
                    // use local values for VR support
                    position = identity.transform.localPosition,
                    rotation = identity.transform.localRotation,
                    scale = identity.transform.localScale,

                    payload = payload,
                });
            }
        }

        static ArraySegment<byte> CreateSpawnMessagePayload(bool isOwner, NetworkIdentity identity, PooledNetworkWriter ownerWriter, PooledNetworkWriter observersWriter)
        {
            // Only call OnSerializeAllSafely if there are NetworkBehaviours
            if (identity.NetworkBehaviours.Length == 0)
            {
                return default;
            }

            // serialize all components with initialState = true
            // (can be null if has none)
            identity.OnSerializeAllSafely(true, ownerWriter, observersWriter);

            // use owner segment if 'conn' owns this identity, otherwise
            // use observers segment
            ArraySegment<byte> payload = isOwner ?
                ownerWriter.ToArraySegment() :
                observersWriter.ToArraySegment();

            return payload;
        }

        bool CheckForPrefab(GameObject obj)
        {
#if UNITY_EDITOR
            return UnityEditor.PrefabUtility.IsPartOfPrefabAsset(obj);
#else
            return false;
#endif
        }

        bool VerifyCanSpawn(GameObject obj)
        {
            if (CheckForPrefab(obj))
            {
                logger.LogFormat(LogType.Error, "GameObject {0} is a prefab, it can't be spawned. This will cause errors in builds.", obj.name);
                return false;
            }

            return true;
        }

        /// <summary>
        /// This spawns an object like NetworkServer.Spawn() but also assigns Client Authority to the specified client.
        /// <para>This is the same as calling NetworkIdentity.AssignClientAuthority on the spawned object.</para>
        /// </summary>
        /// <param name="obj">The object to spawn.</param>
        /// <param name="owner">The player object to set Client Authority to.</param>
        public void Spawn(GameObject obj, GameObject owner)
        {
            NetworkIdentity identity = owner.GetComponent<NetworkIdentity>();
            if (identity is null)
            {
                throw new InvalidOperationException("Player object has no NetworkIdentity");
            }

            if (identity.ConnectionToClient == null)
            {
                throw new InvalidOperationException("Player object is not a player in the connection");
            }

            Spawn(obj, identity.ConnectionToClient);
        }

        /// <summary>
        /// This spawns an object.
        /// </summary>
        /// <param name="identity">The identity to spawn.</param>
        public void Spawn(NetworkIdentity identity)
        {
            Spawn(identity.gameObject, identity.ConnectionToClient);
        }

        /// <summary>
        /// This spawns an object like NetworkServer.Spawn() but also assigns Client Authority to the specified client.
        /// <para>This is the same as calling NetworkIdentity.AssignClientAuthority on the spawned object.</para>
        /// </summary>
        /// <param name="obj">The object to spawn.</param>
        /// <param name="assetId">The assetId of the object to spawn. Used for custom spawn handlers.</param>
        /// <param name="client">The client associated to the object.</param>
        /// <param name="owner">The connection that has authority over the object</param>
        public void Spawn(GameObject obj, Guid assetId, INetworkPlayer owner = null)
        {
            if (VerifyCanSpawn(obj))
            {
                NetworkIdentity identity = obj.GetNetworkIdentity();
                identity.AssetId = assetId;
                SpawnObject(obj, owner);
            }
        }

        /// <summary>
        /// Spawn the given game object on all clients which are ready.
        /// <para>This will cause a new object to be instantiated from the registered prefab, or from a custom spawn function.</para>
        /// </summary>
        /// <param name="obj">Game object with NetworkIdentity to spawn.</param>
        /// <param name="client">Client associated to the object.</param>
        /// <param name="owner">The connection that has authority over the object</param>
        public void Spawn(GameObject obj, INetworkPlayer owner = null)
        {
            if (VerifyCanSpawn(obj))
            {
                SpawnObject(obj, owner);
            }
        }

        void DestroyObject(NetworkIdentity identity, bool destroyServerObject)
        {
            if (logger.LogEnabled()) logger.Log("DestroyObject instance:" + identity.NetId);

            Server.World.RemoveIdentity(identity);
            identity.ConnectionToClient?.RemoveOwnedObject(identity);

            identity.SendToRemoteObservers(new ObjectDestroyMessage { netId = identity.NetId });

            identity.ClearObservers();
            if (Server.LocalClientActive)
            {
                identity.StopClient();
            }

            identity.StopServer();

            identity.Reset();
            // when unspawning, dont destroy the server's object
            if (destroyServerObject)
            {
                UnityEngine.Object.Destroy(identity.gameObject);
            }
        }

        /// <summary>
        /// Destroys this object and corresponding objects on all clients.
        /// <para>In some cases it is useful to remove an object but not delete it on the server. For that, use NetworkServer.UnSpawn() instead of NetworkServer.Destroy().</para>
        /// </summary>
        /// <param name="obj">Game object to destroy.</param>
        public void Destroy(GameObject obj)
        {
            if (obj == null)
            {
                logger.Log("NetworkServer DestroyObject is null");
                return;
            }

            NetworkIdentity identity = obj.GetNetworkIdentity();
            DestroyObject(identity, true);
        }

        /// <summary>
        /// This takes an object that has been spawned and un-spawns it.
        /// <para>The object will be removed from clients that it was spawned on, or the custom spawn handler function on the client will be called for the object.</para>
        /// <para>Unlike when calling NetworkServer.Destroy(), on the server the object will NOT be destroyed. This allows the server to re-use the object, even spawn it again later.</para>
        /// </summary>
        /// <param name="obj">The spawned object to be unspawned.</param>
        public void UnSpawn(GameObject obj)
        {
            if (obj == null)
            {
                logger.Log("NetworkServer UnspawnObject is null");
                return;
            }

            NetworkIdentity identity = obj.GetNetworkIdentity();
            DestroyObject(identity, false);
        }

        internal bool ValidateSceneObject(NetworkIdentity identity)
        {
            if (identity.gameObject.hideFlags == HideFlags.NotEditable ||
                identity.gameObject.hideFlags == HideFlags.HideAndDontSave)
                return false;

#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.IsPersistent(identity.gameObject))
                return false;
#endif

            // If not a scene object
            return identity.sceneId != 0;
        }

        private class NetworkIdentityComparer : IComparer<NetworkIdentity>
        {
            public int Compare(NetworkIdentity x, NetworkIdentity y)
            {
                return x.NetId.CompareTo(y.NetId);
            }
        }

        /// <summary>
        /// This causes NetworkIdentity objects in a scene to be spawned on a server.
        /// <para>
        ///     Calling SpawnObjects() causes all scene objects to be spawned.
        ///     It is like calling NetworkServer.Spawn() for each of them.
        /// </para>
        /// </summary>
        /// <exception cref="T:InvalidOperationException">Thrown when server is not active</exception>
        public void SpawnObjects()
        {
            // only if server active
            if (!Server || !Server.Active)
                throw new InvalidOperationException("Server was not active");

            NetworkIdentity[] identities = Resources.FindObjectsOfTypeAll<NetworkIdentity>();
            Array.Sort(identities, new NetworkIdentityComparer());

            foreach (NetworkIdentity identity in identities)
            {
                if (ValidateSceneObject(identity))
                {
                    if (logger.LogEnabled()) logger.Log("SpawnObjects sceneId:" + identity.sceneId.ToString("X") + " name:" + identity.gameObject.name);

                    Spawn(identity.gameObject);
                }
            }
        }

        /// <summary>
        /// Sets the client to be ready.
        /// <para>When a client has signaled that it is ready, this method tells the server that the client is ready to receive spawned objects and state synchronization updates. This is usually called in a handler for the SYSTEM_READY message. If there is not specific action a game needs to take for this message, relying on the default ready handler function is probably fine, so this call wont be needed.</para>
        /// </summary>
        /// <param name="player">The connection of the client to make ready.</param>
        public void SetClientReady(INetworkPlayer player)
        {
            if (logger.LogEnabled()) logger.Log("SetClientReadyInternal for conn:" + player);

            // set ready
            player.IsReady = true;

            // client is ready to start spawning objects
            if (player.Identity != null)
                SpawnObserversForConnection(player);
        }

        /// <summary>
        /// Marks all connected clients as no longer ready.
        /// <para>All clients will no longer be sent state synchronization updates. The player's clients can call ClientManager.Ready() again to re-enter the ready state. This is useful when switching scenes.</para>
        /// </summary>
        public void SetAllClientsNotReady()
        {
            foreach (INetworkPlayer player in Server.Players)
            {
                SetClientNotReady(player);
            }
        }

        /// <summary>
        /// Sets the client of the connection to be not-ready.
        /// <para>Clients that are not ready do not receive spawned objects or state synchronization updates. They client can be made ready again by calling SetClientReady().</para>
        /// </summary>
        /// <param name="player">The connection of the client to make not ready.</param>
        public void SetClientNotReady(INetworkPlayer player)
        {
            if (player.IsReady)
            {
                if (logger.LogEnabled()) logger.Log("PlayerNotReady " + player);
                player.IsReady = false;
                player.RemoveObservers();

                player.Send(new NotReadyMessage());
            }
        }

        /// <summary>
        /// default ready handler. 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="msg"></param>
        void OnClientReadyMessage(INetworkPlayer player, ReadyMessage msg)
        {
            if (logger.LogEnabled()) logger.Log("Default handler for ready message from " + player);
            SetClientReady(player);
        }
    }
}
