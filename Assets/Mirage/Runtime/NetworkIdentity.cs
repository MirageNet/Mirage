using System;
using System.Collections.Generic;
using Mirage.Events;
using Mirage.Logging;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirage
{
    /// <summary>
    /// The NetworkIdentity identifies objects across the network, between server and clients.
    /// Its primary data is a NetworkInstanceId which is allocated by the server and then set on clients.
    /// This is used in network communications to be able to lookup game objects on different machines.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     The NetworkIdentity is used to synchronize information in the object with the network.
    ///     Only the server should create instances of objects which have NetworkIdentity as otherwise
    ///     they will not be properly connected to the system.
    /// </para>
    /// <para>
    ///     For complex objects with a hierarchy of subcomponents, the NetworkIdentity must be on the root of the hierarchy.
    ///     It is not supported to have multiple NetworkIdentity components on subcomponents of a hierarchy.
    /// </para>
    /// <para>
    ///     NetworkBehaviour scripts require a NetworkIdentity on the game object to be able to function.
    /// </para>
    /// <para>
    ///     The NetworkIdentity manages the dirty state of the NetworkBehaviours of the object.
    ///     When it discovers that NetworkBehaviours are dirty, it causes an update packet to be created and sent to clients.
    /// </para>
    /// 
    /// <list type="bullet">
    ///     <listheader><description>
    ///         The flow for serialization updates managed by the NetworkIdentity is:
    ///     </description></listheader>
    ///     
    ///     <item><description>
    ///         Each NetworkBehaviour has a dirty mask. This mask is available inside OnSerialize as syncVarDirtyBits
    ///     </description></item>
    ///     <item><description>
    ///         Each SyncVar in a NetworkBehaviour script is assigned a bit in the dirty mask.
    ///     </description></item>
    ///     <item><description>
    ///         Changing the value of SyncVars causes the bit for that SyncVar to be set in the dirty mask
    ///     </description></item>
    ///     <item><description>
    ///         Alternatively, calling SetDirtyBit() writes directly to the dirty mask
    ///     </description></item>
    ///     <item><description>
    ///         NetworkIdentity objects are checked on the server as part of it&apos;s update loop
    ///     </description></item>
    ///     <item><description>
    ///         If any NetworkBehaviours on a NetworkIdentity are dirty, then an UpdateVars packet is created for that object
    ///     </description></item>
    ///     <item><description>
    ///         The UpdateVars packet is populated by calling OnSerialize on each NetworkBehaviour on the object
    ///     </description></item>
    ///     <item><description>
    ///         NetworkBehaviours that are NOT dirty write a zero to the packet for their dirty bits
    ///     </description></item>
    ///     <item><description>
    ///         NetworkBehaviours that are dirty write their dirty mask, then the values for the SyncVars that have changed
    ///     </description></item>
    ///     <item><description>
    ///         If OnSerialize returns true for a NetworkBehaviour, the dirty mask is reset for that NetworkBehaviour,
    ///         so it will not send again until its value changes.
    ///     </description></item>
    ///     <item><description>
    ///         The UpdateVars packet is sent to ready clients that are observing the object
    ///     </description></item>
    /// </list>
    /// 
    /// <list type="bullet">
    ///     <listheader><description>
    ///         On the client:
    ///     </description></listheader>
    /// 
    ///     <item><description>
    ///         an UpdateVars packet is received for an object
    ///     </description></item>
    ///     <item><description>
    ///         The OnDeserialize function is called for each NetworkBehaviour script on the object
    ///     </description></item>
    ///     <item><description>
    ///         Each NetworkBehaviour script on the object reads a dirty mask.
    ///     </description></item>
    ///     <item><description>
    ///         If the dirty mask for a NetworkBehaviour is zero, the OnDeserialize functions returns without reading any more
    ///     </description></item>
    ///     <item><description>
    ///         If the dirty mask is non-zero value, then the OnDeserialize function reads the values for the SyncVars that correspond to the dirty bits that are set
    ///     </description></item>
    ///     <item><description>
    ///         If there are SyncVar hook functions, those are invoked with the value read from the stream.
    ///     </description></item>
    /// </list>
    /// </remarks>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkIdentity")]
    [HelpURL("https://miragenet.github.io/Mirage/Articles/Components/NetworkIdentity.html")]
    public sealed class NetworkIdentity : MonoBehaviour
    {
        static readonly ILogger logger = LogFactory.GetLogger<NetworkIdentity>();

        public TransformSpawnSettings SpawnSettings = new TransformSpawnSettings(true, true, true);

        [NonSerialized]
        NetworkBehaviour[] networkBehavioursCache;

        /// <summary>
        /// Returns true if running as a client and this object was spawned by a server.
        /// </summary>
        public bool IsClient => IsSpawned && Client != null && Client.Active;

        /// <summary>
        /// Returns true if NetworkServer.active and server is not stopped.
        /// </summary>
        public bool IsServer => IsSpawned && Server != null && Server.Active;

        /// <summary>
        /// Returns true if we're on host mode.
        /// </summary>
        public bool IsLocalClient => IsSpawned && Server != null && Server.LocalClientActive;

        /// <summary>
        /// This returns true if this object is the one that represents the player on the local machine.
        /// <para>This is set when the server has spawned an object for this particular client.</para>
        /// </summary>
        public bool IsLocalPlayer => IsSpawned && Client != null && Client.Player?.Identity == this;

        /// <summary>
        /// This returns true if this object is the authoritative player object on the client.
        /// <para>This value is determined at runtime. For most objects, authority is held by the server.</para>
        /// <para>For objects that had their authority set by AssignClientAuthority on the server, this will be true on the client that owns the object. NOT on other clients.</para>
        /// </summary>
        public bool HasAuthority { get; internal set; }

        /// <summary>
        /// Unique identifier for this particular object instance, used for tracking objects between networked clients and the server.
        /// <para>This is a unique identifier for this particular GameObject instance. Use it to track GameObjects between networked clients and the server.</para>
        /// </summary>
        public uint NetId { get; internal set; }

        /// <summary>
        /// A unique identifier for NetworkIdentity objects within a scene.
        /// <para>This is used for spawning scene objects on clients.</para>
        /// <para>This Id is generated by <see cref="NetworkIdentityIdGenerator"/></para>
        /// </summary>
        [FormerlySerializedAs("m_SceneId"), FormerlySerializedAs("sceneId")]
        [SerializeField, HideInInspector]
        private ulong _sceneId = 0;

        internal ulong SceneId => _sceneId;

        /// <summary>
        /// Is this object part of a scene and have a Scene Id?
        /// </summary>
        /// <returns></returns>
        public bool IsSceneObject => _sceneId != 0;

        /// <summary>
        /// Is this object a prefab and have a <see cref="PrefabHash"/> so that it can be spawned over the network
        /// </summary>
        /// <returns></returns>
        public bool IsPrefab => !IsSceneObject && PrefabHash != 0;

        /// <summary>
        /// Has this object been spawned and have a <see cref="NetId"/>
        /// </summary>
        /// <returns></returns>
        public bool IsSpawned => NetId != 0;

        /// <summary>
        /// The NetworkServer associated with this NetworkIdentity.
        /// </summary>
        public INetworkServer Server { get; internal set; }

        /// <summary>
        /// The world this object exists in
        /// </summary>
        public NetworkWorld World { get; internal set; }

        [Header("Runtime References")]

        /// <summary>
        /// The ServerObjectManager is present only for server/host instances.
        /// </summary>
        public ServerObjectManager ServerObjectManager;

        /// <summary>
        /// The NetworkClient associated with this NetworkIdentity.
        /// </summary>
        public INetworkClient Client { get; internal set; }

        /// <summary>
        /// The ClientObjectManager is present only for client instances.
        /// </summary>
        public ClientObjectManager ClientObjectManager;

        INetworkPlayer _owner;

        /// <summary>
        /// The INetworkPlayer associated with this <see cref="NetworkIdentity">NetworkIdentity</see>. This property is only valid on server
        /// <para>Use it to return details such as the connection&apos;s identity, IP address and ready status.</para>
        /// </summary>
        public INetworkPlayer Owner
        {
            get => _owner;

            internal set
            {
                if (_owner != null)
                    _owner.RemoveOwnedObject(this);

                _owner = value;
                _owner?.AddOwnedObject(this);
            }
        }

        /// <summary>
        /// Array of NetworkBehaviours associated with this NetworkIdentity. Can be in child GameObjects.
        /// </summary>
        public NetworkBehaviour[] NetworkBehaviours
        {
            get
            {
                if (networkBehavioursCache != null)
                    return networkBehavioursCache;

                NetworkBehaviour[] components = GetComponentsInChildren<NetworkBehaviour>(true);

#if DEBUG
                foreach (NetworkBehaviour item in components)
                {
                    logger.Assert(item.Identity == this, $"Child NetworkBehaviour had a different Identity, this:{name}, Child Identity:{item.Identity.name}");
                }
#endif

                if (components.Length > byte.MaxValue)
                    throw new InvalidOperationException("Only 255 NetworkBehaviour per gameobject allowed");

                networkBehavioursCache = components;
                return networkBehavioursCache;
            }
        }

        [SerializeField, HideInInspector] private int _prefabHash;

        public int PrefabHash
        {
            get
            {
#if UNITY_EDITOR
                // This is important because sometimes OnValidate does not run (like when adding view to prefab with no child links)
                // also check for hash that is empty string, if it is, reset its ID to its real path
                if (_prefabHash == 0 || _prefabHash == StringHash.EmptyString)
                    NetworkIdentityIdGenerator.SetupIDs(this);
#endif
                return _prefabHash;
            }
            internal set
            {
                int newID = value;
                int oldId = _prefabHash;

                // they are the same, do nothing
                if (oldId == newID)
                    return;

                // new is empty
                if (newID == 0)
                {
                    throw new ArgumentException($"Can not set PrefabHash to empty guid on NetworkIdentity '{name}', old PrefabHash '{oldId}'");
                }

                // old not empty
                if (oldId != 0)
                {
                    throw new InvalidOperationException($"Can not Set PrefabHash on NetworkIdentity '{name}' because it already had an PrefabHash, current PrefabHash '{oldId}', attempted new PrefabHash '{newID}'");
                }

                // old is empty
                _prefabHash = newID;

                if (logger.LogEnabled()) logger.Log($"Settings PrefabHash on NetworkIdentity '{name}', new PrefabHash '{newID}'");
            }
        }



        [Header("Events")]
        [SerializeField] AddLateEvent _onStartServer = new AddLateEvent();
        [SerializeField] AddLateEvent _onStartClient = new AddLateEvent();
        [SerializeField] AddLateEvent _onStartLocalPlayer = new AddLateEvent();
        [SerializeField] BoolAddLateEvent _onAuthorityChanged = new BoolAddLateEvent();
        [SerializeField] AddLateEvent _onStopClient = new AddLateEvent();
        [SerializeField] AddLateEvent _onStopServer = new AddLateEvent();

        bool clientStarted;
        bool localPlayerStarted;
        bool hadAuthority;

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// <para>OnStartServer is invoked before this object is added to collection of spawned objects</para>
        /// </summary>
        public IAddLateEvent OnStartServer => _onStartServer;

        /// <summary>
        /// Called on every NetworkBehaviour when it is activated on a client.
        /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
        /// </summary>
        public IAddLateEvent OnStartClient => _onStartClient;

        /// <summary>
        /// Called when the local player object has been set up.
        /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
        /// </summary>
        public IAddLateEvent OnStartLocalPlayer => _onStartLocalPlayer;

        /// <summary>
        /// This is invoked on behaviours that have authority given or removed, see <see cref="HasAuthority">NetworkIdentity.hasAuthority</see>
        /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
        /// <para>
        /// When <see cref="AssignClientAuthority"/> or <see cref="RemoveClientAuthority"/> is called on the server, this will be called on the client that owns the object.
        /// </para>
        /// <para>
        /// When an object is spawned with <see cref="ServerObjectManager.Spawn">NetworkServer.Spawn</see> with a NetworkConnection parameter included,
        /// this will be called on the client that owns the object.
        /// </para>
        /// <para>NOTE: this even is only called for client and host</para>
        /// </summary>
        public IAddLateEvent<bool> OnAuthorityChanged => _onAuthorityChanged;

        /// <summary>
        /// This is invoked on clients when the server has caused this object to be destroyed.
        /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
        /// </summary>
        ///<summary>Called on clients when the server destroys the GameObject.</summary>
        public IAddLateEvent OnStopClient => _onStopClient;

        /// <summary>
        /// This is called on the server when the object is unspawned
        /// </summary>
        /// <remarks>Can be used as hook to save player information</remarks>
        public IAddLateEvent OnStopServer => _onStopServer;

        /// <summary>
        /// used when adding players
        /// </summary>
        /// <param name="player"></param>
        internal void SetClientOwner(INetworkPlayer player)
        {
            // do nothing if it already has an owner
            if (Owner != null && player != Owner)
            {
                throw new InvalidOperationException($"Object {this} netId={NetId} already has an owner. Use RemoveClientAuthority() first");
            }

            // otherwise set the owner connection
            Owner = player;
        }

        /// <summary>
        /// hasSpawned should always be false before runtime
        /// </summary>
        [SerializeField, HideInInspector] bool hasSpawned;
        public bool SpawnedFromInstantiate { get; private set; }

        void Awake()
        {
            if (hasSpawned)
            {
                logger.LogError($"{name} has already spawned. Don't call Instantiate for NetworkIdentities that were in the scene since the beginning (aka scene objects).  Otherwise the client won't know which object to use for a SpawnSceneObject message.");

                SpawnedFromInstantiate = true;
                Destroy(gameObject);
            }

            hasSpawned = true;
        }

        void OnValidate()
        {
            // OnValidate is not called when using Instantiate, so we can use
            // it to make sure that hasSpawned is false
            hasSpawned = false;

#if UNITY_EDITOR
            NetworkIdentityIdGenerator.SetupIDs(this);
#endif
        }

        /// <summary>
        /// Unity will Destroy all networked objects on Scene Change, so we have to handle that here silently.
        /// That means we cannot have any warning or logging in this method.
        /// </summary>
        void OnDestroy()
        {
            // Objects spawned from Instantiate are not allowed so are destroyed right away
            // we don't want to call NetworkServer.Destroy if this is the case
            if (SpawnedFromInstantiate)
                return;

            // If false the object has already been unspawned
            // if it is still true, then we need to unspawn it
            if (IsServer)
            {
                ServerObjectManager.Destroy(this);
            }
        }

        internal void StartServer()
        {
            if (logger.LogEnabled()) logger.Log($"OnStartServer {this} NetId:{NetId} SceneId:{SceneId:X}");

            _onStartServer.Invoke();
        }

        internal void StopServer()
        {
            _onStopServer.Invoke();
        }

        internal void StartClient()
        {
            if (clientStarted)
                return;
            clientStarted = true;

            _onStartClient.Invoke();
        }

        internal void StartLocalPlayer()
        {
            if (localPlayerStarted)
                return;
            localPlayerStarted = true;

            _onStartLocalPlayer.Invoke();
        }

        internal void NotifyAuthority()
        {
            if (!hadAuthority && HasAuthority)
                StartAuthority();
            if (hadAuthority && !HasAuthority)
                StopAuthority();
            hadAuthority = HasAuthority;
        }

        internal void StartAuthority()
        {
            _onAuthorityChanged.Invoke(true);
        }

        internal void StopAuthority()
        {
            _onAuthorityChanged.Invoke(false);
        }

        internal void StopClient()
        {
            _onStopClient.Invoke();
        }

        // random number that is unlikely to appear in a regular data stream
        const byte Barrier = 171;

        // paul: readstring bug prevention: https://issuetracker.unity3d.com/issues/unet-networkwriter-dot-write-causing-readstring-slash-readbytes-out-of-range-errors-in-clients
        // -> OnSerialize writes componentData, barrier, componentData, barrier,componentData,...
        // -> OnDeserialize carefully extracts each data, then deserializes the barrier and check it
        //    -> If we read too many or too few bytes,  the barrier is very unlikely to match
        //    -> we can properly track down errors
        /// <summary>
        /// Serializes component and its lengths
        /// </summary>
        /// <param name="comp"></param>
        /// <param name="writer"></param>
        /// <param name="initialState"></param>
        /// <returns></returns>
        /// <remarks>
        /// paul: readstring bug prevention
        /// <see cref="https://issuetracker.unity3d.com/issues/unet-networkwriter-dot-write-causing-readstring-slash-readbytes-out-of-range-errors-in-clients"/>
        /// <list type="bullet">
        ///     <item><description>
        ///         OnSerialize writes componentData, barrier, componentData, barrier,componentData,...
        ///     </description></item>
        ///     <item><description>
        ///         OnDeserialize carefully extracts each data, then deserializes the barrier and check it
        ///     </description></item>
        ///     <item><description>
        ///         If we read too many or too few bytes,  the barrier is very unlikely to match
        ///     </description></item>
        ///     <item><description>
        ///         We can properly track down errors
        ///     </description></item>
        /// </list>
        /// </remarks>
        void OnSerialize(NetworkBehaviour comp, NetworkWriter writer, bool initialState)
        {
            comp.OnSerialize(writer, initialState);
            if (logger.LogEnabled()) logger.Log($"OnSerializeSafely written for object={comp.name} component={comp.GetType()} sceneId={SceneId:X}");

            // serialize a barrier to be checked by the deserializer
            writer.WriteByte(Barrier);
        }

        /// <summary>
        /// serialize all components using dirtyComponentsMask
        /// <para>check ownerWritten/observersWritten to know if anything was written</para>
        /// <para>We pass dirtyComponentsMask into this function so that we can check if any Components are dirty before creating writers</para>
        /// </summary>
        /// <param name="initialState"></param>
        /// <param name="ownerWriter"></param>
        /// <param name="observersWriter"></param>
        internal (int ownerWritten, int observersWritten) OnSerializeAll(bool initialState, NetworkWriter ownerWriter, NetworkWriter observersWriter)
        {
            int ownerWritten = 0;
            int observersWritten = 0;

            // check if components are in byte.MaxRange just to be 100% sure
            // that we avoid overflows
            NetworkBehaviour[] components = NetworkBehaviours;

            // serialize all components
            for (int i = 0; i < components.Length; ++i)
            {
                // is this component dirty?
                // -> always serialize if initialState so all components are included in spawn packet
                // -> note: IsDirty() is false if the component isn't dirty or sendInterval isn't elapsed yet
                NetworkBehaviour comp = components[i];
                if (initialState || comp.IsDirty())
                {
                    if (logger.LogEnabled()) logger.Log("OnSerializeAllSafely: " + name + " -> " + comp.GetType() + " initial=" + initialState);

                    // remember start position in case we need to copy it into
                    // observers writer too
                    int startBitPosition = ownerWriter.BitPosition;

                    // write index as byte [0..255]
                    ownerWriter.WriteByte((byte)i);

                    // serialize into ownerWriter first
                    // (owner always gets everything!)
                    OnSerialize(comp, ownerWriter, initialState);
                    ownerWritten++;

                    // copy into observersWriter too if SyncMode.Observers
                    // -> we copy instead of calling OnSerialize again because
                    //    we don't know what magic the user does in OnSerialize.
                    // -> it's not guaranteed that calling it twice gets the
                    //    same result
                    // -> it's not guaranteed that calling it twice doesn't mess
                    //    with the user's OnSerialize timing code etc.
                    // => so we just copy the result without touching
                    //    OnSerialize again
                    if (comp.syncMode == SyncMode.Observers)
                    {
                        int bitLength = ownerWriter.BitPosition - startBitPosition;
                        observersWriter.CopyFromWriter(ownerWriter, startBitPosition, bitLength);
                        observersWritten++;
                    }
                }
            }

            return (ownerWritten, observersWritten);
        }

        // Determines if there are changes in any component that have not
        // been synchronized yet. Probably due to not meeting the syncInterval
        internal bool StillDirty()
        {
            foreach (NetworkBehaviour behaviour in NetworkBehaviours)
            {
                if (behaviour.StillDirty())
                    return true;
            }
            return false;
        }

        void OnDeserialize(NetworkBehaviour comp, NetworkReader reader, bool initialState)
        {
            comp.OnDeserialize(reader, initialState);

            // check if Barrier is at end of Deserialize, if it is then the Deserialize was likely a success
            byte barrierData = reader.ReadByte();
            if (barrierData != Barrier)
            {
                throw new DeserializeFailedException($"Deserialize not aligned for object={name} netId={NetId} component={comp.GetType()} sceneId={SceneId:X}. Possible Reasons:\n" +
                    $"  * Do {comp.GetType()}'s OnSerialize and OnDeserialize calls write the same amount of data? \n" +
                    $"  * Are the server and client the exact same project?\n" +
                    $"  * Maybe this OnDeserialize call was meant for another GameObject? The sceneIds can easily get out of sync if the Hierarchy was modified only in the client OR the server. Try rebuilding both.\n\n");
            }
        }

        internal void OnDeserializeAll(NetworkReader reader, bool initialState)
        {
            // needed so that we can deserialize gameobjects and NI
            reader.ObjectLocator = Client != null ? Client.World : null;
            // deserialize all components that were received
            NetworkBehaviour[] components = NetworkBehaviours;
            // check if we can read atleast 1 byte
            while (reader.CanReadBytes(1))
            {
                // todo replace index with bool for if next component in order has changed or not
                //      the index below was an alternative to a mask, but now we have bitpacking we can just use a bool for each NB index
                // read & check index [0..255]
                byte index = reader.ReadByte();
                if (index < components.Length)
                {
                    // deserialize this component
                    OnDeserialize(components[index], reader, initialState);
                }
            }

        }

        /// <summary>
        /// Helper function to handle Command/Rpc
        /// </summary>
        /// <param name="componentIndex"></param>
        /// <param name="functionHash"></param>
        /// <param name="invokeType"></param>
        /// <param name="reader"></param>
        /// <param name="senderPlayer"></param>
        internal void HandleRemoteCall(Skeleton skeleton, int componentIndex, NetworkReader reader, INetworkPlayer senderPlayer = null, int replyId = 0)
        {
            // find the right component to invoke the function on
            if (componentIndex >= 0 && componentIndex < NetworkBehaviours.Length)
            {
                NetworkBehaviour invokeComponent = NetworkBehaviours[componentIndex];
                skeleton?.Invoke(reader, invokeComponent, senderPlayer, replyId);
            }
            else
            {
                throw new MethodInvocationException($"Invalid component {componentIndex} in {this} for RPC {skeleton.invokeFunction}");
            }
        }

        internal void SetServerValues(NetworkServer networkServer, ServerObjectManager serverObjectManager)
        {
            Server = networkServer;
            ServerObjectManager = serverObjectManager;
            World = networkServer.World;
            Client = networkServer.LocalClient;
        }

        internal void SetClientValues(ClientObjectManager clientObjectManager, SpawnMessage msg)
        {
            if (msg.position.HasValue) transform.localPosition = msg.position.Value;
            if (msg.rotation.HasValue) transform.localRotation = msg.rotation.Value;
            if (msg.scale.HasValue) transform.localScale = msg.scale.Value;

            NetId = msg.netId;
            HasAuthority = msg.isOwner;
            ClientObjectManager = clientObjectManager;
            Client = ClientObjectManager.Client;
            World = Client.World;
        }

        /// <summary>
        /// Assign control of an object to a client via the client's <see cref="NetworkPlayer">NetworkConnection.</see>
        /// <para>This causes hasAuthority to be set on the client that owns the object, and NetworkBehaviour.OnStartAuthority will be called on that client. This object then will be in the NetworkConnection.clientOwnedObjects list for the connection.</para>
        /// <para>Authority can be removed with RemoveClientAuthority. Only one client can own an object at any time. This does not need to be called for player objects, as their authority is setup automatically.</para>
        /// </summary>
        /// <param name="player">	The connection of the client to assign authority to.</param>
        public void AssignClientAuthority(INetworkPlayer player)
        {
            if (!IsServer)
            {
                throw new InvalidOperationException("AssignClientAuthority can only be called on the server for spawned objects");
            }

            if (player == null)
            {
                throw new InvalidOperationException("AssignClientAuthority for " + gameObject + " owner cannot be null. Use RemoveClientAuthority() instead");
            }

            if (Owner != null && player != Owner)
            {
                throw new InvalidOperationException("AssignClientAuthority for " + gameObject + " already has an owner. Use RemoveClientAuthority() first");
            }

            SetClientOwner(player);

            // The client will match to the existing object
            // update all variables and assign authority
            ServerObjectManager.SendSpawnMessage(this, player);
        }

        /// <summary>
        /// Removes ownership for an object.
        /// <para>This applies to objects that had authority set by AssignClientAuthority, or <see cref="ServerObjectManager.Spawn">NetworkServer.Spawn</see> with a NetworkConnection parameter included.</para>
        /// <para>Authority cannot be removed for player objects.</para>
        /// </summary>
        public void RemoveClientAuthority()
        {
            if (!IsServer)
            {
                throw new InvalidOperationException("RemoveClientAuthority can only be called on the server for spawned objects");
            }

            if (Owner?.Identity == this)
            {
                throw new InvalidOperationException("RemoveClientAuthority cannot remove authority for a player object");
            }

            if (Owner != null)
            {
                INetworkPlayer previousOwner = Owner;

                Owner = null;

                // we DONT need to resynchronize the entire object
                // so only send a message telling client that it no longer has authority
                ServerObjectManager.SendRemoveAuthorityMessage(this, previousOwner);
            }
        }


        /// <summary>
        /// Marks the identity for future reset, this is because we cant reset the identity during destroy
        /// as people might want to be able to read the members inside OnDestroy(), and we have no way
        /// of invoking reset after OnDestroy is called.
        /// </summary>
        // IMPORTANT: dont use `Reset` as function name because that is a unity callback
        internal void NetworkReset()
        {
            ResetSyncObjects();

            hasSpawned = false;
            clientStarted = false;
            localPlayerStarted = false;
            NetId = 0;
            Server = null;
            Client = null;
            ServerObjectManager = null;
            ClientObjectManager = null;
            Owner = null;
            networkBehavioursCache = null;

            ResetEvents();
        }

        private void ResetEvents()
        {
            // resets stored args and invoked flag
            _onStartServer.Reset();
            _onStartClient.Reset();
            _onStartLocalPlayer.Reset();
            _onAuthorityChanged.Reset();
            _onStopClient.Reset();
            _onStopServer.Reset();
        }

        internal void UpdateVars()
        {
            SendUpdateVarsMessage();
        }

        void SendUpdateVarsMessage()
        {
            // one writer for owner, one for observers
            using (PooledNetworkWriter ownerWriter = NetworkWriterPool.GetWriter(), observersWriter = NetworkWriterPool.GetWriter())
            {
                // serialize all the dirty components and send
                (int ownerWritten, int observersWritten) = OnSerializeAll(false, ownerWriter, observersWriter);
                if (ownerWritten > 0 || observersWritten > 0)
                {
                    var varsMessage = new UpdateVarsMessage
                    {
                        netId = NetId
                    };

                    // send ownerWriter to owner
                    // (only if we serialized anything for owner)
                    // (only if there is a connection (e.g. if not a monster),
                    //  and if connection is ready because we use SendToReady
                    //  below too)
                    if (ownerWritten > 0)
                    {
                        varsMessage.payload = ownerWriter.ToArraySegment();
                        if (Owner != null && Owner.SceneIsReady)
                            Owner.Send(varsMessage);
                    }

                    // send observersWriter to everyone but owner
                    // (only if we serialized anything for observers)
                    if (observersWritten > 0)
                    {
                        varsMessage.payload = observersWriter.ToArraySegment();

                        ServerObjectManager.InterestManager.Send(this, varsMessage, Channel.Reliable, Server.LocalPlayer);
                    }

                    // clear dirty bits only for the components that we serialized
                    // DO NOT clean ALL component's dirty bits, because
                    // components can have different syncIntervals and we don't
                    // want to reset dirty bits for the ones that were not
                    // synced yet.
                    // (we serialized only the IsDirty() components, or all of
                    //  them if initialState. clearing the dirty ones is enough.)
                    ClearDirtyComponentsDirtyBits();
                }
            }
        }

        /// <summary>
        /// Clear only dirty component's dirty bits. ignores components which
        /// may be dirty but not ready to be synced yet (because of syncInterval)
        /// </summary>
        internal void ClearDirtyComponentsDirtyBits()
        {
            foreach (NetworkBehaviour comp in NetworkBehaviours)
            {
                if (comp.IsDirty())
                {
                    comp.ClearAllDirtyBits();
                }
            }
        }

        void ResetSyncObjects()
        {
            foreach (NetworkBehaviour comp in NetworkBehaviours)
            {
                comp.ResetSyncObjects();
            }
        }

        [System.Serializable]
        public struct TransformSpawnSettings
        {
            public bool SendPosition;
            public bool SendRotation;
            public bool SendScale;

            public TransformSpawnSettings(bool sendPosition, bool sendRotation, bool sendScale)
            {
                SendPosition = sendPosition;
                SendRotation = sendRotation;
                SendScale = sendScale;
            }
        }
    }
}
