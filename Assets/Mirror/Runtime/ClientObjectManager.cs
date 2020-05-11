using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Guid = System.Guid;
using Object = UnityEngine.Object;

namespace Mirror
{
    public class ClientObjectManager : MonoBehaviour
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(ClientObjectManager));

        internal NetworkClient client;

        readonly Dictionary<Guid, SpawnHandlerDelegate> spawnHandlers = new Dictionary<Guid, SpawnHandlerDelegate>();
        readonly Dictionary<Guid, UnSpawnDelegate> unspawnHandlers = new Dictionary<Guid, UnSpawnDelegate>();

        bool isSpawnFinished;

        void OnValidate()
        {
#if UNITY_EDITOR
            // automatically assign ClientObjectManager field if we add this to NetworkClient
            client = GetComponent<NetworkClient>();
            if (client != null && client.clientObjectManager == null)
            {
                client.clientObjectManager = this;
                UnityEditor.Undo.RecordObject(gameObject, "Assigned NetworkClient clientObjectManager");
            }
#endif
        }

        internal void RegisterHostHandlers()
        {
            client.Connection.RegisterHandler<ObjectDestroyMessage>(OnHostClientObjectDestroy);
            client.Connection.RegisterHandler<ObjectHideMessage>(OnHostClientObjectHide);
            client.Connection.RegisterHandler<SpawnMessage>(OnHostClientSpawn);
            // host mode reuses objects in the server
            // so we don't need to spawn them
            client.Connection.RegisterHandler<ObjectSpawnStartedMessage>(msg => { });
            client.Connection.RegisterHandler<ObjectSpawnFinishedMessage>(msg => { });
            client.Connection.RegisterHandler<UpdateVarsMessage>(msg => { });
            client.Connection.RegisterHandler<RpcMessage>(OnRpcMessage);
            client.Connection.RegisterHandler<SyncEventMessage>(OnSyncEventMessage);
        }

        internal void RegisterMessageHandlers()
        {
            client.Connection.RegisterHandler<ObjectDestroyMessage>(OnObjectDestroy);
            client.Connection.RegisterHandler<ObjectHideMessage>(OnObjectHide);
            client.Connection.RegisterHandler<SpawnMessage>(OnSpawn);
            client.Connection.RegisterHandler<ObjectSpawnStartedMessage>(OnObjectSpawnStarted);
            client.Connection.RegisterHandler<ObjectSpawnFinishedMessage>(OnObjectSpawnFinished);
            client.Connection.RegisterHandler<UpdateVarsMessage>(OnUpdateVarsMessage);
            client.Connection.RegisterHandler<RpcMessage>(OnRpcMessage);
            client.Connection.RegisterHandler<SyncEventMessage>(OnSyncEventMessage);
        }

        public void Cleanup()
        {
            isSpawnFinished = false;
        }

        #region Host Handlers

        internal void OnHostClientObjectDestroy(ObjectDestroyMessage msg)
        {
            if (logger.LogEnabled()) logger.Log("ClientScene.OnLocalObjectObjDestroy netId:" + msg.netId);

            client.Spawned.Remove(msg.netId);
        }

        internal void OnHostClientObjectHide(ObjectHideMessage msg)
        {
            if (logger.LogEnabled()) logger.Log("ClientScene::OnLocalObjectObjHide netId:" + msg.netId);

            if (client.Spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
            {
                localObject.OnSetHostVisibility(false);
            }
        }

        internal void OnHostClientSpawn(SpawnMessage msg)
        {
            if (client.Spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
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

            client.PrepareToSpawnSceneObjects();
            isSpawnFinished = false;
        }

        internal void OnObjectSpawnFinished(ObjectSpawnFinishedMessage _)
        {
            logger.Log("SpawnFinished");

            // paul: Initialize the objects in the same order as they were initialized
            // in the server.   This is important if spawned objects
            // use data from scene objects
            foreach (NetworkIdentity identity in client.Spawned.Values.OrderBy(uv => uv.NetId))
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

            if (client.Spawned.TryGetValue(msg.netId, out NetworkIdentity localObject) && localObject != null)
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

            if (client.Spawned.TryGetValue(msg.netId, out NetworkIdentity identity))
            {
                using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(msg.payload))
                    identity.HandleRpc(msg.componentIndex, msg.functionHash, networkReader);
            }
        }

        internal void OnSyncEventMessage(SyncEventMessage msg)
        {
            if (logger.LogEnabled()) logger.Log("ClientScene.OnSyncEventMessage " + msg.netId);

            if (client.Spawned.TryGetValue(msg.netId, out NetworkIdentity identity))
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

        #region Helpers

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

        NetworkIdentity GetExistingObject(uint netid)
        {
            client.Spawned.TryGetValue(netid, out NetworkIdentity localObject);
            return localObject;
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

        void ApplySpawnPayload(NetworkIdentity identity, SpawnMessage msg)
        {
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

            client.Spawned[msg.netId] = identity;

            // objects spawned as part of initial state are started on a second pass
            if (isSpawnFinished)
            {
                identity.NotifyAuthority();
                identity.StartClient();
                CheckForLocalPlayer(identity);
            }
        }

        NetworkIdentity SpawnPrefab(SpawnMessage msg)
        {
            if (client.GetPrefab(msg.assetId, out GameObject prefab))
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
                logger.LogError("Spawn scene object not found for " + msg.sceneId.ToString("X") + " SpawnableObjects.Count=" + client.spawnableObjects.Count);

                // dump the whole spawnable objects dict for easier debugging
                if (logger.LogEnabled())
                {
                    foreach (KeyValuePair<ulong, NetworkIdentity> kvp in client.spawnableObjects)
                        logger.Log("Spawnable: SceneId=" + kvp.Key + " name=" + kvp.Value.name);
                }
            }

            if (logger.LogEnabled()) logger.Log("Client spawn for [netId:" + msg.netId + "] [sceneId:" + msg.sceneId + "] obj:" + spawnedId);
            return spawnedId;
        }

        NetworkIdentity SpawnSceneObject(ulong sceneId)
        {
            if (client.spawnableObjects.TryGetValue(sceneId, out NetworkIdentity identity))
            {
                client.spawnableObjects.Remove(sceneId);
                return identity;
            }
            logger.LogWarning("Could not find scene object with sceneid:" + sceneId.ToString("X"));
            return null;
        }

        void DestroyObject(uint netId)
        {
            if (logger.LogEnabled()) logger.Log("ClientScene.OnObjDestroy netId:" + netId);

            if (client.Spawned.TryGetValue(netId, out NetworkIdentity localObject) && localObject != null)
            {
                client.UnSpawn(localObject);
                client.Spawned.Remove(netId);
            }
            else
            {
                logger.LogWarning("Did not find target for destroy message for " + netId);
            }
        }

        #endregion

    }
}
