using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Mirage.Logging;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage
{
    [AddComponentMenu("Network/ClientObjectManager")]
    [HelpURL("https://miragenet.github.io/Mirage/docs/reference/Mirage/ClientObjectManager")]
    [DisallowMultipleComponent]
    public class ClientObjectManager : MonoBehaviour
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(ClientObjectManager));

        internal RpcHandler _rpcHandler;
        internal SyncVarReceiver _syncVarReceiver;

        private NetworkClient _client;
        public NetworkClient Client => _client;

        /// <summary>
        /// List of prefabs that will be registered with the spawning system.
        /// <para>For each of these prefabs, ClientManager.RegisterPrefab() will be automatically invoke.</para>
        /// </summary>
        [Header("Prefabs")]
        public List<NetworkIdentity> spawnPrefabs = new List<NetworkIdentity>();

        /// <summary>
        /// A scriptable object that holds all the prefabs that will be registered with the spawning system.
        /// <para>For each of these prefabs, ClientManager.RegisterPrefab() will be automatically invoked.</para>
        /// </summary>
        public NetworkPrefabs NetworkPrefabs;

        /// <summary>
        /// A filter to only include certain scene objects when spawning.
        /// <para>Used by <see cref="PrepareToSpawnSceneObjects"/> when finding scene objects.</para>
        /// <para>If the filter is null, all valid scene objects will be included.</para>
        /// </summary>
        public Func<NetworkIdentity, bool> SceneObjectFilter { get; set; }

        /// <summary>
        /// This is a dictionary of the prefabs and delegates that are registered on the client with RegisterPrefab().
        /// <para>The key to the dictionary is the prefab asset Id.</para>
        /// </summary>
        internal readonly Dictionary<int, SpawnHandler> _handlers = new Dictionary<int, SpawnHandler>();

        /// <summary>
        /// List of handler that will be used
        /// </summary>
        internal readonly List<DynamicSpawnHandlerDelegate> _dynamicHandlers = new List<DynamicSpawnHandlerDelegate>();

        /// <summary>
        /// This is dictionary of the disabled NetworkIdentity objects in the scene that could be spawned by messages from the server.
        /// <para>The key to the dictionary is the NetworkIdentity sceneId.</para>
        /// </summary>
        public readonly Dictionary<ulong, NetworkIdentity> spawnableObjects = new Dictionary<ulong, NetworkIdentity>();

        internal readonly Dictionary<uint, PendingAsyncSpawn> pendingSpawn = new Dictionary<uint, PendingAsyncSpawn>();

        internal void ClientStarted(NetworkClient client)
        {
            if (_client != null && _client != client)
                throw new InvalidOperationException($"ClientObjectManager already in use by another NetworkClient, current:{_client}, new:{client}");

            _client = client;
            _client.Disconnected.AddListener(OnClientDisconnected);

            RegisterPrefabs(spawnPrefabs, true);
            RegisterPrefabs(NetworkPrefabs?.Prefabs, true);

            // prepare objects right away so objects in first scene can be spawned
            // if user changes scenes without NetworkSceneManager then they will need to manually call it again
            PrepareToSpawnSceneObjects();

            if (_client.IsHost)
            {
                RegisterHostHandlers();
            }
            else
            {
                RegisterMessageHandlers();
            }
        }

        private void OnClientDisconnected(ClientStoppedReason reason)
        {
            DestroyAllClientObjects();
            ClearSpawners();

            // reset for next run
            _client.Disconnected.RemoveListener(OnClientDisconnected);
            _client = null;
            _rpcHandler = null;
            _syncVarReceiver = null;

            foreach (var pending in pendingSpawn.Values)
                pending.Dispose();
            pendingSpawn.Clear();
        }

        internal void RegisterHostHandlers()
        {
            _client.MessageHandler.RegisterHandler<ObjectDestroyMessage>(msg => { });
            _client.MessageHandler.RegisterHandler<ObjectHideMessage>(msg => { });
            _client.MessageHandler.RegisterHandler<SpawnMessage>(OnHostClientSpawn);
            _client.MessageHandler.RegisterHandler<RemoveAuthorityMessage>(OnRemoveAuthority);
            _client.MessageHandler.RegisterHandler<RemoveCharacterMessage>(OnRemoveCharacter);
        }

        internal void RegisterMessageHandlers()
        {
            _client.MessageHandler.RegisterHandler<ObjectDestroyMessage>(OnObjectDestroy);
            _client.MessageHandler.RegisterHandler<ObjectHideMessage>(OnObjectHide);
            _client.MessageHandler.RegisterHandler<SpawnMessage>(OnSpawn);
            _client.MessageHandler.RegisterHandler<RemoveAuthorityMessage>(OnRemoveAuthority);
            _client.MessageHandler.RegisterHandler<RemoveCharacterMessage>(OnRemoveCharacter);

            _rpcHandler = new RpcHandler(_client.World, RpcInvokeType.ClientRpc);
            _client.MessageHandler.RegisterHandler<RpcReply>(_rpcHandler.OnReply); // no pending, but we still need to register it
            _client.MessageHandler.RegisterHandler<RpcMessage>(OnRpcMessage);
            _client.MessageHandler.RegisterHandler<RpcWithReplyMessage>(OnRpcWithReplyMessage);

            _syncVarReceiver = new SyncVarReceiver(_client.World);
            _client.MessageHandler.RegisterHandler<UpdateVarsMessage>(OnUpdateVarsMessage);
        }

        internal void OnRpcMessage(INetworkPlayer player, RpcMessage msg)
        {
            if (pendingSpawn.Count > 0 && pendingSpawn.TryGetValue(msg.NetId, out var pending)) // async spawning
            {
                if (logger.LogEnabled()) logger.Log($"Pending spawn for {msg.NetId}, storing new RpcMessage for later");
                pending.AddMessage(msg);
                return;
            }

            _rpcHandler.OnRpcMessage(player, msg);
        }
        internal void OnRpcWithReplyMessage(INetworkPlayer player, RpcWithReplyMessage msg)
        {
            if (pendingSpawn.Count > 0 && pendingSpawn.TryGetValue(msg.NetId, out var pending)) // async spawning
            {
                if (logger.LogEnabled()) logger.Log($"Pending spawn for {msg.NetId}, storing new RpcWithReplyMessage for later");
                pending.AddMessage(msg);
                return;
            }

            _rpcHandler.OnRpcWithReplyMessage(player, msg);
        }
        internal void OnUpdateVarsMessage(INetworkPlayer player, UpdateVarsMessage msg)
        {
            if (pendingSpawn.Count > 0 && pendingSpawn.TryGetValue(msg.NetId, out var pending)) // async spawning
            {
                if (logger.LogEnabled()) logger.Log($"Pending spawn for {msg.NetId}, storing new UpdateVarsMessage for later");
                pending.AddMessage(msg);
                return;
            }

            _syncVarReceiver.OnUpdateVarsMessage(player, msg);
        }


        private bool ConsiderForSpawning(NetworkIdentity identity)
        {
            // not spawned yet, not hidden, etc.?
            return !identity.IsSpawned &&
                   identity.gameObject.hideFlags != HideFlags.NotEditable &&
                   identity.gameObject.hideFlags != HideFlags.HideAndDontSave &&
                   identity.IsSceneObject;
        }

        // this is called from message handler for Owner message
        internal void InternalAddCharacter(NetworkIdentity identity)
        {
            if (!_client.Active)
            {
                throw new InvalidOperationException("Can't add character while client is not active");
            }

            _client.Player.Identity = identity;
        }

        /// <summary>
        /// Call this after loading/unloading a scene in the client after connection to register the spawnable objects
        /// </summary>
        public void PrepareToSpawnSceneObjects()
        {
            // clear up old scene,
            // we can just assume PrepareToSpawnSceneObjects is called after loading scene and call remove here
            Client.World?.RemoveDestroyedObjects();

            // add all unspawned NetworkIdentities to spawnable objects
            spawnableObjects.Clear();
            foreach (var identity in Resources.FindObjectsOfTypeAll<NetworkIdentity>())
            {
                if (!ConsiderForSpawning(identity))
                    continue;
                if (SceneObjectFilter != null && !SceneObjectFilter.Invoke(identity))
                    continue;

                spawnableObjects.Add(identity.SceneId, identity);
            }
        }

        #region Spawn Prefabs and handlers
        /// <summary>
        /// Calls <see cref="RegisterPrefab(NetworkIdentity)"/> on each object in the <paramref name="prefabs"/> collection
        /// </summary>
        /// <param name="prefabs"></param>
        /// <param name="skipExisting">Dont call <see cref="RegisterPrefab"/> for prefab's who's hash is already in the list of handlers. This can happen if custom handler is added for a prefab in the insepctor list</param>
        public void RegisterPrefabs(IEnumerable<NetworkIdentity> prefabs, bool skipExisting)
        {
            if (prefabs == null)
                return;

            foreach (var prefab in prefabs)
            {
                if (prefab == null)
                    continue;

                if (skipExisting)
                {
                    // check if the hash is ready in collection
                    // if it is, then skip
                    var prefabHash = prefab.PrefabHash;
                    if (_handlers.ContainsKey(prefabHash))
                        continue;
                }

                RegisterPrefab(prefab);
            }
        }


        /// <summary>
        /// Find the registered or dynamic handler for <paramref name="prefabHash"/>
        /// <para>Useful for debuggers</para>
        /// </summary>
        /// <param name="prefabHash">asset id of the prefab</param>
        /// <returns>true if prefab was registered</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="prefabHash"/> is 0</exception>
        /// <exception cref="SpawnObjectException">Thrown prefab </exception>
        public SpawnHandler GetSpawnHandler(int prefabHash)
        {
            ThrowIfZeroHash(prefabHash);

            if (_handlers.TryGetValue(prefabHash, out var registeredHandle))
            {
                if (logger.LogEnabled()) logger.Log($"Found Registered Handle for {prefabHash:X}");
                return registeredHandle;
            }

            foreach (var dynamicHandler in _dynamicHandlers)
            {
                var handler = dynamicHandler.Invoke(prefabHash);
                if (handler != null)
                {
                    if (logger.LogEnabled()) logger.Log($"Found Dynamic Handle for {prefabHash:X}");
                    return handler;
                }
            }

            ThrowMissingHandler(prefabHash);
            return null;
        }

        /// <summary>
        /// Registers a prefab with the spawning system.
        /// <para>
        /// When a NetworkIdentity object is spawned on the server with ServerObjectManager.Spawn(),
        /// the server will send a spawn message to the client with the PrefabHash.
        /// the client then finds the prefab registered with RegisterPrefab() to instantiate the client object.
        /// </para>
        /// <para>The ClientObjectManager has a list of spawnable prefabs, it uses this function to register them.</para>
        /// <para>The set of current spawnable object is available in the <see cref="spawnableObjects"/> dictionary.</para>
        /// </summary>
        /// <param name="identity">A Prefab that will be spawned.</param>
        /// <param name="newPrefabHash">A hash to be assigned to this prefab. This allows a dynamically created game object to be registered for an already known asset Id.</param>
        public void RegisterPrefab(NetworkIdentity identity, int newPrefabHash)
        {
            identity.PrefabHash = newPrefabHash;
            RegisterPrefab(identity);
        }

        /// <summary>
        /// Registers a prefab with the spawning system.
        /// <para>
        /// When a NetworkIdentity object is spawned on the server with ServerObjectManager.Spawn(),
        /// the server will send a spawn message to the client with the PrefabHash.
        /// the client then finds the prefab registered with RegisterPrefab() to instantiate the client object.
        /// </para>
        /// <para>The ClientObjectManager has a list of spawnable prefabs, it uses this function to register them.</para>
        /// <para>The set of current spawnable object is available in the <see cref="spawnableObjects"/> dictionary.</para>
        /// </summary>
        /// <param name="identity">A Prefab that will be spawned.</param>
        // todo does inheritdoc here? instead of having duplicate doc comments for each RegisterPrefab
        public void RegisterPrefab(NetworkIdentity identity)
        {
            ThrowIfZeroHash(identity);

            var prefabHash = identity.PrefabHash;
            ThrowIfExists(prefabHash, identity);

            if (logger.LogEnabled()) logger.Log($"Registering prefab '{identity.name}' as asset:{prefabHash:X}");
            _handlers[prefabHash] = new SpawnHandler(identity);
        }

        /// <summary>
        /// Registers an unspawn handler for a prefab
        /// <para>Should be called after RegisterPrefab</para>
        /// </summary>
        /// <param name="identity">Prefab to add handler for</param>
        /// <param name="unspawnHandler">A method to use as a custom un-spawnhandler on clients.</param>
        public void RegisterUnspawnHandler(NetworkIdentity identity, UnSpawnDelegate unspawnHandler)
        {
            if (unspawnHandler == null)
                throw new ArgumentNullException(nameof(unspawnHandler));

            ThrowIfZeroHash(identity);
            var prefabHash = identity.PrefabHash;
            if (!_handlers.ContainsKey(prefabHash))
            {
                throw new InvalidOperationException($"No prefab with hash {prefabHash:X}. Prefab must be registered before adding unspawn handler");
            }

            if (_handlers[prefabHash].Prefab == null)
            {
                throw new InvalidOperationException($"Existing handler for {prefabHash:X} was not a prefab. Prefab must be registered before adding unspawn handler");
            }

            if (logger.LogEnabled()) logger.Log($"Registering custom prefab '{identity.name}' as asset:{prefabHash:X} {unspawnHandler.Method.Name}");

            _handlers[prefabHash].AddUnspawnHandler(unspawnHandler);
        }

        /// <summary>
        /// Removes a registered spawn prefab that was setup with RegisterPrefab.
        /// </summary>
        /// <param name="identity">The prefab to be removed from registration.</param>
        public void UnregisterPrefab(NetworkIdentity identity)
        {
            var prefabHash = identity.PrefabHash;

            _handlers.Remove(prefabHash);
        }

        /// <summary>
        /// Registers custom handlers for a prefab with the spawning system.
        /// <para>
        /// When a NetworkIdentity object is spawned on the server with ServerObjectManager.Spawn(),
        /// the server will send a spawn message to the client with the PrefabHash.
        /// the client then finds the prefab registered with RegisterPrefab() to instantiate the client object.
        /// </para>
        /// <para>The ClientObjectManager has a list of spawnable prefabs, it uses this function to register them.</para>
        /// <para>The set of current spawnable object is available in the <see cref="spawnableObjects"/> dictionary.</para>
        /// </summary>
        /// <seealso cref="RegisterUnspawnHandler"/>
        /// <param name="identity">A Prefab that will be spawned.</param>
        /// <param name="spawnHandler">A method to use as a custom spawnhandler on clients.</param>
        /// <param name="unspawnHandler">A method to use as a custom un-spawnhandler on clients.</param>
        public void RegisterSpawnHandler(NetworkIdentity identity, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            // check identity has a hash before Validate so that there is good error meessage
            ThrowIfZeroHash(identity);
            var prefabHash = identity.PrefabHash;
            ValidateRegisterSpawnHandler(prefabHash, identity, spawnHandler, unspawnHandler);

            _handlers[prefabHash] = new SpawnHandler(identity, spawnHandler, unspawnHandler);
        }

        /// <summary>
        /// This is an advanced spawning function that registers a custom prefabHash with the UNET spawning system.
        /// <para>This can be used to register custom spawning methods for an prefabHash - instead of the usual method of registering spawning methods for a prefab. This should be used when no prefab exists for the spawned objects - such as when they are constructed dynamically at runtime from configuration data.</para>
        /// </summary>
        /// <param name="prefabHash"></param>
        /// <param name="spawnHandler">A method to use as a custom spawnhandler on clients.</param>
        /// <param name="unspawnHandler">A method to use as a custom un-spawnhandler on clients.</param>
        public void RegisterSpawnHandler(int prefabHash, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            ValidateRegisterSpawnHandler(prefabHash, null, spawnHandler, unspawnHandler);

            _handlers[prefabHash] = new SpawnHandler(spawnHandler, unspawnHandler);
        }

        public void RegisterSpawnHandler(NetworkIdentity identity, SpawnHandlerAsyncDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            // check identity has a hash before Validate so that there is good error meessage
            ThrowIfZeroHash(identity);
            var prefabHash = identity.PrefabHash;
            ValidateRegisterSpawnHandler(prefabHash, identity, spawnHandler, unspawnHandler);

            _handlers[prefabHash] = new SpawnHandler(identity, spawnHandler, unspawnHandler);
        }

        public void RegisterSpawnHandler(int prefabHash, SpawnHandlerAsyncDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            ValidateRegisterSpawnHandler(prefabHash, null, spawnHandler, unspawnHandler);

            _handlers[prefabHash] = new SpawnHandler(spawnHandler, unspawnHandler);
        }

        private void ValidateRegisterSpawnHandler(int prefabHash, NetworkIdentity prefab, Delegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            if (spawnHandler == null)
                throw new ArgumentNullException(nameof(spawnHandler));

            ThrowIfZeroHash(prefabHash);
            ThrowIfExists(prefabHash, prefab);

            if (logger.LogEnabled())
            {
                var spawnName = spawnHandler?.Method.Name ?? "<NULL>";
                var unspawnName = unspawnHandler?.Method.Name ?? "<NULL>";
                logger.Log($"RegisterSpawnHandler PrefabHash:'{prefabHash:X}' Spawn:{spawnName} UnSpawn:{unspawnName}");
            }
        }

        /// <summary>
        /// Removes a registered spawn handler function that was registered with RegisterSpawnHandler().
        /// </summary>
        /// <param name="prefabHash">The prefabHash for the handler to be removed for.</param>
        public void UnregisterSpawnHandler(int prefabHash)
        {
            _handlers.Remove(prefabHash);
        }

        /// <summary>
        /// This clears the registered spawn prefabs and spawn handler functions for this client.
        /// </summary>
        public void ClearSpawners()
        {
            _handlers.Clear();
        }

        public void RegisterDynamicSpawnHandler(DynamicSpawnHandlerDelegate dynamicHandler)
        {
            if (dynamicHandler == null)
                throw new ArgumentNullException(nameof(dynamicHandler));

            _dynamicHandlers.Add(dynamicHandler);
        }

        private static void ThrowIfZeroHash(int prefabHash)
        {
            if (prefabHash == 0)
                throw new ArgumentException("prefabHash is zero", nameof(prefabHash));
        }
        private static void ThrowIfZeroHash(NetworkIdentity identity)
        {
            if (identity.PrefabHash == 0)
            {
                throw new ArgumentException($"prefabHash is zero on {identity.name}", nameof(identity));
            }
        }
        private static void ThrowMissingHandler(int prefabHash)
        {
            throw new SpawnObjectException($"No prefab for {prefabHash:X}. did you forget to add it to the ClientObjectManager?");
        }
        private void ThrowIfExists(int prefabHash, NetworkIdentity newPrefab = null)
        {
            if (_handlers.ContainsKey(prefabHash))
            {
                var old = _handlers[prefabHash];
                // if trying to register the same prefab, dont throw
                if (newPrefab != null && old.Prefab == newPrefab)
                    return;

                var typeString = old.Prefab != null
                    ? "Prefab"
                    : "Handlers";

                throw new InvalidOperationException($"{typeString} with hash {prefabHash:X} already registered. " +
                    $"Unregister before adding new or prefabshandlers. Too add Unspawn handler to prefab use RegisterUnspawnHandler instead");
            }
        }

        #endregion

        private void UnSpawn(NetworkIdentity identity)
        {
            // have to store netid, so we can remove it from world, this is because NetworkReset will clear it
            var netId = identity.NetId;

            logger.Assert(!_client.IsHost, "UnSpawn should not be called in host mode");
            // it is useful to remove authority when destroying the object
            // this can be useful to clean up stuff after a local player is destroyed
            // call before StopClient, but dont reset the HasAuthority bool, people might want to use HasAuthority from stopclient or destroy
            if (identity.HasAuthority)
                identity.CallStopAuthority();

            identity.StopClient();

            if (_handlers.TryGetValue(identity.PrefabHash, out var handler) && handler.UnspawnHandler != null)
            {
                handler.UnspawnHandler.Invoke(identity);
                // need to call reset incase user puts object in pool and wants to re-use it later
                // it needs to be dont after the handler so user still has access to network fields while cleaning it up
                identity.NetworkReset();
            }
            else if (!identity.IsSceneObject)
            {
                Destroy(identity.gameObject);
            }
            else
            {
                identity.NetworkReset();
                identity.gameObject.SetActive(false);
                spawnableObjects[identity.SceneId] = identity;
            }

            _client.World.RemoveIdentity(netId);
        }

        /// <summary>
        /// Destroys all networked objects on the client.
        /// <para>This can be used to clean up when a network connection is closed.</para>
        /// </summary>
        public void DestroyAllClientObjects()
        {
            // dont destroy objects if we are server
            if (_client.IsHost)
            {
                if (logger.LogEnabled()) logger.Log("Skipping DestroyAllClientObjects because we are host client");
                return;
            }

            // create copy so they can be removed inside loop
            // allocation here are fine because is part of clean up
            var all = _client.World.SpawnedIdentities.ToArray();

            foreach (var identity in all)
            {
                // check if destroyed
                if (identity != null && identity.gameObject != null)
                {
                    // if not destoryed, do null unspawn with callbacks
                    UnSpawn(identity);
                }
                else
                {
                    // if destroyed, we just want to remove it from the dictionary
                    _client.World.RemoveIdentity(identity);
                }
            }

            Debug.Assert(_client.World.SpawnedIdentities.Count == 0, "All Identities should have been removed by UnSpawn above");
            _client.World.ClearSpawnedObjects();
        }

        private void ApplySpawnPayload(NetworkIdentity identity, SpawnMessage msg)
        {
            if (msg.PrefabHash.HasValue)
                identity.PrefabHash = msg.PrefabHash.Value;

            identity.SetClientValues(this, msg);

            if (msg.IsLocalPlayer)
                InternalAddCharacter(identity);

            // deserialize components if any payload
            // (Count is 0 if there were no components)
            if (msg.Payload.Count > 0)
            {
                using (var payloadReader = NetworkReaderPool.GetReader(msg.Payload, _client.World))
                {
                    identity.OnDeserializeAll(payloadReader, true);
                }
            }

            // objects spawned as part of initial state are started on a second pass
            identity.NotifyAuthority();
            identity.StartClient();
            CheckForLocalPlayer(identity);
        }

        internal void OnSpawn(SpawnMessage msg)
        {
            // pendingSpawn.Count check to skip dictionary lookup if empty
            if (pendingSpawn.Count > 0 && pendingSpawn.TryGetValue(msg.NetId, out var pending)) // async spawning
            {
                if (logger.LogEnabled()) logger.Log($"Pending spawn for {msg.NetId}, storing new SpawnMessage for later");
                pending.AddMessage(msg);
                return;
            }

            if (msg.PrefabHash == null && msg.SceneId == null)
                throw new SpawnObjectException($"Empty prefabHash and sceneId for netId: {msg.NetId}");

            if (logger.LogEnabled()) logger.Log($"[ClientObjectManager] Spawn: {msg}");

            // was the object already spawned?
            var existing = _client.World.TryGetIdentity(msg.NetId, out var identity);

            if (!existing)
            {
                //is the object on the prefab or scene object lists?
                if (msg.SceneId.HasValue)
                {
                    identity = SpawnSceneObject(msg);
                }
                else if (msg.PrefabHash.HasValue)
                {
                    var handler = GetSpawnHandler(msg.PrefabHash.Value);
                    if (handler.IsAsyncSpawn())
                    {
                        OnSpawnAsync(handler.HandlerAsync, msg).Forget();
                        return;
                    }
                    else
                    {
                        identity = SpawnPrefab(msg, handler);
                    }
                }
                else
                {
                    // note should never get here, because of check at start of method
                    // throw just incase
                    throw new SpawnObjectException("Spawn message did not have a SceneId or PrefabHash");
                }
            }

            AfterSpawn(msg, existing, identity);
        }

        private void AfterSpawn(SpawnMessage msg, bool alreadyExisted, NetworkIdentity spawnedIdentity)
        {
            // should never happen, Spawn methods above should throw instead
            Debug.Assert(spawnedIdentity != null, "AfterSpawn");

            if (spawnedIdentity.NetId != 0 && spawnedIdentity.NetId != msg.NetId)
                logger.LogWarning($"Spawn Identity already had a netId but SpawnMessage has a different NetId. Current Id={spawnedIdentity.NetId}, SpawnMessag Id={msg.NetId}");

            ApplySpawnPayload(spawnedIdentity, msg);

            // add after applying payload, but only if it is new object
            if (!alreadyExisted)
                _client.World.AddIdentity(msg.NetId, spawnedIdentity);
        }

        private async UniTaskVoid OnSpawnAsync(SpawnHandlerAsyncDelegate spawnHandler, SpawnMessage msg)
        {
            try
            {
                // copy payload into new buffer, because it will be release and re-used when this function awaits
                // todo can this be optimized
                using (var writer = NetworkWriterPool.GetWriter())
                {
                    writer.Write(msg.Payload);
                    // use read and write so that payload will look the same as original
                    using (var reader = NetworkReaderPool.GetReader(writer.ToArraySegment(), null))
                    {
                        msg.Payload = reader.Read<ArraySegment<byte>>();
                        var pending = new PendingAsyncSpawn(msg.NetId);
                        pendingSpawn.Add(msg.NetId, pending);

                        try
                        {
                            var identity = await spawnHandler.Invoke(msg);
                            if (identity == null)
                                throw new SpawnObjectException($"Async Spawn handler for prefabHash={msg.PrefabHash:X} returned null");
                            AfterSpawn(msg, false, identity);

                            // IMPORTANT: remove from pendingSpawn first, so that methods will be invoked instead of adding to pending a 2nd time
                            pendingSpawn.Remove(msg.NetId);
                            pending.ApplyAll(this);
                        }
                        finally
                        {
                            // ensure we always clean up, even if spawn fails
                            pending.Dispose();
                            // double check it is removed, incase we throw while spawning
                            pendingSpawn.Remove(msg.NetId);
                        }
                    }
                }
            }
            // todo, should we allow async message handler? then we can just try/catch in there. Would also simplify spawnasync
            // this async is called from message handler, so we want to catch and maybe disconnect
            catch (Exception e)
            {
                _client.MessageHandler.LogAndCheckDisconnect(_client.Player, e);
            }
        }

        private NetworkIdentity SpawnPrefab(SpawnMessage msg, SpawnHandler handler)
        {
            var spawnHandler = handler.Handler;
            if (spawnHandler != null)
            {
                if (logger.LogEnabled()) logger.Log($"[ClientObjectManager] Custom handler for netid:{msg.NetId}");

                var obj = spawnHandler.Invoke(msg);
                if (obj == null)
                    throw new SpawnObjectException($"Spawn handler for prefabHash={msg.PrefabHash:X} returned null");
                return obj;
            }

            // double check async handler is null
            Debug.Assert(handler.HandlerAsync == null);

            var prefab = handler.Prefab;

            if (logger.LogEnabled()) logger.Log($"[ClientObjectManager] Instantiate Prefab for netid:{msg.NetId}, hash:{msg.PrefabHash.Value:X}, prefab:{prefab.name}");

            // we need to set position and rotation here incase that their values are used from awake/onenable
            var pos = msg.SpawnValues.Position ?? prefab.transform.position;
            var rot = msg.SpawnValues.Rotation ?? prefab.transform.rotation;
            return Instantiate(prefab, pos, rot);
        }

        internal NetworkIdentity SpawnSceneObject(SpawnMessage msg)
        {
            var sceneId = msg.SceneId.Value;

            if (spawnableObjects.TryGetValue(sceneId, out var foundSceneObject))
            {
                spawnableObjects.Remove(sceneId);

                if (foundSceneObject == null)
                    throw new SpawnObjectException($"Scene object is null, sceneId={msg.SceneId:X}, NetId={msg.NetId}");

                if (logger.LogEnabled()) logger.Log($"[ClientObjectManager] Found scene object for netid:{msg.NetId}, sceneId:{msg.SceneId.Value:X}, obj:{foundSceneObject}");
                return foundSceneObject;
            }

            // failed to spawn
            var errorMsg = $"Could not find scene object with sceneId={msg.SceneId:X}, NetId={msg.NetId}. Enable full logs in project settings to see current list of SpawnableObjects in the scene";
            // dump the whole spawnable objects dict for easier debugging
            if (logger.LogEnabled())
            {
                var builder = new StringBuilder();
                builder.AppendLine($"{errorMsg} SpawnableObjects.Count={spawnableObjects.Count}");

                foreach (var kvp in spawnableObjects)
                    builder.AppendLine($"Spawnable: SceneId={kvp.Key:X} name={kvp.Value.name}");

                logger.Log(builder.ToString());
            }

            throw new SpawnObjectException(errorMsg);
        }

        internal void OnRemoveAuthority(RemoveAuthorityMessage msg)
        {
            if (pendingSpawn.Count > 0 && pendingSpawn.TryGetValue(msg.NetId, out var pending)) // async spawning
            {
                if (logger.LogEnabled()) logger.Log($"Pending spawn for {msg.NetId}, storing new RemoveAuthorityMessage for later");
                pending.AddMessage(msg);
                return;
            }

            if (logger.LogEnabled()) logger.Log($"Client remove auth handler");

            // was the object already spawned?
            var existing = _client.World.TryGetIdentity(msg.NetId, out var identity);

            if (!existing)
            {
                logger.LogWarning($"Could not find object with id {msg.NetId}");
                return;
            }

            identity.HasAuthority = false;

            identity.NotifyAuthority();
        }

        internal void OnRemoveCharacter(RemoveCharacterMessage msg)
        {
            if (logger.LogEnabled()) logger.Log($"Client remove character handler");

            var player = _client.Player;
            var identity = player.Identity;

            if (identity == null)
            {
                logger.LogWarning($"Could not find player's character");
                return;
            }

            player.Identity = null;
            identity.HasAuthority = msg.KeepAuthority;

            identity.NotifyAuthority();
        }

        internal void OnObjectHide(ObjectHideMessage msg)
        {
            if (pendingSpawn.Count > 0 && pendingSpawn.TryGetValue(msg.NetId, out var pending)) // async spawning
            {
                if (logger.LogEnabled()) logger.Log($"Pending spawn for {msg.NetId}, storing new ObjectHideMessage for later");
                pending.AddMessage(msg);
                return;
            }

            DestroyObject(msg.NetId);
        }

        internal void OnObjectDestroy(ObjectDestroyMessage msg)
        {
            if (pendingSpawn.Count > 0 && pendingSpawn.TryGetValue(msg.NetId, out var pending)) // async spawning
            {
                if (logger.LogEnabled()) logger.Log($"Pending spawn for {msg.NetId}, storing new ObjectDestroyMessage for later");
                pending.AddMessage(msg);
                return;
            }

            DestroyObject(msg.NetId);
        }

        private void DestroyObject(uint netId)
        {
            if (logger.LogEnabled()) logger.Log($"OnObjectDestroy netId:{netId}");

            if (_client.World.TryGetIdentity(netId, out var localObject))
            {
                UnSpawn(localObject);
            }
            else
            {
                logger.LogWarning("Did not find target for destroy message for " + netId);
            }
        }

        internal void OnHostClientSpawn(SpawnMessage msg)
        {
            if (_client.World.TryGetIdentity(msg.NetId, out var localObject))
            {
                if (msg.IsLocalPlayer)
                    InternalAddCharacter(localObject);

                localObject.SetClientValues(this, msg);

                localObject.NotifyAuthority();
                localObject.StartClient();
                CheckForLocalPlayer(localObject);
            }
        }

        private void CheckForLocalPlayer(NetworkIdentity identity)
        {
            if (identity && identity == _client.Player?.Identity)
            {
                // Set isLocalPlayer to true on this NetworkIdentity and trigger OnStartLocalPlayer in all scripts on the same GO
                identity.StartLocalPlayer();

                if (logger.LogEnabled()) logger.Log($"OnOwnerMessage player={identity.name}");
            }
        }



#if UNITY_EDITOR
        private readonly Dictionary<int, NetworkIdentity> _validateCache = new Dictionary<int, NetworkIdentity>();

        private void OnValidate()
        {
            // clear before just incase it didn't clear last time
            _validateCache.Clear();

            ValidatePrefabs(spawnPrefabs);
            ValidatePrefabs(NetworkPrefabs?.Prefabs);

            // clear after so unity can release prefabs if it wants to
            _validateCache.Clear();
        }

        private void ValidatePrefabs(IEnumerable<NetworkIdentity> prefabs)
        {
            if (prefabs == null)
                return;

            foreach (var prefab in prefabs)
            {
                if (prefab == null)
                    continue;

                var hash = prefab.PrefabHash;
                if (_validateCache.TryGetValue(hash, out var existing))
                {
                    if (prefab == existing)
                        continue;

                    var pathA = UnityEditor.AssetDatabase.GetAssetPath(prefab);
                    var pathB = UnityEditor.AssetDatabase.GetAssetPath(existing);
                    logger.Assert(prefab.PrefabHash == existing.PrefabHash);
                    logger.LogError($"2 prefabs had the same hash:'{hash}', A:'{prefab.name}' B:'{existing.name}'. Path A:{pathA} Path B:{pathB}");
                }

                _validateCache[hash] = prefab;
            }
        }
#endif
    }

    public class SpawnHandler
    {
        public readonly NetworkIdentity Prefab;

        public readonly SpawnHandlerDelegate Handler;
        public readonly SpawnHandlerAsyncDelegate HandlerAsync;

        public UnSpawnDelegate UnspawnHandler { get; private set; }

        public SpawnHandler(NetworkIdentity prefab)
        {
            Prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
        }

        public SpawnHandler(SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            Handler = spawnHandler ?? throw new ArgumentNullException(nameof(spawnHandler));
            // unspawn is allowed to be null
            UnspawnHandler = unspawnHandler;
        }

        public SpawnHandler(NetworkIdentity prefab, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            Prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
            Handler = spawnHandler ?? throw new ArgumentNullException(nameof(spawnHandler));
            // unspawn is allowed to be null
            UnspawnHandler = unspawnHandler;
        }

        public SpawnHandler(SpawnHandlerAsyncDelegate spawnHandlerAsync, UnSpawnDelegate unspawnHandler)
        {
            HandlerAsync = spawnHandlerAsync ?? throw new ArgumentNullException(nameof(spawnHandlerAsync));
            // unspawn is allowed to be null
            UnspawnHandler = unspawnHandler;
        }

        public SpawnHandler(NetworkIdentity prefab, SpawnHandlerAsyncDelegate spawnHandlerAsync, UnSpawnDelegate unspawnHandler)
        {
            Prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
            HandlerAsync = spawnHandlerAsync ?? throw new ArgumentNullException(nameof(spawnHandlerAsync));
            // unspawn is allowed to be null
            UnspawnHandler = unspawnHandler;
        }

        public void AddUnspawnHandler(UnSpawnDelegate unspawnHandler)
        {
            if (Prefab == null)
            {
                throw new InvalidOperationException("Can only add unspawn handler if prefab is already registered");
            }

            UnspawnHandler = unspawnHandler;
        }

        public bool IsAsyncSpawn()
        {
            return HandlerAsync != null;
        }
    }

    public class PendingAsyncSpawn : IDisposable
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(ClientObjectManager));

        public readonly uint NetId;

        /// <summary>
        /// extra messages to apply in order after spawning is complete
        /// </summary>
        private List<MessageType> _messageTypes;
        private PooledNetworkWriter _messageBytes;

        public int PendingCount => _messageTypes?.Count ?? 0;

        public PendingAsyncSpawn(uint netid)
        {
            NetId = netid;
        }

        public void AddMessage(ObjectDestroyMessage message) => AddMessageInternal(MessageType.ObjectDestroyMessage, message);
        public void AddMessage(ObjectHideMessage message) => AddMessageInternal(MessageType.ObjectHideMessage, message);
        public void AddMessage(SpawnMessage message) => AddMessageInternal(MessageType.SpawnMessage, message);
        public void AddMessage(RemoveAuthorityMessage message) => AddMessageInternal(MessageType.RemoveAuthorityMessage, message);
        public void AddMessage(RpcMessage message) => AddMessageInternal(MessageType.RpcMessage, message);
        public void AddMessage(RpcWithReplyMessage message) => AddMessageInternal(MessageType.RpcWithReplyMessage, message);
        public void AddMessage(UpdateVarsMessage message) => AddMessageInternal(MessageType.UpdateVarsMessage, message);

        private void AddMessageInternal<T>(MessageType messageType, T message) where T : struct
        {
            _messageTypes ??= new List<MessageType>();
            _messageBytes ??= NetworkWriterPool.GetWriter();

            _messageTypes.Add(messageType);
            _messageBytes.Write(message);

            // log warning for every 20, we dont want to be storing too many message here
            // it also indicates that spawning failed or is taking too long
            if (_messageTypes.Count % 20 == 0)
            {
                if (logger.WarnEnabled())
                    logger.LogWarning($"Pending message count for {NetId} is {_messageTypes.Count}. Make sure async spawned object isn't being sent too many message before it is spawned");
            }
        }

        public void ApplyAll(ClientObjectManager clientObjectManager)
        {
            if (_messageTypes == null)
                return;

            // use read and write so that payload will look the same as original
            var world = clientObjectManager.Client.World;
            var player = clientObjectManager.Client.Player;
            using (var reader = NetworkReaderPool.GetReader(_messageBytes.ToArraySegment(), world))
            {
                foreach (var messageType in _messageTypes)
                {
                    switch (messageType)
                    {
                        case MessageType.ObjectDestroyMessage:
                            {
                                var msg = reader.Read<ObjectDestroyMessage>();
                                clientObjectManager.OnObjectDestroy(msg);

                                break;
                            }
                        case MessageType.ObjectHideMessage:
                            {
                                var msg = reader.Read<ObjectHideMessage>();
                                clientObjectManager.OnObjectHide(msg);

                                break;
                            }
                        case MessageType.SpawnMessage:
                            {
                                var msg = reader.Read<SpawnMessage>();
                                clientObjectManager.OnSpawn(msg);

                                break;
                            }
                        case MessageType.RemoveAuthorityMessage:
                            {
                                var msg = reader.Read<RemoveAuthorityMessage>();
                                clientObjectManager.OnRemoveAuthority(msg);

                                break;
                            }

                        case MessageType.RpcMessage:
                            {
                                var msg = reader.Read<RpcMessage>();
                                clientObjectManager.OnRpcMessage(player, msg);
                                break;
                            }
                        case MessageType.RpcWithReplyMessage:
                            {
                                var msg = reader.Read<RpcWithReplyMessage>();
                                clientObjectManager.OnRpcWithReplyMessage(player, msg);
                                break;
                            }

                        case MessageType.UpdateVarsMessage:
                            {
                                var msg = reader.Read<UpdateVarsMessage>();
                                clientObjectManager.OnUpdateVarsMessage(player, msg);
                                break;
                            }

                        default:
                            {
                                // should never happen, but log just incase we dont add case here
                                logger.LogError($"Unhandled MessageType {messageType} in PendingAsyncSpawn.ApplyAll");
                                break;
                            }
                    }
                }
            }
        }

        public void Dispose()
        {
            _messageTypes = null;
            _messageBytes?.Release();
            _messageBytes = null;
        }

        public enum MessageType
        {
            ObjectDestroyMessage,
            ObjectHideMessage,
            SpawnMessage,
            RemoveAuthorityMessage,

            RpcMessage,
            RpcWithReplyMessage,

            UpdateVarsMessage,
        }
    }
}
