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
        /// <summary>HashSet for NetworkIdentity that can be re-used without allocation</summary>
        private static HashSet<NetworkIdentity> _skipCache = new HashSet<NetworkIdentity>();
        /// <summary>HashSet for NetworkIdentity that can be re-used without allocation</summary>
        private static List<NetworkIdentity> _spawnCache = new List<NetworkIdentity>();

        internal RpcHandler _rpcHandler;

        private NetworkServer _server;
        public NetworkServer Server => _server;

        public INetIdGenerator NetIdGenerator;
        private uint _nextNetworkId = 1;

        private uint GetNextNetworkId() => NetIdGenerator?.GenerateNetId() ?? checked(_nextNetworkId++);

        public INetworkVisibility DefaultVisibility { get; private set; }

        internal void ServerStarted(NetworkServer server)
        {
            if (_server != null && _server != server)
                throw new InvalidOperationException($"ServerObjectManager already in use by another NetworkServer, current:{_server}, new:{server}");

            _server = server;
            _server.Stopped.AddListener(OnServerStopped);

            DefaultVisibility = new AlwaysVisible(server);

            _rpcHandler = new RpcHandler(_server.MessageHandler, _server.World, RpcInvokeType.ServerRpc);
        }

        private void OnServerStopped()
        {
            // todo dont send messages on server stop, only reset NI
            foreach (var obj in _server.World.SpawnedIdentities.Reverse())
            {
                // Unspawn all, but only destroy non-scene objects on server
                DestroyObject(obj, !obj.IsSceneObject);
            }

            _server.World.ClearSpawnedObjects();
            // reset so ids stay small in each session
            _nextNetworkId = 1;

            // clear server after stopping
            _server.Stopped.RemoveListener(OnServerStopped);
            _server = null;
        }

        internal void SpawnOrActivate()
        {
            if (!_server || !_server.Active)
            {
                logger.LogWarning("SpawnOrActivate called when server was not active");
                return;
            }

            SpawnSceneObjects();

            // host mode?
            if (_server.IsHost)
            {
                StartHostClientObjects();
            }
        }

        /// <summary>
        /// Loops spawned collection for NetworkIdentities that are not IsClient and calls StartClient().
        /// </summary>
        // todo can this function be removed? do we only need to run it when host connects?
        private void StartHostClientObjects()
        {
            foreach (var identity in _server.World.SpawnedIdentities)
            {
                if (!identity.IsClient)
                {
                    if (logger.LogEnabled()) logger.Log("ActivateHostScene " + identity.NetId + " " + identity);

                    identity.StartClient();
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

        internal void HideToPlayer(NetworkIdentity identity, INetworkPlayer player)
        {
            var visiblity = identity.Visibility;
            if (visiblity is NetworkVisibility networkVisibility)
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
        ///     <see cref="NetworkIdentity.PrefabHash"/> can only be set on an identity if the current value is Empty
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

                var payload = CreateSpawnMessagePayload(isOwner, identity, ownerWriter, observersWriter);

                var prefabHash = identity.IsPrefab ? identity.PrefabHash : default(int?);
                var sceneId = identity.IsSceneObject ? identity.SceneId : default(ulong?);
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

        private static ArraySegment<byte> CreateSpawnMessagePayload(bool isOwner, NetworkIdentity identity, PooledNetworkWriter ownerWriter, PooledNetworkWriter observersWriter)
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
            var payload = isOwner ?
                ownerWriter.ToArraySegment() :
                observersWriter.ToArraySegment();

            return payload;
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
            if (logger.LogEnabled()) logger.Log("DestroyObject instance:" + identity.NetId);

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

            var identities = Resources.FindObjectsOfTypeAll<NetworkIdentity>();
            Array.Sort(identities, new NetworkIdentityComparer());

            foreach (var identity in identities)
            {
                if (ValidateSceneObject(identity))
                {
                    if (logger.LogEnabled()) logger.Log($"SpawnObjects sceneId:{identity.SceneId:X} name:{identity.gameObject.name}");

                    Spawn(identity);
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
        // note: can't use optional param here because we need just NetworkPlayer version for event
        public void SpawnVisibleObjects(INetworkPlayer player)
        {
            SpawnVisibleObjects(player, false, (HashSet<NetworkIdentity>)null);
        }

        /// <summary>
        /// Sends spawn message for scene objects and other visible objects to the given player if it has a character
        /// </summary>
        /// <param name="player">The player to spawn objects for</param>
        /// <param name="ignoreHasCharacter">If true will spawn visibile objects even if player does not have a spawned character yet</param>
        public void SpawnVisibleObjects(INetworkPlayer player, bool ignoreHasCharacter)
        {
            SpawnVisibleObjects(player, ignoreHasCharacter, (HashSet<NetworkIdentity>)null);
        }

        /// <summary>
        /// Sends spawn message for scene objects and other visible objects to the given player if it has a character
        /// </summary>
        /// <param name="player">The player to spawn objects for</param>
        /// <param name="ignoreHasCharacter">If true will spawn visibile objects even if player does not have a spawned character yet</param>
        public void SpawnVisibleObjects(INetworkPlayer player, NetworkIdentity skip)
        {
            SpawnVisibleObjects(player, false, skip);
        }

        /// <summary>
        /// Sends spawn message for scene objects and other visible objects to the given player if it has a character
        /// </summary>
        /// <param name="player">The player to spawn objects for</param>
        /// <param name="ignoreHasCharacter">If true will spawn visibile objects even if player does not have a spawned character yet</param>
        /// <param name="skip">NetworkIdentity to skip when spawning. Can be null</param>
        public void SpawnVisibleObjects(INetworkPlayer player, bool ignoreHasCharacter, NetworkIdentity skip)
        {
            _skipCache.Clear();
            _skipCache.Add(skip);
            SpawnVisibleObjects(player, ignoreHasCharacter, _skipCache);
        }

        /// <summary>
        /// Sends spawn message for scene objects and other visible objects to the given player if it has a character
        /// </summary>
        /// <param name="player">The player to spawn objects for</param>
        /// <param name="ignoreHasCharacter">If true will spawn visible objects even if player does not have a spawned character yet</param>
        /// <param name="skip">NetworkIdentity to skip when spawning. Can be null</param>
        public void SpawnVisibleObjects(INetworkPlayer player, bool ignoreHasCharacter, HashSet<NetworkIdentity> skip)
        {
            // todo Call player.RemoveAllVisibleObjects() first so that it will send spawn message for objects destroyed in scene change

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

            // add to cache first, so SpawnedIdentities can be modified inside loop without throwing
            _spawnCache.Clear();
            _spawnCache.AddRange(_server.World.SpawnedIdentities);
            // add connection to each nearby NetworkIdentity's observers, which
            // internally sends a spawn message for each one to the connection.
            foreach (var identity in _spawnCache)
            {
                // allow for skips so that addCharacter doesn't send 2 spawn message for existing object
                if (skip != null && skip.Contains(identity))
                    continue;

                // todo, do we only need to spawn active objects here? or all objects?
                if (identity.gameObject.activeSelf)
                {
                    if (logger.LogEnabled()) logger.Log($"Checking Observers on server objects name='{identity.name}' netId={identity.NetId} sceneId={identity.SceneId:X}");

                    var visible = identity.OnCheckObserver(player);
                    if (visible)
                    {
                        identity.AddObserver(player);
                    }
                }
            }

            _spawnCache.Clear();
        }

        private sealed class NetworkIdentityComparer : IComparer<NetworkIdentity>
        {
            public int Compare(NetworkIdentity x, NetworkIdentity y)
            {
                return x.NetId.CompareTo(y.NetId);
            }
        }
    }
}
