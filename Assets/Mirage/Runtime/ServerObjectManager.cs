using System;
using System.Collections.Generic;
using System.Linq;
using Mirage.Logging;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mirage
{
    /// <summary>
    /// The ServerObjectManager.
    /// </summary>
    /// <remarks>
    /// <para>The set of networked objects that have been spawned is managed by ServerObjectManager.
    /// Objects are spawned with ServerObjectManager.Spawn() which adds them to this set, and makes them be created on clients.
    /// Spawned objects are removed automatically when they are destroyed, or than they can be removed from the spawned set by calling ServerObjectManager.UnSpawn() - this does not destroy the object.</para>
    /// </remarks>
    [AddComponentMenu("Network/ServerObjectManager")]
    [HelpURL("https://miragenet.github.io/Mirage/docs/reference/Mirage/ServerObjectManager")]
    [DisallowMultipleComponent]
    public class ServerObjectManager : MonoBehaviour
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(ServerObjectManager));

        internal RpcHandler _rpcHandler;
        private SyncVarReceiver _syncVarReceiver;

        private NetworkServer _server;
        public NetworkServer Server => _server;

        public INetIdGenerator NetIdGenerator;
        private uint _nextNetworkId = 1;

        private uint GetNextNetworkId() => NetIdGenerator?.GenerateNetId() ?? checked(_nextNetworkId++);

        public INetworkVisibility DefaultVisibility { get; private set; }

        /// <summary>
        /// A filter to only include certain scene objects when spawning.
        /// <para>Used by <see cref="SpawnSceneObjects"/> when finding scene objects.</para>
        /// <para>If the filter is null, all valid scene objects will be included.</para>
        /// </summary>
        public Func<NetworkIdentity, bool> SceneObjectFilter { get; set; }

        internal void ServerStarted(NetworkServer server)
        {
            if (_server != null && _server != server)
                throw new InvalidOperationException($"ServerObjectManager already in use by another NetworkServer, current:{_server}, new:{server}");

            _server = server;
            _server.Stopped.AddListener(OnServerStopped);

            DefaultVisibility = new AlwaysVisible(server);

            _rpcHandler = new RpcHandler(_server.World, RpcInvokeType.ServerRpc);
            _rpcHandler.ServerRegisterHandler(_server.MessageHandler);

            _syncVarReceiver = new SyncVarReceiver(_server.World);
            _syncVarReceiver.ServerRegisterHandlers(_server.MessageHandler);
        }

        private void OnServerStopped()
        {
            // todo dont send messages on server stop, only reset NI
            // todo why are we calling .Reverse() here? should we be using sorted list here instead of dictionary
            foreach (var obj in _server.World.SpawnedIdentities.Reverse())
            {
                // Unspawn all, but only destroy non-scene objects on server
                DestroyObject(obj, !obj.IsSceneObject);
            }

            Debug.Assert(_server.World.SpawnedIdentities.Count == 0, "All Identities should have been removed by DestroyObject above");
            _server.World.ClearSpawnedObjects();
            // reset so ids stay small in each session
            _nextNetworkId = 1;

            // clear server after stopping
            _server.Stopped.RemoveListener(OnServerStopped);
            _server = null;
            _rpcHandler = null;
            _syncVarReceiver = null;
        }

        [System.Obsolete("Use SpawnSceneObjects instead")]
        internal void SpawnOrActivate() => SpawnSceneObjects();

        /// <summary>
        /// Should be called when server is started, after host client is connected.
        /// <para>
        /// note: this method only needs to be called on setup, <see cref="SpawnSceneObjects"/> should be called after scene loading.
        /// </para>
        /// </summary>
        internal void FirstServerSpawn()
        {
            if (!_server || !_server.Active)
            {
                logger.LogWarning("SpawnOrActivate called when server was not active");
                return;
            }

            SpawnSceneObjects();

            if (_server.IsHost)
            {
                // edge case, if object is spawned before host client is connected,
                // we will need to spawn on host's client side
                var sortedIdentities = _server.World.GetSortedIdentities();
                foreach (var identity in sortedIdentities)
                {
                    // object was not set up as host, need to call OnHostClientSpawn to set it up
                    if (!identity.IsClient)
                    {
                        if (logger.LogEnabled()) logger.Log("ActivateHostScene " + identity.NetId + " " + identity);
                        Debug.Assert(identity.IsServer, "identity should already be spawned on server");

                        // easier to just make a spawn message and call OnHostClientSpawn,
                        // than to copy those function
                        var localPlayer = _server.LocalPlayer;
                        var msg = new SpawnMessage
                        {
                            NetId = identity.NetId,
                            IsOwner = identity.Owner == localPlayer,
                            IsLocalPlayer = localPlayer.Identity == identity,
                        };

                        _server.LocalClient.ObjectManager.OnHostClientSpawn(msg);
                        Debug.Assert(identity.IsClient && identity.IsServer, "identity should now be both client and server");
                    }
                }
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
                throw new InvalidOperationException($"ReplaceCharacter can only be called if Player already has a character");
            }

            //NOTE: there can be an existing player
            logger.Log("NetworkServer ReplacePlayer");

            var previousCharacter = player.Identity;

            player.Identity = identity;

            // Set the connection on the NetworkIdentity on the server, NetworkIdentity.SetLocalPlayer is not called on the server (it is on clients)
            identity.SetOwner(player);

            // special case,  we are in host mode,  set hasAuthority to true so that all overrides see it
            if (_server.LocalPlayer != null && player == _server.LocalPlayer)
            {
                identity.HasAuthority = true;
                _server.LocalClient.Player.Identity = identity;
            }

            // add connection to observers AFTER the playerController was set.
            // by definition, there is nothing to observe if there is no player
            // controller.
            //
            // IMPORTANT: do this in AddCharacter & ReplaceCharacter!
            SpawnVisibleObjects(player, identity);

            if (logger.LogEnabled()) logger.Log($"Replacing playerGameObject object netId: {identity.NetId} asset ID {identity.PrefabHash:X}");

            Respawn(identity);

            if (!keepAuthority)
                previousCharacter.RemoveClientAuthority();
        }

        /// <summary>
        /// <para>When <see cref="AddCharacterMessage"/> is received from a player, the server calls this to associate the character GameObject with the NetworkPlayer.</para>
        /// <para>When a character is added for a player the object is automatically spawned, so you do not need to call ServerObjectManager.Spawn for that object.</para>
        /// <para>This function is used for adding a character, not replacing. If there is already a character then use <see cref="ReplaceCharacter"/> instead.</para>
        /// </summary>
        /// <param name="player">the Player to add the character to</param>
        /// <param name="character">The Network Object to add to the Player. Can be spawned or unspawned. Calling this method will respawn it.</param>
        /// <param name="prefabHash">New prefab hash to give to the player, used for dynamically creating objects at runtime.</param>
        /// <exception cref="ArgumentException">throw when the player already has a character</exception>
        public void AddCharacter(INetworkPlayer player, NetworkIdentity character, int prefabHash)
        {
            character.PrefabHash = prefabHash;
            AddCharacter(player, character);
        }

        /// <summary>
        /// <para>When <see cref="AddCharacterMessage"/> is received from a player, the server calls this to associate the character GameObject with the NetworkPlayer.</para>
        /// <para>When a character is added for a player the object is automatically spawned, so you do not need to call ServerObjectManager.Spawn for that object.</para>
        /// <para>This function is used for adding a character, not replacing. If there is already a character then use <see cref="ReplaceCharacter"/> instead.</para>
        /// </summary>
        /// <param name="player">the Player to add the character to</param>
        /// <param name="character">The Network Object to add to the Player. Can be spawned or unspawned. Calling this method will respawn it.</param>
        /// <exception cref="ArgumentException">throw when the player already has a character</exception>
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

            identity.SetServerValues(_server, this);

            // Set the connection on the NetworkIdentity on the server, NetworkIdentity.SetLocalPlayer is not called on the server (it is on clients)
            identity.SetOwner(player);

            // special case, we are in host mode, set hasAuthority to true so that all overrides see it
            if (_server.LocalPlayer != null && player == _server.LocalPlayer)
            {
                identity.HasAuthority = true;
                _server.LocalClient.Player.Identity = identity;
            }

            // spawn any new visible scene objects
            SpawnVisibleObjects(player, identity);

            if (logger.LogEnabled()) logger.Log($"Adding new playerGameObject object netId: {identity.NetId} asset ID {identity.PrefabHash:X}");

            Respawn(identity);
        }

        private void Respawn(NetworkIdentity identity)
        {
            if (!identity.IsSpawned)
            {
                // If the object has not been spawned, then do a full spawn and update observers
                Spawn(identity, identity.Owner);
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
            var visibility = identity.Visibility;
            if (visibility is NetworkVisibility networkVisibility)
                networkVisibility.InvokeVisibilityChanged(player, true);

            // dont send if loading scene
            if (player.SceneIsReady)
                SendSpawnMessage(identity, player);
        }
        /// <summary>
        /// Sends spawn message to player if it is not loading a scene
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="player"></param>
        internal void ShowToPlayerMany(NetworkIdentity identity, List<INetworkPlayer> players)
        {
            // make new list so that we can filter out SceneIsReady
            using var addedWrapper = AutoPool<List<INetworkPlayer>>.Take();
            var sendTo = addedWrapper.Item;
            Debug.Assert(sendTo.Count == 0);

            foreach (var player in players)
            {
                var visibility = identity.Visibility;
                if (visibility is NetworkVisibility networkVisibility)
                    networkVisibility.InvokeVisibilityChanged(player, true);

                if (player.SceneIsReady)
                    sendTo.Add(player);
            }

            if (sendTo.Count == 1)
                SendSpawnMessage(identity, sendTo[0]);
            else if (sendTo.Count > 1)
                SendSpawnMessageMany(identity, sendTo);
        }


        internal void HideToPlayer(NetworkIdentity identity, INetworkPlayer player)
        {
            var visibility = identity.Visibility;
            if (visibility is NetworkVisibility networkVisibility)
                networkVisibility.InvokeVisibilityChanged(player, false);

            player.Send(new ObjectHideMessage { NetId = identity.NetId });
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

            var identity = player.Identity;
            player.Identity = null;
            if (!keepAuthority)
            {
                logger.Assert(identity.Owner == player, "Owner should be player that is being removed");
                identity.SetOwner(null);
            }

            player.Send(new RemoveCharacterMessage { KeepAuthority = keepAuthority });
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
        /// Assigns <paramref name="prefabHash"/> to the <paramref name="identity"/> and then spawns it with <paramref name="owner"/>
        /// <para>
        ///     <see cref="NetworkIdentity.PrefabHash"/> can only be set to a non-zero value.
        /// </para>
        /// <para>
        ///     <see cref="NetworkIdentity.SceneId"/> will be cleared when calling this method, this will ensure that the object is spawned using the new PrefabHash rather than SceneId
        /// </para>
        /// <para>
        ///     This method is useful if you are creating network objects at runtime and both server and client know what <see cref="Guid"/> to set on an object
        /// </para>
        /// </summary>
        /// <param name="obj">The object to spawn.</param>
        /// <param name="prefabHash">The prefabHash of the object to spawn. Used for custom spawn handlers.</param>
        /// <param name="owner">The connection that has authority over the object</param>
        public void Spawn(NetworkIdentity identity, int prefabHash, INetworkPlayer owner = null)
        {
            // check first before setting prefab
            ThrowIfPrefab(identity);

            identity.PrefabHash = prefabHash;

            if (identity.IsSceneObject)
            {
                if (logger.LogEnabled()) logger.Log($"Clearing SceneId on {identity} because setting prefabHash when spawning. Old sceneId={identity.SceneId:X} New PrefabHash:{prefabHash:X}");
                identity.ClearSceneId();
            }

            Spawn(identity, owner);
        }

        /// <summary>
        /// Spawns the <paramref name="identity"/> and keeping owner as <see cref="NetworkIdentity.Owner"/>
        /// </summary>
        public void Spawn(NetworkIdentity identity, INetworkPlayer owner)
        {
            // check first before setting owner
            ThrowIfPrefab(identity);

            identity.SetOwner(owner);
            Spawn(identity);
        }

        /// <summary>
        /// Spawns the <paramref name="identity"/> and assigns <paramref name="owner"/> to be it's owner
        /// </summary>
        public void Spawn(NetworkIdentity identity)
        {
            if (!_server || !_server.Active)
            {
                throw new InvalidOperationException("NetworkServer is not active. Cannot spawn objects without an active server.");
            }

            ThrowIfPrefab(identity.gameObject);


            identity.SetServerValues(_server, this);

            // special case to make sure hasAuthority is set
            // on start server in host mode
            // note: we need != null here, HasAuthority should never be null on server
            //       this is so that logic in syncvar sender works correctly
            if (_server.LocalPlayer != null && identity.Owner == _server.LocalPlayer)
                identity.HasAuthority = true;

            if (!identity.IsSpawned)
            {
                // the object has not been spawned yet
                identity.NetId = GetNextNetworkId();
                identity.StartServer();
                _server.World.AddIdentity(identity.NetId, identity);
            }

            if (logger.LogEnabled()) logger.Log($"SpawnObject NetId:{identity.NetId} PrefabHash:{identity.PrefabHash:X}");

            identity.RebuildObservers(true);
        }

        internal void SendSpawnMessage(NetworkIdentity identity, INetworkPlayer player)
        {
            logger.Assert(player.IsAuthenticated || !(identity.Visibility is AlwaysVisible), // can't use `is not` in unity2020
                "SendSpawnMessage should only be called if player is authenticated, or there is custom visibility");
            if (logger.LogEnabled()) logger.Log($"Server SendSpawnMessage: name={identity.name} sceneId={identity.SceneId:X} netId={identity.NetId}");

            // one writer for owner, one for observers
            using (PooledNetworkWriter ownerWriter = NetworkWriterPool.GetWriter(), observersWriter = NetworkWriterPool.GetWriter())
            {
                var isOwner = identity.Owner == player;

                var prefabHash = identity.IsPrefab ? identity.PrefabHash : default(int?);
                var sceneId = identity.IsSceneObject ? identity.SceneId : default(ulong?);

                // same validation that client does
                // no point sending message if client has no way to spawn it
                if (prefabHash == null && sceneId == null)
                    throw new SpawnObjectException($"Empty prefabHash and sceneId for {identity}");

                ArraySegment<byte> payload = default;
                var hasPayload = CreateSpawnMessagePayload(identity, ownerWriter, observersWriter);
                if (hasPayload)
                {
                    payload = isOwner
                        ? ownerWriter.ToArraySegment()
                        : observersWriter.ToArraySegment();
                }

                var msg = new SpawnMessage
                {
                    NetId = identity.NetId,
                    IsLocalPlayer = player.Identity == identity,
                    IsOwner = isOwner,
                    SceneId = sceneId,
                    PrefabHash = prefabHash,
                    Payload = payload,
                };

                msg.SpawnValues = CreateSpawnValues(identity);

                player.Send(msg);
            }
        }
        internal void SendSpawnMessageMany(NetworkIdentity identity, List<INetworkPlayer> players)
        {
            if (logger.LogEnabled()) logger.Log($"Server SendSpawnMessage: name={identity.name} sceneId={identity.SceneId:X} netId={identity.NetId}");

            // one writer for owner, one for observers
            using (PooledNetworkWriter ownerWriter = NetworkWriterPool.GetWriter(), observersWriter = NetworkWriterPool.GetWriter())
            {
                ArraySegment<byte> payload = default;
                var hasPayload = CreateSpawnMessagePayload(identity, ownerWriter, observersWriter);

                var prefabHash = identity.IsPrefab ? identity.PrefabHash : default(int?);
                var sceneId = identity.IsSceneObject ? identity.SceneId : default(ulong?);
                var msg = new SpawnMessage
                {
                    NetId = identity.NetId,
                    SceneId = sceneId,
                    PrefabHash = prefabHash,
                    Payload = payload,
                };
                msg.SpawnValues = CreateSpawnValues(identity);

                // we have to send local/Owner values as their own message.
                // but observers can be sent using SendToMany to avoid copying bytes multiple times
                using var observersList = AutoPool<List<INetworkPlayer>>.Take();
                var observerPlayers = observersList.Item;
                Debug.Assert(observerPlayers.Count == 0);

                foreach (var player in players)
                {
                    if (identity.Owner == player)
                    {
                        // send to owner
                        msg.IsLocalPlayer = player.Identity == identity;
                        msg.IsOwner = true;
                        if (hasPayload)
                            msg.Payload = ownerWriter.ToArraySegment();

                        player.Send(msg);
                    }
                    else
                    {
                        // add all others players to list and send after
                        observerPlayers.Add(player);
                    }
                }

                // we only call this function with atleast 2 players, so there should always be observers
                Debug.Assert(observerPlayers.Count > 0);
                msg.IsLocalPlayer = false;
                msg.IsOwner = false;
                if (hasPayload)
                    msg.Payload = observersWriter.ToArraySegment();
                NetworkServer.SendToMany(observerPlayers, msg);
            }
        }

        private SpawnValues CreateSpawnValues(NetworkIdentity identity)
        {
            var settings = identity.SpawnSettings;
            SpawnValues values = default;

            // values in msg are nullable, so by default they are null
            // only set those values if the identity's settings say to send them
            if (settings.SendPosition) values.Position = identity.transform.localPosition;
            if (settings.SendRotation) values.Rotation = identity.transform.localRotation;
            if (settings.SendScale) values.Scale = identity.transform.localScale;
            if (settings.SendName) values.Name = identity.name;
            switch (settings.SendActive)
            {
                case SyncActiveOption.SyncWithServer:
                    values.SelfActive = identity.gameObject.activeSelf;
                    break;
                case SyncActiveOption.ForceEnable:
                    values.SelfActive = true;
                    break;
            }

            return values;
        }

        internal void SendRemoveAuthorityMessage(NetworkIdentity identity, INetworkPlayer previousOwner)
        {
            if (logger.LogEnabled()) logger.Log($"Server SendRemoveAuthorityMessage: name={identity.name} sceneId={identity.SceneId:X} netId={identity.NetId}");

            previousOwner.Send(new RemoveAuthorityMessage
            {
                NetId = identity.NetId,
            });
        }

        private static bool CreateSpawnMessagePayload(NetworkIdentity identity, PooledNetworkWriter ownerWriter, PooledNetworkWriter observersWriter)
        {
            // Only call OnSerializeAllSafely if there are NetworkBehaviours
            if (identity.NetworkBehaviours.Length == 0)
            {
                return false;
            }

            // serialize all components with initialState = true
            // (can be null if has none)
            identity.OnSerializeInitial(ownerWriter, observersWriter);

            return true;
        }

        /// <summary>
        /// Prefabs are not allowed to be spawned, they most be instantiated first
        /// <para>This check does nothing in builds</para>
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws in the editor if object is part of a prefab</exception>
        private static void ThrowIfPrefab(Object obj)
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

            var identity = gameObject.GetNetworkIdentity();
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

        private void DestroyObject(NetworkIdentity identity, bool destroyServerObject)
        {
            if (identity.NetId == 0)
            {
                if (logger.WarnEnabled()) logger.LogWarning($"DestroyObject was given identity without an id {identity}");
                return;
            }

            if (logger.LogEnabled()) logger.Log($"DestroyObject NetId={identity.NetId}");

            _server.World.RemoveIdentity(identity);
            identity.Owner?.RemoveOwnedObject(identity);

            identity.SendToRemoteObservers(new ObjectDestroyMessage { NetId = identity.NetId });

            identity.ClearObservers();
            if (_server.IsHost)
            {
                // see ClientObjectManager.UnSpawn for comments
                if (identity.HasAuthority)
                    identity.CallStopAuthority();

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
            {
                return false;
            }

#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.IsPersistent(identity.gameObject))
                return false;
#endif

            // If not a scene object
            return identity.IsSceneObject;
        }

        /// <summary>
        /// This causes NetworkIdentity objects in a scene to be spawned on a server.
        /// <para>
        ///     Calling SpawnObjects() causes all scene objects to be spawned.
        ///     It is like calling Spawn() for each of them.
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when server is not active</exception>
        public void SpawnSceneObjects()
        {
            // only if server active
            if (!_server || !_server.Active)
                throw new InvalidOperationException("Server was not active");

            using var spawnListWrapper = AutoPool<List<NetworkIdentity>>.Take();
            var spawnList = spawnListWrapper.Item;
            spawnList.Clear();

            foreach (var identity in Resources.FindObjectsOfTypeAll<NetworkIdentity>())
            {
                if (!ValidateSceneObject(identity))
                    continue;
                if (SceneObjectFilter != null && !SceneObjectFilter.Invoke(identity))
                    continue;
                spawnList.Add(identity);
            }

            // sort after filtering objects
            spawnList.Sort(NetworkIdentityComparer.instance);
            foreach (var identity in spawnList)
            {
                if (logger.LogEnabled()) logger.Log($"SpawnObjects sceneId:{identity.SceneId:X} name:{identity.gameObject.name}");
                Spawn(identity);
            }
            spawnList.Clear();
        }

        /// <summary>
        /// Sends spawn message for scene objects and other visible objects to the given player if it has a character
        /// <para>
        /// If there is a <see cref="Mirage.NetworkSceneManager"/> then this will be called after the client finishes loading the scene and sends <see cref="SceneReadyMessage"/>
        /// </para>
        /// </summary>
        /// <param name="player">The player to spawn objects for</param>
        // note: can't use optional param here because we need just NetworkPlayer version for event
        public void SpawnVisibleObjects(INetworkPlayer player)
        {
            SpawnVisibleObjects(player, false, (HashSet<NetworkIdentity>)null);
        }

        /// <summary>
        /// Sends spawn message for scene objects and other visible objects to the given player if it has a character
        /// </summary>
        /// <param name="player">The player to spawn objects for</param>
        /// <param name="ignoreHasCharacter">If true will spawn visible objects even if player does not have a spawned character yet</param>
        public void SpawnVisibleObjects(INetworkPlayer player, bool ignoreHasCharacter)
        {
            SpawnVisibleObjects(player, ignoreHasCharacter, (HashSet<NetworkIdentity>)null);
        }

        /// <summary>
        /// Sends spawn message for scene objects and other visible objects to the given player if it has a character
        /// </summary>
        /// <param name="player">The player to spawn objects for</param>
        /// <param name="ignoreHasCharacter">If true will spawn visible objects even if player does not have a spawned character yet</param>
        public void SpawnVisibleObjects(INetworkPlayer player, NetworkIdentity skip)
        {
            SpawnVisibleObjects(player, false, skip);
        }

        /// <summary>
        /// Sends spawn message for scene objects and other visible objects to the given player if it has a character
        /// </summary>
        /// <param name="player">The player to spawn objects for</param>
        /// <param name="ignoreHasCharacter">If true will spawn visible objects even if player does not have a spawned character yet</param>
        /// <param name="skip">NetworkIdentity to skip when spawning. Can be null</param>
        public void SpawnVisibleObjects(INetworkPlayer player, bool ignoreHasCharacter, NetworkIdentity skip)
        {
            using var skipWrapper = AutoPool<HashSet<NetworkIdentity>>.Take();
            var skipSet = skipWrapper.Item;
            skipSet.Clear();
            skipSet.Add(skip);
            SpawnVisibleObjects(player, ignoreHasCharacter, skipSet);
            skipSet.Clear();
        }

        /// <summary>
        /// Sends spawn message for scene objects and other visible objects to the given player if it has a character
        /// </summary>
        /// <param name="player">The player to spawn objects for</param>
        /// <param name="ignoreHasCharacter">If true will spawn visible objects even if player does not have a spawned character yet</param>
        /// <param name="skip">NetworkIdentity to skip when spawning. Can be null</param>
        public void SpawnVisibleObjects(INetworkPlayer player, bool ignoreHasCharacter, HashSet<NetworkIdentity> skip)
        {
            // remove all, so that it will send spawn message for objects destroyed in scene change
            player.RemoveAllVisibleObjects();

            if (!ignoreHasCharacter && !player.HasCharacter)
            {
                if (logger.LogEnabled()) logger.Log($"SpawnVisibleObjects: not spawning objects for {player} because it does not have a character");
                return;
            }

            if (!player.SceneIsReady)
            {
                // client needs to finish loading scene before we can spawn objects
                // otherwise it would not find scene objects.
                if (logger.LogEnabled()) logger.Log($"SpawnVisibleObjects: not spawning objects for {player} because scene not ready");
                return;
            }

            if (logger.LogEnabled()) logger.Log($"SpawnVisibleObjects: Checking Observers on {_server.World.SpawnedIdentities.Count} objects for player: {player}");

            // use sorted list, it will be sorted by netid
            // we want to make sure Identities are always spawned on same order on client
            var sortedIdentities = _server.World.GetSortedIdentities();

            // add connection to each nearby NetworkIdentity's observers, which
            // internally sends a spawn message for each one to the connection.
            foreach (var identity in sortedIdentities)
            {
                // allow for skips so that addCharacter doesn't send 2 spawn message for existing object
                if (skip != null && skip.Contains(identity))
                    continue;

                if (logger.LogEnabled()) logger.Log($"Checking Observers on server objects name='{identity.name}' netId={identity.NetId} sceneId={identity.SceneId:X}");

                var visible = identity.OnCheckObserver(player);
                if (visible)
                {
                    identity.AddObserver(player);
                }
            }
        }

        private sealed class NetworkIdentityComparer : IComparer<NetworkIdentity>
        {
            public static readonly NetworkIdentityComparer instance = new NetworkIdentityComparer();
            public int Compare(NetworkIdentity x, NetworkIdentity y)
            {
                return x.NetId.CompareTo(y.NetId);
            }
        }
    }
}
