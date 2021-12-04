using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.Logging;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirage
{

    [AddComponentMenu("Network/ClientObjectManager")]
    [DisallowMultipleComponent]
    public class ClientObjectManager : MonoBehaviour, IClientObjectManager
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(ClientObjectManager));

        [FormerlySerializedAs("client")]
        public NetworkClient Client;
        [FormerlySerializedAs("networkSceneManager")]
        public NetworkSceneManager NetworkSceneManager;

        // spawn handlers. internal for testing purposes. do not use directly.
        internal readonly Dictionary<int, SpawnHandlerDelegate> spawnHandlers = new Dictionary<int, SpawnHandlerDelegate>();
        internal readonly Dictionary<int, UnSpawnDelegate> unspawnHandlers = new Dictionary<int, UnSpawnDelegate>();

        [Header("Prefabs")]
        /// <summary>
        /// List of prefabs that will be registered with the spawning system.
        /// <para>For each of these prefabs, ClientManager.RegisterPrefab() will be automatically invoke.</para>
        /// </summary>
        public List<NetworkIdentity> spawnPrefabs = new List<NetworkIdentity>();

        /// <summary>
        /// This is a dictionary of the prefabs that are registered on the client with ClientScene.RegisterPrefab().
        /// <para>The key to the dictionary is the prefab asset Id.</para>
        /// </summary>
        internal readonly Dictionary<int, NetworkIdentity> prefabs = new Dictionary<int, NetworkIdentity>();

        /// <summary>
        /// This is dictionary of the disabled NetworkIdentity objects in the scene that could be spawned by messages from the server.
        /// <para>The key to the dictionary is the NetworkIdentity sceneId.</para>
        /// </summary>
        public readonly Dictionary<ulong, NetworkIdentity> spawnableObjects = new Dictionary<ulong, NetworkIdentity>();

        internal ServerObjectManager ServerObjectManager;

        SyncVarReceiver syncVarReceiver;

        public void Start()
        {
            if (Client != null)
            {
                Client.Connected.AddListener(OnClientConnected);
                Client.Disconnected.AddListener(OnClientDisconnected);

                if (NetworkSceneManager != null)
                    NetworkSceneManager.OnClientFinishedSceneChange.AddListener(OnFinishedSceneChange);
            }
        }

#if UNITY_EDITOR
        readonly Dictionary<int, NetworkIdentity> validateCache = new Dictionary<int, NetworkIdentity>();
        void OnValidate()
        {
            validateCache.Clear();
            foreach (NetworkIdentity prefab in spawnPrefabs)
            {
                if (prefab == null)
                    continue;

                int hash = prefab.PrefabHash;
                if (validateCache.TryGetValue(hash, out NetworkIdentity existing))
                {
                    if (prefab == existing)
                        continue;

                    string pathA = UnityEditor.AssetDatabase.GetAssetPath(prefab);
                    string pathB = UnityEditor.AssetDatabase.GetAssetPath(existing);
                    logger.Assert(prefab.PrefabHash == existing.PrefabHash);
                    logger.LogError($"2 prefabs had the same hash:'{hash}', A:'{prefab.name}' B:'{existing.name}'. Path A:{pathA} Path B:{pathB}");
                }

                validateCache[hash] = prefab;
            }
        }
#endif

        void OnClientConnected(INetworkPlayer player)
        {
            syncVarReceiver = new SyncVarReceiver(Client, Client.World);
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

        void OnClientDisconnected(ClientStoppedReason reason)
        {
            ClearSpawners();
            DestroyAllClientObjects();
            syncVarReceiver = null;
        }

        void OnFinishedSceneChange(string scenePath, SceneOperation sceneOperation)
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

        bool ConsiderForSpawning(NetworkIdentity identity)
        {
            // not spawned yet, not hidden, etc.?
            return !identity.IsSpawned &&
                   identity.gameObject.hideFlags != HideFlags.NotEditable &&
                   identity.gameObject.hideFlags != HideFlags.HideAndDontSave &&
                   identity.IsSceneObject;
        }

        // this is called from message handler for Owner message
        internal void InternalAddPlayer(NetworkIdentity identity)
        {
            if (Client.Player != null)
            {
                Client.Player.Identity = identity;
            }
            else
            {
                logger.LogWarning("No ready connection found for setting player controller during InternalAddPlayer");
            }
        }

        /// <summary>
        /// Call this after loading/unloading a scene in the client after connection to register the spawnable objects
        /// </summary>
        public void PrepareToSpawnSceneObjects()
        {
            // add all unspawned NetworkIdentities to spawnable objects
            spawnableObjects.Clear();
            IEnumerable<NetworkIdentity> sceneObjects =
                Resources.FindObjectsOfTypeAll<NetworkIdentity>()
                               .Where(ConsiderForSpawning);

            foreach (NetworkIdentity obj in sceneObjects)
            {
                spawnableObjects.Add(obj.SceneId, obj);
            }
        }

        #region Spawn Prefabs
        private void RegisterSpawnPrefabs()
        {
            for (int i = 0; i < spawnPrefabs.Count; i++)
            {
                NetworkIdentity prefab = spawnPrefabs[i];
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
        public NetworkIdentity GetPrefab(int prefabHash)
        {
            if (prefabHash == 0)
                return null;

            if (prefabs.TryGetValue(prefabHash, out NetworkIdentity identity))
            {
                return identity;
            }
            return null;
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
            prefabs[identity.PrefabHash] = identity;
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
            prefabs[identity.PrefabHash] = identity;
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
            int prefabHash = identity.PrefabHash;

            if (prefabHash == 0)
            {
                throw new InvalidOperationException("RegisterPrefab game object " + identity.name + " has no " + nameof(identity) + ". Use RegisterSpawnHandler() instead?");
            }

            if (logger.LogEnabled()) logger.Log("Registering custom prefab '" + identity.name + "' as asset:" + prefabHash + " " + spawnHandler.Method.Name + "/" + unspawnHandler.Method.Name);

            spawnHandlers[prefabHash] = spawnHandler;
            unspawnHandlers[prefabHash] = unspawnHandler;
        }

        /// <summary>
        /// Removes a registered spawn prefab that was setup with ClientScene.RegisterPrefab.
        /// </summary>
        /// <param name="identity">The prefab to be removed from registration.</param>
        public void UnregisterPrefab(NetworkIdentity identity)
        {
            int prefabHash = identity.PrefabHash;

            spawnHandlers.Remove(prefabHash);
            unspawnHandlers.Remove(prefabHash);
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
            if (logger.LogEnabled()) logger.Log("RegisterSpawnHandler asset '" + prefabHash + "' " + spawnHandler.Method.Name + "/" + unspawnHandler.Method.Name);

            spawnHandlers[prefabHash] = spawnHandler;
            unspawnHandlers[prefabHash] = unspawnHandler;
        }

        /// <summary>
        /// Removes a registered spawn handler function that was registered with ClientScene.RegisterHandler().
        /// </summary>
        /// <param name="prefabHash">The prefabHash for the handler to be removed for.</param>
        public void UnregisterSpawnHandler(int prefabHash)
        {
            spawnHandlers.Remove(prefabHash);
            unspawnHandlers.Remove(prefabHash);
        }

        /// <summary>
        /// This clears the registered spawn prefabs and spawn handler functions for this client.
        /// </summary>
        public void ClearSpawners()
        {
            prefabs.Clear();
            spawnHandlers.Clear();
            unspawnHandlers.Clear();
        }

        #endregion

        void UnSpawn(NetworkIdentity identity)
        {
            identity.StopClient();
            if (unspawnHandlers.TryGetValue(identity.PrefabHash, out UnSpawnDelegate handler) && handler != null)
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
            // create copy so they can be removed inside loop
            // allocation here are fine because is part of clean up
            NetworkIdentity[] all = Client.World.SpawnedIdentities.ToArray();

            foreach (NetworkIdentity identity in all)
            {
                if (identity != null && identity.gameObject != null)
                {
                    UnSpawn(identity);
                }
            }
            Client.World.ClearSpawnedObjects();
        }

        void ApplySpawnPayload(NetworkIdentity identity, SpawnMessage msg)
        {
            if (msg.prefabHash.HasValue)
                identity.PrefabHash = msg.prefabHash.Value;

            if (!identity.gameObject.activeSelf)
            {
                identity.gameObject.SetActive(true);
            }

            identity.SetClientValues(this, msg);

            if (msg.isLocalPlayer)
                InternalAddPlayer(identity);

            // deserialize components if any payload
            // (Count is 0 if there were no components)
            if (msg.payload.Count > 0)
            {
                using (PooledNetworkReader payloadReader = NetworkReaderPool.GetReader(msg.payload))
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
            {
                throw new InvalidOperationException($"OnSpawn has empty prefabHash and sceneId for netId: {msg.netId}");
            }
            if (logger.LogEnabled()) logger.Log($"Client spawn handler instantiating netId={msg.netId} prefabHash={msg.prefabHash:X} sceneId={msg.sceneId:X} pos={msg.position}");

            // was the object already spawned?
            bool existing = Client.World.TryGetIdentity(msg.netId, out NetworkIdentity identity);

            if (!existing)
            {
                //is the object on the prefab or scene object lists?
                identity = msg.sceneId.HasValue
                    ? SpawnSceneObject(msg)
                    : SpawnPrefab(msg);
            }

            if (identity == null)
            {
                //object could not be found.
                throw new InvalidOperationException($"Could not spawn prefabHash={msg.prefabHash:X} scene={msg.sceneId:X} netId={msg.netId}");
            }

            ApplySpawnPayload(identity, msg);

            // add after applying payload, but only if it is new object
            if (!existing)
                Client.World.AddIdentity(msg.netId, identity);
        }

        NetworkIdentity SpawnPrefab(SpawnMessage msg)
        {
            if (spawnHandlers.TryGetValue(msg.prefabHash.Value, out SpawnHandlerDelegate handler) && handler != null)
            {
                NetworkIdentity obj = handler(msg);
                if (obj == null)
                {
                    logger.LogWarning($"Client spawn handler for {msg.prefabHash:X} returned null");
                    return null;
                }
                return obj;
            }
            NetworkIdentity prefab = GetPrefab(msg.prefabHash.Value);
            if (!(prefab is null))
            {
                // we need to set position and rotation here incase that their values can be used form awake/onenable
                Vector3 pos = msg.position ?? prefab.transform.position;
                Quaternion rot = msg.rotation ?? prefab.transform.rotation;
                NetworkIdentity obj = Instantiate(prefab, pos, rot);
                if (logger.LogEnabled())
                {
                    logger.Log($"Client spawn handler instantiating [netId:{msg.netId} asset ID:{msg.prefabHash:X} pos:{msg.position} rotation: {msg.rotation}]");
                }

                return obj;
            }
            logger.LogError("Failed to spawn server object, did you forget to add it to the ClientObjectManager? prefabHash=" + msg.prefabHash + " netId=" + msg.netId);
            return null;
        }

        internal NetworkIdentity SpawnSceneObject(SpawnMessage msg)
        {
            NetworkIdentity spawned = SpawnSceneObject(msg.sceneId.Value);
            if (spawned == null)
            {
                logger.LogError($"Spawn scene object not found for {msg.sceneId:X} SpawnableObjects.Count={spawnableObjects.Count}");

                // dump the whole spawnable objects dict for easier debugging
                if (logger.LogEnabled())
                {
                    foreach (KeyValuePair<ulong, NetworkIdentity> kvp in spawnableObjects)
                        logger.Log($"Spawnable: SceneId={kvp.Key} name={kvp.Value.name}");
                }
            }

            if (logger.LogEnabled()) logger.Log($"Client spawn for [netId:{msg.netId}] [sceneId:{msg.sceneId:X}] obj:{spawned}");
            return spawned;
        }

        NetworkIdentity SpawnSceneObject(ulong sceneId)
        {
            if (spawnableObjects.TryGetValue(sceneId, out NetworkIdentity identity))
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
            bool existing = Client.World.TryGetIdentity(msg.netId, out NetworkIdentity identity);

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

            INetworkPlayer player = Client.Player;
            NetworkIdentity identity = player.Identity;

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

        void DestroyObject(uint netId)
        {
            if (logger.LogEnabled()) logger.Log("ClientScene.OnObjDestroy netId:" + netId);

            if (Client.World.TryGetIdentity(netId, out NetworkIdentity localObject))
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
            if (Client.World.TryGetIdentity(msg.netId, out NetworkIdentity localObject))
            {
                if (msg.isLocalPlayer)
                    InternalAddPlayer(localObject);

                localObject.SetClientValues(this, msg);

                localObject.NotifyAuthority();
                localObject.StartClient();
                CheckForLocalPlayer(localObject);
            }
        }

        internal void OnRpcMessage(RpcMessage msg)
        {
            if (logger.LogEnabled()) logger.Log("ClientScene.OnRPCMessage hash:" + msg.functionHash + " netId:" + msg.netId);

            Skeleton skeleton = RemoteCallHelper.GetSkeleton(msg.functionHash);

            if (skeleton.invokeType != RpcInvokeType.ClientRpc)
            {
                throw new MethodInvocationException($"Invalid RPC call with id {msg.functionHash}");
            }
            if (Client.World.TryGetIdentity(msg.netId, out NetworkIdentity identity))
            {
                using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(msg.payload))
                {
                    networkReader.ObjectLocator = Client.World;
                    identity.HandleRemoteCall(skeleton, msg.componentIndex, networkReader);
                }
            }
        }

        void CheckForLocalPlayer(NetworkIdentity identity)
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
            if (callbacks.TryGetValue(reply.replyId, out Action<NetworkReader> action))
            {
                callbacks.Remove(replyId);
                using (PooledNetworkReader reader = NetworkReaderPool.GetReader(reply.payload))
                {
                    reader.ObjectLocator = Client.World;
                    action(reader);
                }
            }
            else
            {
                throw new MethodAccessException("Received reply but no handler was registered");
            }
        }

        private readonly Dictionary<int, Action<NetworkReader>> callbacks = new Dictionary<int, Action<NetworkReader>>();
        private int replyId;

        /// <summary>
        /// Creates a task that waits for a reply from the server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>the task that will be completed when the result is in, and the id to use in the request</returns>
        internal (UniTask<T> task, int replyId) CreateReplyTask<T>()
        {
            int newReplyId = replyId++;
            var completionSource = AutoResetUniTaskCompletionSource<T>.Create();
            void Callback(NetworkReader reader)
            {
                T result = reader.Read<T>();
                completionSource.TrySetResult(result);
            }

            callbacks.Add(newReplyId, Callback);
            return (completionSource.Task, newReplyId);
        }
    }
}
