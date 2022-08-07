using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Mirage.Logging;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Mirage
{

    [AddComponentMenu("Network/ClientObjectManager")]
    [DisallowMultipleComponent]
    public class ClientObjectManager : MonoBehaviour, IClientObjectManager
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(ClientObjectManager));

        [FormerlySerializedAs("client")]
        public NetworkClient Client;
        [FormerlySerializedAs("networkSceneManager")]
        public NetworkSceneManager NetworkSceneManager;

        // spawn handlers. internal for testing purposes. do not use directly.
        internal readonly Dictionary<int, SpawnHandlerDelegate> _spawnHandlers = new Dictionary<int, SpawnHandlerDelegate>();
        internal readonly Dictionary<int, UnSpawnDelegate> _unspawnHandlers = new Dictionary<int, UnSpawnDelegate>();

        /// <summary>
        /// List of prefabs that will be registered with the spawning system.
        /// <para>For each of these prefabs, ClientManager.RegisterPrefab() will be automatically invoke.</para>
        /// </summary>
        [Header("Prefabs")]
        public List<NetworkIdentity> spawnPrefabs = new List<NetworkIdentity>();

        /// <summary>
        /// This is a dictionary of the prefabs that are registered on the client with ClientScene.RegisterPrefab().
        /// <para>The key to the dictionary is the prefab asset Id.</para>
        /// </summary>
        internal readonly Dictionary<int, NetworkIdentity> _prefabs = new Dictionary<int, NetworkIdentity>();

        /// <summary>
        /// This is dictionary of the disabled NetworkIdentity objects in the scene that could be spawned by messages from the server.
        /// <para>The key to the dictionary is the NetworkIdentity sceneId.</para>
        /// </summary>
        public readonly Dictionary<ulong, NetworkIdentity> spawnableObjects = new Dictionary<ulong, NetworkIdentity>();

        internal ServerObjectManager _serverObjectManager;
        private SyncVarReceiver _syncVarReceiver;

        public void Start()
        {
            if (Client != null)
            {
                Client.Connected.AddListener(OnClientConnected);
                Client.Disconnected.AddListener(OnClientDisconnected);

                if (NetworkSceneManager != null)
                    NetworkSceneManager.OnClientFinishedSceneChange.AddListener(OnFinishedSceneChange);
            }
            else
            {
                Debug.LogWarning($"Client is null for ClientObjectManager on {gameObject.name}");
            }
        }

#if UNITY_EDITOR
        private readonly Dictionary<int, NetworkIdentity> _validateCache = new Dictionary<int, NetworkIdentity>();

        private void OnValidate()
        {
            _validateCache.Clear();
            foreach (var prefab in spawnPrefabs)
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

        private void OnClientConnected(INetworkPlayer player)
        {
            _syncVarReceiver = new SyncVarReceiver(Client, Client.World);
            RegisterSpawnPrefabs();

            // prepare objects right away so objects in first scene can be spawned
            // if user changes scenes without NetworkSceneManager then they will need to manually call it again
            PrepareToSpawnSceneObjects();

            if (Client.IsLocalClient)
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
            ClearSpawners();
            DestroyAllClientObjects();
            _syncVarReceiver = null;
        }

        private void OnFinishedSceneChange(Scene scene, SceneOperation sceneOperation)
        {
            Client.World.RemoveDestroyedObjects();

            PrepareToSpawnSceneObjects();
        }

        internal void RegisterHostHandlers()
        {
            Client.MessageHandler.RegisterHandler<ObjectDestroyMessage>(msg => { });
            Client.MessageHandler.RegisterHandler<ObjectHideMessage>(msg => { });
            Client.MessageHandler.RegisterHandler<SpawnMessage>(OnHostClientSpawn);
            Client.MessageHandler.RegisterHandler<RemoveAuthorityMessage>(OnRemoveAuthority);
            Client.MessageHandler.RegisterHandler<RemoveCharacterMessage>(OnRemoveCharacter);
            Client.MessageHandler.RegisterHandler<ServerRpcReply>(msg => { });
            Client.MessageHandler.RegisterHandler<RpcMessage>(msg => { });
        }

        internal void RegisterMessageHandlers()
        {
            Client.MessageHandler.RegisterHandler<ObjectDestroyMessage>(OnObjectDestroy);
            Client.MessageHandler.RegisterHandler<ObjectHideMessage>(OnObjectHide);
            Client.MessageHandler.RegisterHandler<SpawnMessage>(OnSpawn);
            Client.MessageHandler.RegisterHandler<RemoveAuthorityMessage>(OnRemoveAuthority);
            Client.MessageHandler.RegisterHandler<RemoveCharacterMessage>(OnRemoveCharacter);
            Client.MessageHandler.RegisterHandler<ServerRpcReply>(OnServerRpcReply);
            Client.MessageHandler.RegisterHandler<RpcMessage>(OnRpcMessage);
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
            if (!Client.Active)
            {
                throw new InvalidOperationException("Can't add character while client is not active");
            }

            Client.Player.Identity = identity;
        }

        /// <summary>
        /// Call this after loading/unloading a scene in the client after connection to register the spawnable objects
        /// </summary>
        public void PrepareToSpawnSceneObjects()
        {
            // add all unspawned NetworkIdentities to spawnable objects
            spawnableObjects.Clear();
            var sceneObjects =
                Resources.FindObjectsOfTypeAll<NetworkIdentity>()
                               .Where(ConsiderForSpawning);

            foreach (var obj in sceneObjects)
            {
                spawnableObjects.Add(obj.SceneId, obj);
            }
        }

        #region Spawn Prefabs
        private void RegisterSpawnPrefabs()
        {
            for (var i = 0; i < spawnPrefabs.Count; i++)
            {
                var prefab = spawnPrefabs[i];
                if (prefab != null)
                {
                    RegisterPrefab(prefab);
                }
            }
        }

        /// <summary>
        /// Find the registered prefab for this asset id.
        /// Useful for debuggers
        /// </summary>
        /// <param name="prefabHash">asset id of the prefab</param>
        /// <returns>true if prefab was registered</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="prefabHash"/> is 0</exception>
        /// <exception cref="SpawnObjectException">Thrown prefab </exception>
        public NetworkIdentity GetPrefab(int prefabHash)
        {
            if (prefabHash == 0)
                throw new ArgumentException("prefabHash was 0", nameof(prefabHash));

            if (_prefabs.TryGetValue(prefabHash, out var identity))
                return identity;

            throw new SpawnObjectException($"No prefab for {prefabHash:X}. did you forget to add it to the ClientObjectManager?");
        }

        /// <summary>
        /// Registers a prefab with the spawning system.
        /// <para>When a NetworkIdentity object is spawned on a server with NetworkServer.SpawnObject(), and the prefab that the object was created from was registered with RegisterPrefab(), the client will use that prefab to instantiate a corresponding client object with the same netId.</para>
        /// <para>The ClientObjectManager has a list of spawnable prefabs, it uses this function to register those prefabs with the ClientScene.</para>
        /// <para>The set of current spawnable object is available in the ClientScene static member variable ClientScene.prefabs, which is a dictionary of PrefabHash and prefab references.</para>
        /// </summary>
        /// <param name="identity">A Prefab that will be spawned.</param>
        /// <param name="newPrefabHash">A hash to be assigned to this prefab. This allows a dynamically created game object to be registered for an already known asset Id.</param>
        public void RegisterPrefab(NetworkIdentity identity, int newPrefabHash)
        {
            identity.PrefabHash = newPrefabHash;

            if (logger.LogEnabled()) logger.Log($"Registering prefab '{identity.name}' as asset:{identity.PrefabHash:X}");
            _prefabs[identity.PrefabHash] = identity;
        }

        /// <summary>
        /// Registers a prefab with the spawning system.
        /// <para>When a NetworkIdentity object is spawned on a server with NetworkServer.SpawnObject(), and the prefab that the object was created from was registered with RegisterPrefab(), the client will use that prefab to instantiate a corresponding client object with the same netId.</para>
        /// <para>The ClientObjectManager has a list of spawnable prefabs, it uses this function to register those prefabs with the ClientScene.</para>
        /// <para>The set of current spawnable object is available in the ClientScene static member variable ClientScene.prefabs, which is a dictionary of PrefabHash and prefab references.</para>
        /// </summary>
        /// <param name="identity">A Prefab that will be spawned.</param>
        public void RegisterPrefab(NetworkIdentity identity)
        {
            if (logger.LogEnabled()) logger.Log($"Registering prefab '{identity.name}' as asset:{identity.PrefabHash:X}");
            _prefabs[identity.PrefabHash] = identity;
        }

        /// <summary>
        /// Registers a prefab with the spawning system.
        /// <para>When a NetworkIdentity object is spawned on a server with NetworkServer.SpawnObject(), and the prefab that the object was created from was registered with RegisterPrefab(), the client will use that prefab to instantiate a corresponding client object with the same netId.</para>
        /// <para>The ClientObjectManager has a list of spawnable prefabs, it uses this function to register those prefabs with the ClientScene.</para>
        /// <para>The set of current spawnable object is available in the ClientScene static member variable ClientScene.prefabs, which is a dictionary of PrefabHash and prefab references.</para>
        /// </summary>
        /// <param name="identity">A Prefab that will be spawned.</param>
        /// <param name="spawnHandler">A method to use as a custom spawnhandler on clients.</param>
        /// <param name="unspawnHandler">A method to use as a custom un-spawnhandler on clients.</param>
        public void RegisterPrefab(NetworkIdentity identity, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            var prefabHash = identity.PrefabHash;

            if (prefabHash == 0)
            {
                throw new InvalidOperationException("RegisterPrefab game object " + identity.name + " has no " + nameof(identity) + ". Use RegisterSpawnHandler() instead?");
            }

            if (logger.LogEnabled()) logger.Log("Registering custom prefab '" + identity.name + "' as asset:" + prefabHash + " " + spawnHandler.Method.Name + "/" + unspawnHandler.Method.Name);

            _spawnHandlers[prefabHash] = spawnHandler;
            _unspawnHandlers[prefabHash] = unspawnHandler;
        }

        /// <summary>
        /// Removes a registered spawn prefab that was setup with ClientScene.RegisterPrefab.
        /// </summary>
        /// <param name="identity">The prefab to be removed from registration.</param>
        public void UnregisterPrefab(NetworkIdentity identity)
        {
            var prefabHash = identity.PrefabHash;

            _spawnHandlers.Remove(prefabHash);
            _unspawnHandlers.Remove(prefabHash);
        }

        #endregion

        #region Spawn Handler

        /// <summary>
        /// This is an advanced spawning function that registers a custom prefabHash with the UNET spawning system.
        /// <para>This can be used to register custom spawning methods for an prefabHash - instead of the usual method of registering spawning methods for a prefab. This should be used when no prefab exists for the spawned objects - such as when they are constructed dynamically at runtime from configuration data.</para>
        /// </summary>
        /// <param name="prefabHash"></param>
        /// <param name="spawnHandler">A method to use as a custom spawnhandler on clients.</param>
        /// <param name="unspawnHandler">A method to use as a custom un-spawnhandler on clients.</param>
        public void RegisterSpawnHandler(int prefabHash, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            if (logger.LogEnabled())
            {
                var spawnName = spawnHandler?.Method.Name ?? "<NULL>";
                var unspawnName = unspawnHandler?.Method.Name ?? "<NULL>";
                logger.Log($"RegisterSpawnHandler PrefabHash:'{prefabHash}' Spawn:{spawnName} UnSpawn:{unspawnName}");
            }

            _spawnHandlers[prefabHash] = spawnHandler;
            _unspawnHandlers[prefabHash] = unspawnHandler;
        }

        /// <summary>
        /// Removes a registered spawn handler function that was registered with ClientScene.RegisterHandler().
        /// </summary>
        /// <param name="prefabHash">The prefabHash for the handler to be removed for.</param>
        public void UnregisterSpawnHandler(int prefabHash)
        {
            _spawnHandlers.Remove(prefabHash);
            _unspawnHandlers.Remove(prefabHash);
        }

        /// <summary>
        /// This clears the registered spawn prefabs and spawn handler functions for this client.
        /// </summary>
        public void ClearSpawners()
        {
            _prefabs.Clear();
            _spawnHandlers.Clear();
            _unspawnHandlers.Clear();
        }

        #endregion

        private void UnSpawn(NetworkIdentity identity)
        {
            logger.Assert(!Client.IsLocalClient, "UnSpawn should not be called in host mode");
            // it is useful to remove authority when destroying the object
            // this can be useful to clean up stuff after a local player is destroyed
            // call before StopClient, but dont reset the HasAuthority bool, people might want to use HasAuthority from stopclient or destroy
            if (identity.HasAuthority)
                identity.CallStopAuthority();

            identity.StopClient();

            if (_unspawnHandlers.TryGetValue(identity.PrefabHash, out var handler) && handler != null)
            {
                handler(identity);
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

            Client.World.RemoveIdentity(identity);
        }

        /// <summary>
        /// Destroys all networked objects on the client.
        /// <para>This can be used to clean up when a network connection is closed.</para>
        /// </summary>
        public void DestroyAllClientObjects()
        {
            // dont destroy objects if we are server
            if (Client.IsLocalClient)
            {
                if (logger.LogEnabled()) logger.Log("Skipping DestroyAllClientObjects because we are host client");
                return;
            }

            // create copy so they can be removed inside loop
            // allocation here are fine because is part of clean up
            var all = Client.World.SpawnedIdentities.ToArray();

            foreach (var identity in all)
            {
                if (identity != null && identity.gameObject != null)
                {
                    UnSpawn(identity);
                }
            }
            Client.World.ClearSpawnedObjects();
        }

        private void ApplySpawnPayload(NetworkIdentity identity, SpawnMessage msg)
        {
            if (msg.prefabHash.HasValue)
                identity.PrefabHash = msg.prefabHash.Value;

            if (!identity.gameObject.activeSelf)
            {
                identity.gameObject.SetActive(true);
            }

            identity.SetClientValues(this, msg);

            if (msg.isLocalPlayer)
                InternalAddCharacter(identity);

            // deserialize components if any payload
            // (Count is 0 if there were no components)
            if (msg.payload.Count > 0)
            {
                using (var payloadReader = NetworkReaderPool.GetReader(msg.payload, Client.World))
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
            if (msg.prefabHash == null && msg.sceneId == null)
                throw new SpawnObjectException($"Empty prefabHash and sceneId for netId: {msg.netId}");

            if (logger.LogEnabled()) logger.Log($"Client spawn handler instantiating netId={msg.netId} prefabHash={msg.prefabHash:X} sceneId={msg.sceneId:X} pos={msg.position}");

            // was the object already spawned?
            var existing = Client.World.TryGetIdentity(msg.netId, out var identity);

            if (!existing)
            {
                //is the object on the prefab or scene object lists?
                identity = msg.sceneId.HasValue
                    ? SpawnSceneObject(msg)
                    : SpawnPrefab(msg);
            }

            // should never happen, Spawn methods above should throw instead
            Debug.Assert(identity != null);

            ApplySpawnPayload(identity, msg);

            // add after applying payload, but only if it is new object
            if (!existing)
                Client.World.AddIdentity(msg.netId, identity);
        }

        private NetworkIdentity SpawnPrefab(SpawnMessage msg)
        {
            // try spawn handler first, then prefab after
            if (_spawnHandlers.TryGetValue(msg.prefabHash.Value, out var handler) && handler != null)
            {
                if (logger.LogEnabled()) logger.Log($"Client spawn with custom handler: [netId:{msg.netId} prefabHash:{msg.prefabHash:X} pos:{msg.position} rotation: {msg.rotation}]");

                var obj = handler.Invoke(msg);
                if (obj == null)
                    throw new SpawnObjectException($"Spawn handler for prefabHash={msg.prefabHash:X} returned null");
                return obj;
            }

            var prefab = GetPrefab(msg.prefabHash.Value);

            if (logger.LogEnabled()) logger.Log($"Client spawn from prefab: [netId:{msg.netId} prefabHash:{msg.prefabHash:X} pos:{msg.position} rotation: {msg.rotation}]");

            // we need to set position and rotation here incase that their values are used from awake/onenable
            var pos = msg.position ?? prefab.transform.position;
            var rot = msg.rotation ?? prefab.transform.rotation;
            return Instantiate(prefab, pos, rot);
        }

        internal NetworkIdentity SpawnSceneObject(SpawnMessage msg)
        {
            var spawned = SpawnSceneObject(msg.sceneId.Value);
            if (spawned != null)
            {
                if (logger.LogEnabled()) logger.Log($"Client spawn from scene object [netId:{msg.netId}] [sceneId:{msg.sceneId:X}] obj:{spawned}");
                return spawned;
            }

            // failed to spawn
            var errorMsg = $"Failed to spawn scene object sceneId={msg.sceneId:X}";
            // dump the whole spawnable objects dict for easier debugging
            if (logger.LogEnabled())
            {
                var builder = new StringBuilder();
                builder.AppendLine($"{errorMsg} SpawnableObjects.Count={spawnableObjects.Count}");

                foreach (var kvp in spawnableObjects)
                    builder.AppendLine($"Spawnable: SceneId={kvp.Key} name={kvp.Value.name}");

                logger.Log(builder.ToString());
            }

            throw new SpawnObjectException(errorMsg);
        }

        private NetworkIdentity SpawnSceneObject(ulong sceneId)
        {
            if (spawnableObjects.TryGetValue(sceneId, out var identity))
            {
                spawnableObjects.Remove(sceneId);
                return identity;
            }
            logger.LogWarning($"Could not find scene object with sceneId:{sceneId:X}");
            return null;
        }

        internal void OnRemoveAuthority(RemoveAuthorityMessage msg)
        {
            if (logger.LogEnabled()) logger.Log($"Client remove auth handler");

            // was the object already spawned?
            var existing = Client.World.TryGetIdentity(msg.netId, out var identity);

            if (!existing)
            {
                logger.LogWarning($"Could not find object with id {msg.netId}");
                return;
            }

            identity.HasAuthority = false;

            identity.NotifyAuthority();
        }

        internal void OnRemoveCharacter(RemoveCharacterMessage msg)
        {
            if (logger.LogEnabled()) logger.Log($"Client remove character handler");

            var player = Client.Player;
            var identity = player.Identity;

            if (identity == null)
            {
                logger.LogWarning($"Could not find player's character");
                return;
            }

            player.Identity = null;
            identity.HasAuthority = msg.keepAuthority;

            identity.NotifyAuthority();
        }

        internal void OnObjectHide(ObjectHideMessage msg)
        {
            DestroyObject(msg.netId);
        }

        internal void OnObjectDestroy(ObjectDestroyMessage msg)
        {
            DestroyObject(msg.netId);
        }

        private void DestroyObject(uint netId)
        {
            if (logger.LogEnabled()) logger.Log("ClientScene.OnObjDestroy netId:" + netId);

            if (Client.World.TryGetIdentity(netId, out var localObject))
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
            if (Client.World.TryGetIdentity(msg.netId, out var localObject))
            {
                if (msg.isLocalPlayer)
                    InternalAddCharacter(localObject);

                localObject.SetClientValues(this, msg);

                localObject.NotifyAuthority();
                localObject.StartClient();
                CheckForLocalPlayer(localObject);
            }
        }

        internal void OnRpcMessage(RpcMessage msg)
        {
            if (logger.LogEnabled()) logger.Log($"ClientScene.OnRPCMessage index:{msg.functionIndex} netId:{msg.netId}");

            if (!Client.World.TryGetIdentity(msg.netId, out var identity))
            {
                if (logger.WarnEnabled()) logger.LogWarning($"Spawned object not found when handling ClientRpc message [netId={msg.netId}]");
                return;
            }

            var behaviour = identity.NetworkBehaviours[msg.componentIndex];

            var remoteCall = behaviour.RemoteCallCollection.Get(msg.functionIndex);

            if (remoteCall.InvokeType != RpcInvokeType.ClientRpc)
            {
                throw new MethodInvocationException($"Invalid RPC call with index {msg.functionIndex}");
            }

            using (var reader = NetworkReaderPool.GetReader(msg.payload, Client.World))
            {
                remoteCall.Invoke(reader, behaviour, null, 0);
            }
        }

        private void CheckForLocalPlayer(NetworkIdentity identity)
        {
            if (identity && identity == Client.Player?.Identity)
            {
                // Set isLocalPlayer to true on this NetworkIdentity and trigger OnStartLocalPlayer in all scripts on the same GO
                identity.StartLocalPlayer();

                if (logger.LogEnabled()) logger.Log("ClientScene.OnOwnerMessage - player=" + identity.name);
            }
        }

        private void OnServerRpcReply(INetworkPlayer player, ServerRpcReply reply)
        {
            // find the callback that was waiting for this and invoke it.
            if (_callbacks.TryGetValue(reply.replyId, out var action))
            {
                _callbacks.Remove(_replyId);
                using (var reader = NetworkReaderPool.GetReader(reply.payload, Client.World))
                {
                    action.Invoke(reader);
                }
            }
            else
            {
                throw new MethodAccessException("Received reply but no handler was registered");
            }
        }

        private readonly Dictionary<int, Action<NetworkReader>> _callbacks = new Dictionary<int, Action<NetworkReader>>();
        private int _replyId;

        /// <summary>
        /// Creates a task that waits for a reply from the server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>the task that will be completed when the result is in, and the id to use in the request</returns>
        internal (UniTask<T> task, int replyId) CreateReplyTask<T>()
        {
            var newReplyId = _replyId++;
            var completionSource = AutoResetUniTaskCompletionSource<T>.Create();
            void Callback(NetworkReader reader)
            {
                var result = reader.Read<T>();
                completionSource.TrySetResult(result);
            }

            _callbacks.Add(newReplyId, Callback);
            return (completionSource.Task, newReplyId);
        }
    }
}
