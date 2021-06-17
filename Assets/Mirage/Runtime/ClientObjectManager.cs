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
        internal readonly Dictionary<Guid, SpawnHandlerDelegate> spawnHandlers = new Dictionary<Guid, SpawnHandlerDelegate>();
        internal readonly Dictionary<Guid, UnSpawnDelegate> unspawnHandlers = new Dictionary<Guid, UnSpawnDelegate>();

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
        internal readonly Dictionary<Guid, NetworkIdentity> prefabs = new Dictionary<Guid, NetworkIdentity>();

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
                    NetworkSceneManager.ClientSceneChanged.AddListener(OnClientSceneChanged);
            }
        }

        void OnClientConnected(INetworkPlayer player)
        {
            syncVarReceiver = new SyncVarReceiver(Client, Client.World);
            RegisterSpawnPrefabs();

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

        void OnClientSceneChanged(string scenePath, SceneOperation sceneOperation)
        {
            PrepareToSpawnSceneObjects();
        }

        internal void RegisterHostHandlers()
        {
            Client.Player.RegisterHandler<ObjectDestroyMessage>(msg => { });
            Client.Player.RegisterHandler<ObjectHideMessage>(msg => { });
            Client.Player.RegisterHandler<SpawnMessage>(OnHostClientSpawn);
            Client.Player.RegisterHandler<ServerRpcReply>(msg => { });
            Client.Player.RegisterHandler<RpcMessage>(msg => { });
        }

        internal void RegisterMessageHandlers()
        {
            Client.Player.RegisterHandler<ObjectDestroyMessage>(OnObjectDestroy);
            Client.Player.RegisterHandler<ObjectHideMessage>(OnObjectHide);
            Client.Player.RegisterHandler<SpawnMessage>(OnSpawn);
            Client.Player.RegisterHandler<ServerRpcReply>(OnServerRpcReply);
            Client.Player.RegisterHandler<RpcMessage>(OnRpcMessage);
        }

        bool ConsiderForSpawning(NetworkIdentity identity)
        {
            // not spawned yet, not hidden, etc.?
            return identity.NetId == 0 &&
                   identity.gameObject.hideFlags != HideFlags.NotEditable &&
                   identity.gameObject.hideFlags != HideFlags.HideAndDontSave &&
                   identity.sceneId != 0;
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
                spawnableObjects.Add(obj.sceneId, obj);
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
        /// <param name="assetId">asset id of the prefab</param>
        /// <returns>true if prefab was registered</returns>
        public NetworkIdentity GetPrefab(Guid assetId)
        {
            if (assetId == Guid.Empty)
                return null;

            if (prefabs.TryGetValue(assetId, out NetworkIdentity identity))
            {
                return identity;
            }
            return null;
        }

        /// <summary>
        /// Registers a prefab with the spawning system.
        /// <para>When a NetworkIdentity object is spawned on a server with NetworkServer.SpawnObject(), and the prefab that the object was created from was registered with RegisterPrefab(), the client will use that prefab to instantiate a corresponding client object with the same netId.</para>
        /// <para>The ClientObjectManager has a list of spawnable prefabs, it uses this function to register those prefabs with the ClientScene.</para>
        /// <para>The set of current spawnable object is available in the ClientScene static member variable ClientScene.prefabs, which is a dictionary of NetworkAssetIds and prefab references.</para>
        /// </summary>
        /// <param name="identity">A Prefab that will be spawned.</param>
        /// <param name="newAssetId">An assetId to be assigned to this prefab. This allows a dynamically created game object to be registered for an already known asset Id.</param>
        public void RegisterPrefab(NetworkIdentity identity, Guid newAssetId)
        {
            identity.AssetId = newAssetId;

            if (logger.LogEnabled()) logger.Log("Registering prefab '" + identity.name + "' as asset:" + identity.AssetId);
            prefabs[identity.AssetId] = identity;
        }

        /// <summary>
        /// Registers a prefab with the spawning system.
        /// <para>When a NetworkIdentity object is spawned on a server with NetworkServer.SpawnObject(), and the prefab that the object was created from was registered with RegisterPrefab(), the client will use that prefab to instantiate a corresponding client object with the same netId.</para>
        /// <para>The ClientObjectManager has a list of spawnable prefabs, it uses this function to register those prefabs with the ClientScene.</para>
        /// <para>The set of current spawnable object is available in the ClientScene static member variable ClientScene.prefabs, which is a dictionary of NetworkAssetIds and prefab references.</para>
        /// </summary>
        /// <param name="identity">A Prefab that will be spawned.</param>
        public void RegisterPrefab(NetworkIdentity identity)
        {
            if (logger.LogEnabled()) logger.Log("Registering prefab '" + identity.name + "' as asset:" + identity.AssetId);
            prefabs[identity.AssetId] = identity;
        }

        /// <summary>
        /// Registers a prefab with the spawning system.
        /// <para>When a NetworkIdentity object is spawned on a server with NetworkServer.SpawnObject(), and the prefab that the object was created from was registered with RegisterPrefab(), the client will use that prefab to instantiate a corresponding client object with the same netId.</para>
        /// <para>The ClientObjectManager has a list of spawnable prefabs, it uses this function to register those prefabs with the ClientScene.</para>
        /// <para>The set of current spawnable object is available in the ClientScene static member variable ClientScene.prefabs, which is a dictionary of NetworkAssetIds and prefab references.</para>
        /// </summary>
        /// <param name="identity">A Prefab that will be spawned.</param>
        /// <param name="spawnHandler">A method to use as a custom spawnhandler on clients.</param>
        /// <param name="unspawnHandler">A method to use as a custom un-spawnhandler on clients.</param>
        public void RegisterPrefab(NetworkIdentity identity, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            if (identity.AssetId == Guid.Empty)
            {
                throw new InvalidOperationException("RegisterPrefab game object " + identity.name + " has no " + nameof(identity) + ". Use RegisterSpawnHandler() instead?");
            }

            if (logger.LogEnabled()) logger.Log("Registering custom prefab '" + identity.name + "' as asset:" + identity.AssetId + " " + spawnHandler.Method.Name + "/" + unspawnHandler.Method.Name);

            spawnHandlers[identity.AssetId] = spawnHandler;
            unspawnHandlers[identity.AssetId] = unspawnHandler;
        }

        /// <summary>
        /// Removes a registered spawn prefab that was setup with ClientScene.RegisterPrefab.
        /// </summary>
        /// <param name="identity">The prefab to be removed from registration.</param>
        public void UnregisterPrefab(NetworkIdentity identity)
        {
            spawnHandlers.Remove(identity.AssetId);
            unspawnHandlers.Remove(identity.AssetId);
        }

        #endregion

        #region Spawn Handler

        /// <summary>
        /// This is an advanced spawning function that registers a custom assetId with the UNET spawning system.
        /// <para>This can be used to register custom spawning methods for an assetId - instead of the usual method of registering spawning methods for a prefab. This should be used when no prefab exists for the spawned objects - such as when they are constructed dynamically at runtime from configuration data.</para>
        /// </summary>
        /// <param name="assetId">Custom assetId string.</param>
        /// <param name="spawnHandler">A method to use as a custom spawnhandler on clients.</param>
        /// <param name="unspawnHandler">A method to use as a custom un-spawnhandler on clients.</param>
        public void RegisterSpawnHandler(Guid assetId, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
            if (logger.LogEnabled()) logger.Log("RegisterSpawnHandler asset '" + assetId + "' " + spawnHandler.Method.Name + "/" + unspawnHandler.Method.Name);

            spawnHandlers[assetId] = spawnHandler;
            unspawnHandlers[assetId] = unspawnHandler;
        }

        /// <summary>
        /// Removes a registered spawn handler function that was registered with ClientScene.RegisterHandler().
        /// </summary>
        /// <param name="assetId">The assetId for the handler to be removed for.</param>
        public void UnregisterSpawnHandler(Guid assetId)
        {
            spawnHandlers.Remove(assetId);
            unspawnHandlers.Remove(assetId);
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
            Guid assetId = identity.AssetId;

            identity.StopClient();
            if (unspawnHandlers.TryGetValue(assetId, out UnSpawnDelegate handler) && handler != null)
            {
                handler(identity);
            }
            else if (identity.sceneId == 0)
            {
                Destroy(identity.gameObject);
            }
            else
            {
                identity.Reset();
                identity.gameObject.SetActive(false);
                spawnableObjects[identity.sceneId] = identity;
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
            if (msg.assetId != Guid.Empty)
                identity.AssetId = msg.assetId;

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
            if (msg.assetId == Guid.Empty && msg.sceneId == 0)
            {
                throw new InvalidOperationException("OnObjSpawn netId: " + msg.netId + " has invalid asset Id");
            }
            if (logger.LogEnabled()) logger.Log($"Client spawn handler instantiating netId={msg.netId} assetID={msg.assetId} sceneId={msg.sceneId} pos={msg.position}");

            // was the object already spawned?
            bool existing = Client.World.TryGetIdentity(msg.netId, out NetworkIdentity identity);

            if (!existing)
            {
                //is the object on the prefab or scene object lists?
                identity = msg.sceneId == 0
                    ? SpawnPrefab(msg)
                    : SpawnSceneObject(msg);
            }

            if (identity == null)
            {
                //object could not be found.
                throw new InvalidOperationException($"Could not spawn assetId={msg.assetId} scene={msg.sceneId} netId={msg.netId}");
            }

            ApplySpawnPayload(identity, msg);

            // add after applying payload, but only if it is new object
            if (!existing)
                Client.World.AddIdentity(msg.netId, identity);
        }

        NetworkIdentity SpawnPrefab(SpawnMessage msg)
        {
            if (spawnHandlers.TryGetValue(msg.assetId, out SpawnHandlerDelegate handler))
            {
                NetworkIdentity obj = handler(msg);
                if (obj == null)
                {
                    logger.LogWarning("Client spawn handler for " + msg.assetId + " returned null");
                    return null;
                }
                return obj;
            }
            NetworkIdentity prefab = GetPrefab(msg.assetId);
            if (!(prefab is null))
            {
                NetworkIdentity obj = Instantiate(prefab, msg.position, msg.rotation);
                if (logger.LogEnabled())
                {
                    logger.Log("Client spawn handler instantiating [netId:" + msg.netId + " asset ID:" + msg.assetId + " pos:" + msg.position + " rotation: " + msg.rotation + "]");
                }

                return obj;
            }
            logger.LogError("Failed to spawn server object, did you forget to add it to the ClientObjectManager? assetId=" + msg.assetId + " netId=" + msg.netId);
            return null;
        }

        internal NetworkIdentity SpawnSceneObject(SpawnMessage msg)
        {
            NetworkIdentity spawnedId = SpawnSceneObject(msg.sceneId);
            if (spawnedId == null)
            {
                logger.LogError("Spawn scene object not found for " + msg.sceneId.ToString("X") + " SpawnableObjects.Count=" + spawnableObjects.Count);

                // dump the whole spawnable objects dict for easier debugging
                if (logger.LogEnabled())
                {
                    foreach (KeyValuePair<ulong, NetworkIdentity> kvp in spawnableObjects)
                        logger.Log("Spawnable: SceneId=" + kvp.Key + " name=" + kvp.Value.name);
                }
            }

            if (logger.LogEnabled()) logger.Log("Client spawn for [netId:" + msg.netId + "] [sceneId:" + msg.sceneId + "] obj:" + spawnedId);
            return spawnedId;
        }

        NetworkIdentity SpawnSceneObject(ulong sceneId)
        {
            if (spawnableObjects.TryGetValue(sceneId, out NetworkIdentity identity))
            {
                spawnableObjects.Remove(sceneId);
                return identity;
            }
            logger.LogWarning("Could not find scene object with sceneId:" + sceneId.ToString("X"));
            return null;
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
