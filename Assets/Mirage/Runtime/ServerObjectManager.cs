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

        public void Start()
        {
            if (Server != null)
            {
                Server.Started.AddListener(OnServerStarted);
                Server.OnStartHost.AddListener(StartedHost);
                Server.Stopped.AddListener(OnServerStopped);

                if (NetworkSceneManager != null)
                {
                    NetworkSceneManager.OnServerFinishedSceneChange.AddListener(OnFinishedSceneChange);
                    NetworkSceneManager.OnPlayerSceneReady.AddListener(SpawnVisibleObjects);
                }
            }
        }

        internal void RegisterMessageHandlers()
        {
            Server.MessageHandler.RegisterHandler<ServerRpcMessage>(OnServerRpcMessage);
        }

        void OnServerStarted()
        {
            RegisterMessageHandlers();
            SpawnOrActivate();
        }

        void OnServerStopped()
        {
            // todo dont send messages on server stop, only reset NI
            foreach (NetworkIdentity obj in Server.World.SpawnedIdentities.Reverse())
            {
                // Unspawn all, but only destroy non-scene objects on server
                DestroyObject(obj, obj.sceneId == 0);
            }

            Server.World.ClearSpawnedObjects();
            // reset so ids stay small in each session
            nextNetworkId = 1;
        }

        void OnFinishedSceneChange(string scenePath, SceneOperation sceneOperation)
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
        /// Loops spawned collection for NetworkIdentities that are not IsClient and calls StartClient().
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
        /// <param name="character">Player object spawned for the player.</param>
        /// <param name="assetId"></param>
        /// <param name="keepAuthority">Does the previous player remain attached to this connection?</param>
        /// <returns></returns>
        public void ReplaceCharacter(INetworkPlayer player, GameObject character, Guid assetId, bool keepAuthority = false)
        {
            NetworkIdentity identity = character.GetNetworkIdentity();
            identity.AssetId = assetId;
            ReplaceCharacter(player, identity, keepAuthority);
        }

        /// <summary>
        /// This replaces the player object for a connection with a different player object. The old player object is not destroyed.
        /// <para>If a connection already has a player object, this can be used to replace that object with a different player object. This does NOT change the ready state of the connection, so it can safely be used while changing scenes.</para>
        /// </summary>
        /// <param name="player">Connection which is adding the player.</param>
        /// <param name="character">Player object spawned for the player.</param>
        /// <param name="keepAuthority">Does the previous player remain attached to this connection?</param>
        /// <returns></returns>
        public void ReplaceCharacter(INetworkPlayer player, GameObject character, bool keepAuthority = false)
        {
            NetworkIdentity identity = character.GetNetworkIdentity();
            ReplaceCharacter(player, identity, keepAuthority);
        }

        /// <summary>
        /// This replaces the player object for a connection with a different player object. The old player object is not destroyed.
        /// <para>If a connection already has a player object, this can be used to replace that object with a different player object. This does NOT change the ready state of the connection, so it can safely be used while changing scenes.</para>
        /// </summary>
        /// <param name="player">Connection which is adding the player.</param>
        /// <param name="identity">Player object spawned for the player.</param>
        /// <param name="keepAuthority">Does the previous player remain attached to this connection?</param>
        /// <returns></returns>
        public void ReplaceCharacter(INetworkPlayer player, NetworkIdentity identity, bool keepAuthority = false)
        {
            if (identity.Owner != null && identity.Owner != player)
            {
                throw new ArgumentException($"Cannot replace player for connection. New player is already owned by a different connection {identity}");
            }

            //NOTE: there can be an existing player
            logger.Log("NetworkServer ReplacePlayer");

            NetworkIdentity previousPlayer = player.Identity;

            player.Identity = identity;

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
            SpawnVisibleObjectForPlayer(player);

            if (logger.LogEnabled()) logger.Log($"Replacing playerGameObject object netId: {identity.NetId} asset ID {identity.AssetId}");

            Respawn(identity);

            if (!keepAuthority)
                previousPlayer.RemoveClientAuthority();
        }

        void SpawnVisibleObjectForPlayer(INetworkPlayer player)
        {
            if (logger.LogEnabled()) logger.Log($"Checking Observers on {Server.World.SpawnedIdentities.Count} objects for player: {player}");

            if (!player.SceneIsReady)
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
                    if (logger.LogEnabled()) logger.Log($"Checking Observers on server objects name='{identity.name}' netId={identity.NetId} sceneId={identity.sceneId}");

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
        /// <param name="character">Player object spawned for the player.</param>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public void AddCharacter(INetworkPlayer player, GameObject character, Guid assetId)
        {
            NetworkIdentity identity = character.GetNetworkIdentity();
            identity.AssetId = assetId;
            AddCharacter(player, identity);
        }

        /// <summary>
        /// <para>When an <see cref="AddCharacterMessage"/> message handler has received a request from a player, the server calls this to associate the player object with the connection.</para>
        /// <para>When a player is added for a connection, the client for that connection is made ready automatically. The player object is automatically spawned, so you do not need to call NetworkServer.Spawn for that object. This function is used for "adding" a player, not for "replacing" the player on a connection. If there is already a player on this playerControllerId for this connection, this will fail.</para>
        /// </summary>
        /// <param name="player">Connection which is adding the player.</param>
        /// <param name="character">Player object spawned for the player.</param>
        /// <exception cref="ArgumentException">NetworkIdentity must not be null.</exception>
        public void AddCharacter(INetworkPlayer player, GameObject character)
        {
            NetworkIdentity identity = character.GetNetworkIdentity();
            AddCharacter(player, identity);
        }

        /// <summary>
        /// <para>When an <see cref="AddCharacterMessage"/> message handler has received a request from a player, the server calls this to associate the player object with the connection.</para>
        /// <para>When a player is added for a connection, the client for that connection is made ready automatically. The player object is automatically spawned, so you do not need to call NetworkServer.Spawn for that object. This function is used for "adding" a player, not for "replacing" the player on a connection. If there is already a player on this playerControllerId for this connection, this will fail.</para>
        /// </summary>
        /// <param name="player">Connection which is adding the player.</param>
        /// <param name="identity">Player object spawned for the player.</param>
        /// <exception cref="ArgumentException">NetworkIdentity must not be null.</exception>
        public void AddCharacter(INetworkPlayer player, NetworkIdentity identity)
        {
            // cannot have an existing player object while trying to Add another.
            if (player.Identity != null)
            {
                throw new ArgumentException("AddPlayer: player object already exists");
            }

            // make sure we have a controller before we call SetClientReady
            // because the observers will be rebuilt only if we have a controller
            player.Identity = identity;

            identity.SetServerValues(Server, this);

            // Set the connection on the NetworkIdentity on the server, NetworkIdentity.SetLocalPlayer is not called on the server (it is on clients)
            identity.SetClientOwner(player);

            // special case,  we are in host mode,  set hasAuthority to true so that all overrides see it
            if (player == Server.LocalPlayer)
            {
                identity.HasAuthority = true;
                Server.LocalClient.Player.Identity = identity;
            }

            // spawn any new visible scene objects
            SpawnVisibleObjects(player);

            if (logger.LogEnabled()) logger.Log("Adding new playerGameObject object netId: " + identity.NetId + " asset ID " + identity.AssetId);

            Respawn(identity);
        }

        void Respawn(NetworkIdentity identity)
        {
            if (identity.NetId == 0)
            {
                // If the object has not been spawned, then do a full spawn and update observers
                Spawn(identity.gameObject, identity.Owner);
            }
            else
            {
                // otherwise just replace his data
                SendSpawnMessage(identity, identity.Owner);
            }
        }

        internal void ShowForConnection(NetworkIdentity identity, INetworkPlayer player)
        {
            if (player.SceneIsReady)
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
        /// <exception cref="InvalidOperationException">Received remove player message but connection has no player</exception>
        public void RemovePlayerForConnection(INetworkPlayer player, bool destroyServerObject = false)
        {
            if (player.Identity != null)
            {
                Destroy(player.Identity.gameObject, destroyServerObject);
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
            if (!Server.World.TryGetIdentity(msg.netId, out NetworkIdentity identity))
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
            if (skeleton.cmdRequireAuthority && identity.Owner != player)
            {
                if (logger.WarnEnabled()) logger.LogWarning("ServerRpc for object without authority [netId=" + msg.netId + "]");
                return;
            }

            if (logger.LogEnabled()) logger.Log("OnServerRpcMessage for netId=" + msg.netId + " conn=" + player);

            using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(msg.payload))
            {
                networkReader.ObjectLocator = Server.World;
                identity.HandleRemoteCall(skeleton, msg.componentIndex, networkReader, player, msg.replyId.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Spawns the <paramref name="identity"/> and settings its owner to the player that owns <paramref name="ownerObject"/>
        /// </summary>
        /// <param name="ownerObject">An object owned by a player</param>
        public void Spawn(GameObject obj, GameObject ownerObject)
        {
            NetworkIdentity ownerIdentity = ownerObject.GetNetworkIdentity();

            if (ownerIdentity.Owner == null)
            {
                throw new InvalidOperationException("Player object is not a player in the connection");
            }

            Spawn(obj, ownerIdentity.Owner);
        }

        /// <summary>
        /// Assigns <paramref name="assetId"/> to the <paramref name="obj"/> and then it with <paramref name="owner"/>
        /// <para>
        ///     <see cref="NetworkIdentity.AssetId"/> can only be set on an identity if the current value is Empty
        /// </para>
        /// <para>
        ///     This method is useful if you are creating network objects at runtime and both server and client know what <see cref="Guid"/> to set on an object
        /// </para>
        /// </summary>
        /// <param name="obj">The object to spawn.</param>
        /// <param name="assetId">The assetId of the object to spawn. Used for custom spawn handlers.</param>
        /// <param name="owner">The connection that has authority over the object</param>
        public void Spawn(GameObject obj, Guid assetId, INetworkPlayer owner = null)
        {
            // check first before setting AssetId
            ThrowIfPrefab(obj);

            NetworkIdentity identity = obj.GetNetworkIdentity();
            identity.AssetId = assetId;
            Spawn(identity, owner);
        }

        /// <summary>
        /// Spawns the <paramref name="identity"/> and settings its owner to <paramref name="owner"/>
        /// </summary>
        public void Spawn(GameObject obj, INetworkPlayer owner = null)
        {
            NetworkIdentity identity = obj.GetNetworkIdentity();
            Spawn(identity, owner);
        }

        /// <summary>
        /// Spawns the <paramref name="identity"/> and keeping owner as <see cref="NetworkIdentity.Owner"/>
        /// </summary>
        public void Spawn(NetworkIdentity identity)
        {
            Spawn(identity, identity.Owner);
        }

        /// <summary>
        /// Spawns the <paramref name="identity"/> and assigns <paramref name="owner"/> to be it's owner
        /// </summary>
        public void Spawn(NetworkIdentity identity, INetworkPlayer owner)
        {
            if (!Server || !Server.Active)
            {
                throw new InvalidOperationException("NetworkServer is not active. Cannot spawn objects without an active server.");
            }

            ThrowIfPrefab(identity.gameObject);

            identity.Owner = owner;

            identity.SetServerValues(Server, this);

            // special case to make sure hasAuthority is set
            // on start server in host mode
            if (owner == Server.LocalPlayer)
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
            if (logger.LogEnabled()) logger.Log("Server SendSpawnMessage: name=" + identity.name + " sceneId=" + identity.sceneId.ToString("X") + " netId=" + identity.NetId);

            // one writer for owner, one for observers
            using (PooledNetworkWriter ownerWriter = NetworkWriterPool.GetWriter(), observersWriter = NetworkWriterPool.GetWriter())
            {
                bool isOwner = identity.Owner == player;

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
            identity.OnSerializeAll(true, ownerWriter, observersWriter);

            // use owner segment if 'conn' owns this identity, otherwise
            // use observers segment
            ArraySegment<byte> payload = isOwner ?
                ownerWriter.ToArraySegment() :
                observersWriter.ToArraySegment();

            return payload;
        }

        /// <summary>
        /// Prefabs are not allowed to be spawned, they most be instantiated first
        /// <para>This check does nothing in builds</para>
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws in the editor if object is part of a prefab</exception>
        static void ThrowIfPrefab(GameObject obj)
        {
#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(obj))
            {
                throw new InvalidOperationException($"GameObject {obj.name} is a prefab, it can't be spawned.");
            }
#endif
        }

        /// <summary>
        /// Destroys this object and corresponding objects on all clients.
        /// <param name="gameObject">Game object to destroy.</param>
        /// <param name="destroyServerObject">Sets if server object will also be destroyed</param>
        /// </summary>
        public void Destroy(GameObject gameObject, bool destroyServerObject = true)
        {
            if (gameObject == null)
            {
                logger.Log("NetworkServer DestroyObject is null");
                return;
            }

            NetworkIdentity identity = gameObject.GetNetworkIdentity();
            DestroyObject(identity, destroyServerObject);
        }

        /// <summary>
        /// Destroys this object and corresponding objects on all clients.
        /// <param name="identity">Game object to destroy.</param>
        /// <param name="destroyServerObject">Sets if server object will also be destroyed</param>
        /// </summary>
        public void Destroy(NetworkIdentity identity, bool destroyServerObject = true)
        {
            if (identity == null)
            {
                logger.Log("NetworkServer DestroyObject is null");
                return;
            }

            DestroyObject(identity, destroyServerObject);
        }

        void DestroyObject(NetworkIdentity identity, bool destroyServerObject)
        {
            if (logger.LogEnabled()) logger.Log("DestroyObject instance:" + identity.NetId);

            Server.World.RemoveIdentity(identity);
            identity.Owner?.RemoveOwnedObject(identity);

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
        /// <exception cref="InvalidOperationException">Thrown when server is not active</exception>
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
        /// Sends spawn message for scene objects and other visible objects to the given player if it has a character
        /// <para>
        /// If there is a <see cref="Mirage.NetworkSceneManager"/> then this will be called after the client finishes loading the scene and sends <see cref="SceneReadyMessage"/>
        /// </para>
        /// </summary>
        /// <param name="player">The player to spawn objects for</param>
        public void SpawnVisibleObjects(INetworkPlayer player)
        {
            if (logger.LogEnabled()) logger.Log("SetClientReadyInternal for conn:" + player);

            // client is ready to start spawning objects
            if (player.Identity != null)
                SpawnVisibleObjectForPlayer(player);
        }
    }
}
