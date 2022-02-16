using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Mirage.Logging;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        public INetIdGenerator NetIdGenerator;

        uint nextNetworkId = 1;
        uint GetNextNetworkId() => NetIdGenerator?.GenerateNetId() ?? checked(nextNetworkId++);

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
            Server.MessageHandler.RegisterHandler<ServerRpcWithReplyMessage>(OnServerRpcWithReplyMessage);
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
                DestroyObject(obj, !obj.IsSceneObject);
            }

            Server.World.ClearSpawnedObjects();
            // reset so ids stay small in each session
            nextNetworkId = 1;
        }

        void OnFinishedSceneChange(Scene scene, SceneOperation sceneOperation)
        {
            Server.World.RemoveDestroyedObjects();

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
        /// <param name="prefabHash"></param>
        /// <param name="keepAuthority">Does the previous player remain attached to this connection?</param>
        /// <returns></returns>
        public void ReplaceCharacter(INetworkPlayer player, GameObject character, int prefabHash, bool keepAuthority = false)
        {
            NetworkIdentity identity = character.GetNetworkIdentity();
            ReplaceCharacter(player, identity, prefabHash, keepAuthority);
        }

        /// <summary>
        /// This replaces the player object for a connection with a different player object. The old player object is not destroyed.
        /// <para>If a connection already has a player object, this can be used to replace that object with a different player object. This does NOT change the ready state of the connection, so it can safely be used while changing scenes.</para>
        /// </summary>
        /// <param name="player">Connection which is adding the player.</param>
        /// <param name="character">Player object spawned for the player.</param>
        /// <param name="prefabHash"></param>
        /// <param name="keepAuthority">Does the previous player remain attached to this connection?</param>
        /// <returns></returns>
        public void ReplaceCharacter(INetworkPlayer player, NetworkIdentity character, int prefabHash, bool keepAuthority = false)
        {
            character.PrefabHash = prefabHash;
            ReplaceCharacter(player, character, keepAuthority);
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
            if (!player.HasCharacter)
            {
                throw new InvalidOperationException($"ReplaceCharacter can only be called if Player already has a charater");
            }

            //NOTE: there can be an existing player
            logger.Log("NetworkServer ReplacePlayer");

            NetworkIdentity previousCharacter = player.Identity;

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

            if (logger.LogEnabled()) logger.Log($"Replacing playerGameObject object netId: {identity.NetId} asset ID {identity.PrefabHash:X}");

            Respawn(identity);

            if (!keepAuthority)
                previousCharacter.RemoveClientAuthority();
        }

        void SpawnVisibleObjectForPlayer(INetworkPlayer player)
        {
            if (logger.LogEnabled()) logger.Log($"Checking Observers on {Server.World.SpawnedIdentities.Count} objects for player: {player}");

            if (!player.SceneIsReady)
            {
                // client needs to finish loading scene before we can spawn objects
                // otherwise it would not find scene objects.
                return;
            }

            // add connection to each nearby NetworkIdentity's observers, which
            // internally sends a spawn message for each one to the connection.
            foreach (NetworkIdentity identity in Server.World.SpawnedIdentities)
            {
                // todo, do we only need to spawn active objects here? or all objects?
                if (identity.gameObject.activeSelf)
                {
                    if (logger.LogEnabled()) logger.Log($"Checking Observers on server objects name='{identity.name}' netId={identity.NetId} sceneId={identity.SceneId:X}");

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
        /// <param name="prefabHash"></param>
        /// <returns></returns>
        public void AddCharacter(INetworkPlayer player, GameObject character, int prefabHash)
        {
            NetworkIdentity identity = character.GetNetworkIdentity();
            AddCharacter(player, identity, prefabHash);
        }

        /// <summary>
        /// <para>When an <see cref="AddCharacterMessage"/> message handler has received a request from a player, the server calls this to associate the player object with the connection.</para>
        /// <para>When a player is added for a connection, the client for that connection is made ready automatically. The player object is automatically spawned, so you do not need to call NetworkServer.Spawn for that object. This function is used for "adding" a player, not for "replacing" the player on a connection. If there is already a player on this playerControllerId for this connection, this will fail.</para>
        /// </summary>
        /// <param name="player">Connection which is adding the player.</param>
        /// <param name="character">Player object spawned for the player.</param>
        /// <param name="prefabHash"></param>
        /// <returns></returns>
        public void AddCharacter(INetworkPlayer player, NetworkIdentity character, int prefabHash)
        {
            character.PrefabHash = prefabHash;
            AddCharacter(player, character);
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
            if (player.HasCharacter)
            {
                throw new ArgumentException("AddCharacter can only be called if the player does not already have a character");
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

            if (logger.LogEnabled()) logger.Log($"Adding new playerGameObject object netId: {identity.NetId} asset ID {identity.PrefabHash:X}");

            Respawn(identity);
        }

        void Respawn(NetworkIdentity identity)
        {
            if (!identity.IsSpawned)
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

        /// <summary>
        /// Sends spawn message to player if it is not loading a scene
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="player"></param>
        internal void ShowToPlayer(NetworkIdentity identity, INetworkPlayer player)
        {
            // dont send if loading scene
            if (player.SceneIsReady)
                SendSpawnMessage(identity, player);
        }

        internal void HideToPlayer(NetworkIdentity identity, INetworkPlayer player)
        {
            player.Send(new ObjectHideMessage { netId = identity.NetId });
        }

        /// <summary>
        /// Removes the character from a player, with the option to keep the player as the owner of the object
        /// </summary>
        /// <param name="player"></param>
        /// <param name="keepAuthority"></param>
        /// <exception cref="InvalidOperationException">Throws when player does not have a character</exception>
        public void RemoveCharacter(INetworkPlayer player, bool keepAuthority = false)
        {
            ThrowIfNoCharacter(player);

            NetworkIdentity identity = player.Identity;
            player.Identity = null;
            if (!keepAuthority)
            {
                logger.Assert(identity.Owner == player, "Owner should be player that is being removed");
                identity.Owner = null;
            }

            player.Send(new RemoveCharacterMessage { keepAuthority = keepAuthority });
        }

        /// <summary>
        /// Removes and destroys the character from a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="destroyServerObject"></param>
        /// <exception cref="InvalidOperationException">Throws when player does not have a character</exception>
        public void DestroyCharacter(INetworkPlayer player, bool destroyServerObject = true)
        {
            ThrowIfNoCharacter(player);

            Destroy(player.Identity.gameObject, destroyServerObject);
            player.Identity = null;
        }

        /// <exception cref="InvalidOperationException">Throws when player does not have a character</exception>
        private static void ThrowIfNoCharacter(INetworkPlayer player)
        {
            if (!player.HasCharacter)
            {
                throw new InvalidOperationException("Player did not have a character");
            }
        }


        /// <summary>
        /// Handle ServerRpc from specific player, this could be one of multiple players on a single client
        /// </summary>
        /// <param name="player"></param>
        /// <param name="msg"></param>
        void OnServerRpcWithReplyMessage(INetworkPlayer player, ServerRpcWithReplyMessage msg)
        {
            OnServerRpc(player, msg.netId, msg.componentIndex, msg.functionIndex, msg.payload, msg.replyId);
        }
        void OnServerRpcMessage(INetworkPlayer player, ServerRpcMessage msg)
        {
            OnServerRpc(player, msg.netId, msg.componentIndex, msg.functionIndex, msg.payload, default);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnServerRpc(INetworkPlayer player, uint netId, int componentIndex, int functionIndex, ArraySegment<byte> payload, int replyId)
        {
            if (!Server.World.TryGetIdentity(netId, out NetworkIdentity identity))
            {
                if (logger.WarnEnabled()) logger.LogWarning($"Spawned object not found when handling ServerRpc message [netId={netId}]");
                return;
            }

            NetworkBehaviour behaviour = identity.NetworkBehaviours[componentIndex];

            RemoteCall remoteCall = behaviour.remoteCallCollection.Get(functionIndex);

            if (remoteCall.InvokeType != RpcInvokeType.ServerRpc)
            {
                throw new MethodInvocationException($"Invalid ServerRpc for index {functionIndex}");
            }

            // ServerRpcs can be for player objects, OR other objects with client-authority
            // -> so if this connection's controller has a different netId then
            //    only allow the ServerRpc if clientAuthorityOwner
            if (remoteCall.RequireAuthority && identity.Owner != player)
            {
                if (logger.WarnEnabled()) logger.LogWarning($"ServerRpc for object without authority [netId={netId}]");
                return;
            }

            if (logger.LogEnabled()) logger.Log($"OnServerRpcMessage for netId={netId} conn={player}");

            using (PooledNetworkReader reader = NetworkReaderPool.GetReader(payload))
            {
                reader.ObjectLocator = Server.World;
                remoteCall.Invoke(reader, behaviour, player, replyId);
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
        /// Assigns <paramref name="prefabHash"/> to the <paramref name="obj"/> and then it with <paramref name="owner"/>
        /// <para>
        ///     <see cref="NetworkIdentity.PrefabHash"/> can only be set on an identity if the current value is Empty
        /// </para>
        /// <para>
        ///     This method is useful if you are creating network objects at runtime and both server and client know what <see cref="Guid"/> to set on an object
        /// </para>
        /// </summary>
        /// <param name="obj">The object to spawn.</param>
        /// <param name="prefabHash">The prefabHash of the object to spawn. Used for custom spawn handlers.</param>
        /// <param name="owner">The connection that has authority over the object</param>
        public void Spawn(GameObject obj, int prefabHash, INetworkPlayer owner = null)
        {
            // check first before setting prefab
            ThrowIfPrefab(obj);

            NetworkIdentity identity = obj.GetNetworkIdentity();
            identity.PrefabHash = prefabHash;
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

            if (!identity.IsSpawned)
            {
                // the object has not been spawned yet
                identity.NetId = GetNextNetworkId();
                identity.StartServer();
                Server.World.AddIdentity(identity.NetId, identity);
            }

            if (logger.LogEnabled()) logger.Log($"SpawnObject instance ID {identity.NetId} asset ID {identity.PrefabHash:X}");

            identity.RebuildObservers(true);
        }

        internal void SendSpawnMessage(NetworkIdentity identity, INetworkPlayer player)
        {
            if (logger.LogEnabled()) logger.Log($"Server SendSpawnMessage: name={identity.name} sceneId={identity.SceneId:X} netId={identity.NetId}");

            // one writer for owner, one for observers
            using (PooledNetworkWriter ownerWriter = NetworkWriterPool.GetWriter(), observersWriter = NetworkWriterPool.GetWriter())
            {
                bool isOwner = identity.Owner == player;

                ArraySegment<byte> payload = CreateSpawnMessagePayload(isOwner, identity, ownerWriter, observersWriter);

                int? prefabHash = identity.IsPrefab ? identity.PrefabHash : default(int?);
                ulong? sceneId = identity.IsSceneObject ? identity.SceneId : default(ulong?);
                var msg = new SpawnMessage
                {
                    netId = identity.NetId,
                    isLocalPlayer = player.Identity == identity,
                    isOwner = isOwner,
                    sceneId = sceneId,
                    prefabHash = prefabHash,
                    payload = payload,
                };

                // values in msg are nullable, so by default they are null
                // only set those values if the identity's settings say to send them
                if (identity.SpawnSettings.SendPosition) msg.position = identity.transform.localPosition;
                if (identity.SpawnSettings.SendRotation) msg.rotation = identity.transform.localRotation;
                if (identity.SpawnSettings.SendScale) msg.scale = identity.transform.localScale;

                player.Send(msg);
            }
        }

        internal void SendRemoveAuthorityMessage(NetworkIdentity identity, INetworkPlayer previousOwner)
        {
            if (logger.LogEnabled()) logger.Log($"Server SendRemoveAuthorityMessage: name={identity.name} sceneId={identity.SceneId:X} netId={identity.NetId}");

            previousOwner.Send(new RemoveAuthorityMessage
            {
                netId = identity.NetId,
            });
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

            identity.NetworkReset();
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
            return identity.IsSceneObject;
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
                    if (logger.LogEnabled()) logger.Log($"SpawnObjects sceneId:{identity.SceneId:X} name:{identity.gameObject.name}");

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
        public void SpawnVisibleObjects(INetworkPlayer player) => SpawnVisibleObjects(player, false);

        /// <summary>
        /// Sends spawn message for scene objects and other visible objects to the given player if it has a character
        /// </summary>
        /// <param name="player">The player to spawn objects for</param>
        /// <param name="ignoreHasCharacter">If true will spawn visibile objects even if player does not have a spawned character yet</param>
        // note: can't use optional param here because we need just NetworkPlayer version for event
        public void SpawnVisibleObjects(INetworkPlayer player, bool ignoreHasCharacter)
        {
            if (logger.LogEnabled()) logger.Log("SetClientReadyInternal for conn:" + player);

            // client is ready to start spawning objects
            if (ignoreHasCharacter || player.HasCharacter)
                SpawnVisibleObjectForPlayer(player);
        }
    }
}
