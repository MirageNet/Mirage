using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Guid = System.Guid;
using Object = UnityEngine.Object;

namespace Mirror
{
    /// <summary>
    /// This is a client object manager class used by the networking system. It manages spawning and unspawning of objects for the network client.
    /// </summary>
    [RequireComponent(typeof(NetworkClient))]
    [DisallowMultipleComponent]
    public class ClientObjectManager : MonoBehaviour
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkClient));

        public NetworkClient client;

        // spawn handlers
        readonly Dictionary<Guid, SpawnHandlerDelegate> spawnHandlers = new Dictionary<Guid, SpawnHandlerDelegate>();
        readonly Dictionary<Guid, UnSpawnDelegate> unspawnHandlers = new Dictionary<Guid, UnSpawnDelegate>();

        /// <summary>
        /// List of prefabs that will be registered with the spawning system.
        /// <para>For each of these prefabs, ClientManager.RegisterPrefab() will be automatically invoke.</para>
        /// </summary>
        public List<GameObject> spawnPrefabs = new List<GameObject>();

        readonly Dictionary<uint, NetworkIdentity> spawned = new Dictionary<uint, NetworkIdentity>();

        bool isSpawnFinished;

        /// <summary>
        /// This is a dictionary of the prefabs that are registered on the client with ClientScene.RegisterPrefab().
        /// <para>The key to the dictionary is the prefab asset Id.</para>
        /// </summary>
        private readonly Dictionary<Guid, GameObject> prefabs = new Dictionary<Guid, GameObject>();

        /// <summary>
        /// This is dictionary of the disabled NetworkIdentity objects in the scene that could be spawned by messages from the server.
        /// <para>The key to the dictionary is the NetworkIdentity sceneId.</para>
        /// </summary>
        public readonly Dictionary<ulong, NetworkIdentity> spawnableObjects = new Dictionary<ulong, NetworkIdentity>();

        /// <summary>
        /// List of all objects spawned in this client
        /// </summary>
        public Dictionary<uint, NetworkIdentity> Spawned
        {
            get
            {
                // if we are in host mode,  the list of spawned object is the same as the server list
                if (client.hostServer != null)
                    return client.hostServer.spawned;
                else
                    return spawned;
            }
        }

        private void Start()
        {
            if (logger.LogEnabled()) logger.Log("ClientObjectManager started");

            client.Connected.AddListener(Connected);
        }

        void Connected(INetworkConnection conn)
        {
            if (client.IsLocalClient)
            {
                RegisterHostHandlers(conn);
            }
            else
            {
                RegisterMessageHandlers(conn);
            }
        }

        internal void RegisterHostHandlers(INetworkConnection connection)
        {
            connection.RegisterHandler<ObjectDestroyMessage>(OnHostClientObjectDestroy);
            connection.RegisterHandler<ObjectHideMessage>(OnHostClientObjectHide);
            connection.RegisterHandler<NetworkPongMessage>(msg => { });
            connection.RegisterHandler<SpawnMessage>(OnHostClientSpawn);
            // host mode reuses objects in the server
            // so we don't need to spawn them
            connection.RegisterHandler<ObjectSpawnStartedMessage>(msg => { });
            connection.RegisterHandler<ObjectSpawnFinishedMessage>(msg => { });
            connection.RegisterHandler<UpdateVarsMessage>(msg => { });
            connection.RegisterHandler<RpcMessage>(OnRpcMessage);
            connection.RegisterHandler<SyncEventMessage>(OnSyncEventMessage);
        }

        internal void RegisterMessageHandlers(INetworkConnection connection)
        {
            connection.RegisterHandler<ObjectDestroyMessage>(OnObjectDestroy);
            connection.RegisterHandler<ObjectHideMessage>(OnObjectHide);
            connection.RegisterHandler<SpawnMessage>(OnSpawn);
            connection.RegisterHandler<ObjectSpawnStartedMessage>(OnObjectSpawnStarted);
            connection.RegisterHandler<ObjectSpawnFinishedMessage>(OnObjectSpawnFinished);
            connection.RegisterHandler<UpdateVarsMessage>(OnUpdateVarsMessage);
            connection.RegisterHandler<RpcMessage>(OnRpcMessage);
            connection.RegisterHandler<SyncEventMessage>(OnSyncEventMessage);
        }

        #region Client Handlers
        internal void OnObjectDestroy(ObjectDestroyMessage msg)
        {
            DestroyObject(msg.netId);
        }

        internal void OnObjectHide(ObjectHideMessage msg)
        {
            DestroyObject(msg.netId);
        }

        internal void OnSpawn(SpawnMessage msg)
        {
            if (msg.assetId == Guid.Empty && msg.sceneId == 0)
            {
                throw new InvalidOperationException("OnObjSpawn netId: " + msg.netId + " has invalid asset Id");
            }
            if (logger.LogEnabled()) logger.Log($"Client spawn handler instantiating netId={msg.netId} assetID={msg.assetId} sceneId={msg.sceneId} pos={msg.position}");

            // was the object already spawned?
            NetworkIdentity identity = GetExistingObject(msg.netId);

            if (identity == null)
            {
                identity = msg.sceneId == 0 ? SpawnPrefab(msg) : SpawnSceneObject(msg);
            }

            if (identity == null)
            {
                throw new InvalidOperationException($"Could not spawn assetId={msg.assetId} scene={msg.sceneId} netId={msg.netId}");
            }

            ApplySpawnPayload(identity, msg);
        }

        internal void OnObjectSpawnStarted(ObjectSpawnStartedMessage _)
        {
            logger.Log("SpawnStarted");

            PrepareToSpawnSceneObjects();
            isSpawnFinished = false;
        }

        internal void OnObjectSpawnFinished(ObjectSpawnFinishedMessage _)
        {
            logger.Log("SpawnFinished");

            // paul: Initialize the objects in the same order as they were initialized
            // in the server.   This is important if spawned objects
            // use data from scene objects
            foreach (NetworkIdentity identity in Spawned.Values.OrderBy(uv => uv.NetId))
            {
                identity.NotifyAuthority();
                identity.StartClient();
                CheckForLocalPlayer(identity);
            }
            isSpawnFinished = true;
        }

        internal void OnUpdateVarsMessage(UpdateVarsMessage msg)
        {
            if (logger.LogEnabled()) logger.Log("ClientScene.OnUpdateVarsMessage " + msg.netId);

            if (Spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
            {
                using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(msg.payload))
                    localObject.OnDeserializeAllSafely(networkReader, false);
            }
            else
            {
                logger.LogWarning("Did not find target for sync message for " + msg.netId + " . Note: this can be completely normal because UDP messages may arrive out of order, so this message might have arrived after a Destroy message.");
            }
        }

        internal void OnRpcMessage(RpcMessage msg)
        {
            if (logger.LogEnabled()) logger.Log("ClientScene.OnRPCMessage hash:" + msg.functionHash + " netId:" + msg.netId);

            if (Spawned.TryGetValue(msg.netId, out NetworkIdentity identity))
            {
                using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(msg.payload))
                    identity.HandleRpc(msg.componentIndex, msg.functionHash, networkReader);
            }
        }

        internal void OnSyncEventMessage(SyncEventMessage msg)
        {
            if (logger.LogEnabled()) logger.Log("ClientScene.OnSyncEventMessage " + msg.netId);

            if (Spawned.TryGetValue(msg.netId, out NetworkIdentity identity))
            {
                using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(msg.payload))
                    identity.HandleSyncEvent(msg.componentIndex, msg.functionHash, networkReader);
            }
            else
            {
                logger.LogWarning("Did not find target for SyncEvent message for " + msg.netId);
            }
        }

        #endregion

        #region Host

        internal void OnHostClientObjectDestroy(ObjectDestroyMessage msg)
        {
            if (logger.LogEnabled()) logger.Log("ClientScene.OnLocalObjectObjDestroy netId:" + msg.netId);

            Spawned.Remove(msg.netId);
        }

        internal void OnHostClientObjectHide(ObjectHideMessage msg)
        {
            if (logger.LogEnabled()) logger.Log("ClientScene::OnLocalObjectObjHide netId:" + msg.netId);

            if (Spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
            {
                localObject.OnSetHostVisibility(false);
            }
        }

        internal void OnHostClientSpawn(SpawnMessage msg)
        {
            if (Spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
            {
                if (msg.isLocalPlayer)
                    InternalAddPlayer(localObject);

                localObject.HasAuthority = msg.isOwner;
                localObject.NotifyAuthority();
                localObject.StartClient();
                localObject.OnSetHostVisibility(true);
                CheckForLocalPlayer(localObject);
            }
        }

        #endregion

        #region Helpers

        NetworkIdentity GetExistingObject(uint netid)
        {
            Spawned.TryGetValue(netid, out NetworkIdentity localObject);
            return localObject;
        }

        NetworkIdentity SpawnPrefab(SpawnMessage msg)
        {
            if (GetPrefab(msg.assetId, out GameObject prefab))
            {
                GameObject obj = Object.Instantiate(prefab, msg.position, msg.rotation);
                if (logger.LogEnabled())
                {
                    logger.Log("Client spawn handler instantiating [netId:" + msg.netId + " asset ID:" + msg.assetId + " pos:" + msg.position + " rotation: " + msg.rotation + "]");
                }

                return obj.GetComponent<NetworkIdentity>();
            }
            if (spawnHandlers.TryGetValue(msg.assetId, out SpawnHandlerDelegate handler))
            {
                GameObject obj = handler(msg);
                if (obj == null)
                {
                    logger.LogWarning("Client spawn handler for " + msg.assetId + " returned null");
                    return null;
                }
                return obj.GetComponent<NetworkIdentity>();
            }
            logger.LogError("Failed to spawn server object, did you forget to add it to the NetworkManager? assetId=" + msg.assetId + " netId=" + msg.netId);
            return null;
        }

        NetworkIdentity SpawnSceneObject(SpawnMessage msg)
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
            logger.LogWarning("Could not find scene object with sceneid:" + sceneId.ToString("X"));
            return null;
        }

        void ApplySpawnPayload(NetworkIdentity identity, SpawnMessage msg)
        {
            identity.Reset();

            if (msg.assetId != Guid.Empty)
                identity.AssetId = msg.assetId;

            if (!identity.gameObject.activeSelf)
            {
                identity.gameObject.SetActive(true);
            }

            // apply local values for VR support
            identity.transform.localPosition = msg.position;
            identity.transform.localRotation = msg.rotation;
            identity.transform.localScale = msg.scale;
            identity.HasAuthority = msg.isOwner;
            identity.NetId = msg.netId;
            identity.Server = client.hostServer;
            identity.Client = client;

            if (msg.isLocalPlayer)
                InternalAddPlayer(identity);

            // deserialize components if any payload
            // (Count is 0 if there were no components)
            if (msg.payload.Count > 0)
            {
                using (PooledNetworkReader payloadReader = NetworkReaderPool.GetReader(msg.payload))
                {
                    identity.OnDeserializeAllSafely(payloadReader, true);
                }
            }

            Spawned[msg.netId] = identity;

            // objects spawned as part of initial state are started on a second pass
            if (isSpawnFinished)
            {
                identity.NotifyAuthority();
                identity.StartClient();
                CheckForLocalPlayer(identity);
            }
        }

        /// <summary>
        /// Find the registered prefab for this asset id.
        /// Useful for debuggers
        /// </summary>
        /// <param name="assetId">asset id of the prefab</param>
        /// <param name="prefab">the prefab gameobject</param>
        /// <returns>true if prefab was registered</returns>
        public bool GetPrefab(Guid assetId, out GameObject prefab)
        {
            prefab = null;
            return assetId != Guid.Empty &&
                   prefabs.TryGetValue(assetId, out prefab) && prefab != null;
        }

        // this is called from message handler for Owner message
        internal void InternalAddPlayer(NetworkIdentity identity)
        {
            if (client.Connection != null)
            {
                client.Connection.Identity = identity;
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

        static bool ConsiderForSpawning(NetworkIdentity identity)
        {
            // not spawned yet, not hidden, etc.?
            return !identity.gameObject.activeSelf &&
                   identity.gameObject.hideFlags != HideFlags.NotEditable &&
                   identity.gameObject.hideFlags != HideFlags.HideAndDontSave &&
                   identity.sceneId != 0;
        }

        void CheckForLocalPlayer(NetworkIdentity identity)
        {
            if (identity == client.LocalPlayer)
            {
                // Set isLocalPlayer to true on this NetworkIdentity and trigger OnStartLocalPlayer in all scripts on the same GO
                identity.ConnectionToServer = client.Connection;
                identity.StartLocalPlayer();

                if (logger.LogEnabled()) logger.Log("ClientScene.OnOwnerMessage - player=" + identity.name);
            }
        }

        void DestroyObject(uint netId)
        {
            if (logger.LogEnabled()) logger.Log("ClientScene.OnObjDestroy netId:" + netId);

            if (Spawned.TryGetValue(netId, out NetworkIdentity localObject) && localObject != null)
            {
                UnSpawn(localObject);
                Spawned.Remove(netId);
            }
            else
            {
                logger.LogWarning("Did not find target for destroy message for " + netId);
            }
        }

        void UnSpawn(NetworkIdentity identity)
        {
            Guid assetId = identity.AssetId;

            identity.NetworkDestroy();
            if (unspawnHandlers.TryGetValue(assetId, out UnSpawnDelegate handler) && handler != null)
            {
                handler(identity.gameObject);
            }
            else if (identity.sceneId == 0)
            {
                Destroy(identity.gameObject);
            }
            else
            {
                identity.MarkForReset();
                identity.gameObject.SetActive(false);
                spawnableObjects[identity.sceneId] = identity;
            }
        }

        #endregion
    }
}
