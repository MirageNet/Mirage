using System;
using System.Collections.Generic;
using Mirage.Events;
using Mirage.Logging;
using Mirage.RemoteCalls;
using Mirage.Serialization;
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
    [HelpURL("https://miragenet.github.io/Mirage/docs/components/network-identity")]
    public sealed class NetworkIdentity : MonoBehaviour
    {
        private static readonly ILogger logger = LogFactory.GetLogger<NetworkIdentity>();

        public NetworkSpawnSettings SpawnSettings = NetworkSpawnSettings.Default;

        [NonSerialized]
        private NetworkBehaviour[] _networkBehavioursCache;

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
        /// The set of network connections (players) that can see this object.
        /// </summary>
        public readonly HashSet<INetworkPlayer> observers = new HashSet<INetworkPlayer>();

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
        public NetworkServer Server { get; internal set; }

        /// <summary>
        /// The world this object exists in
        /// </summary>
        public NetworkWorld World { get; internal set; }

        public SyncVarSender SyncVarSender { get; internal set; }

        /// <summary>
        /// True while applying spawn payload within OnDeserializeAll
        /// <para>Can be used inside syncvar hooks to tell if object has just spawned</para>
        /// </summary>
        public bool InitialState { get; private set; }

        [Header("Runtime References")]

        /// <summary>
        /// The ServerObjectManager is present only for server/host instances.
        /// </summary>
        [ReadOnlyInspector]
        [Tooltip("Reference to Server set after the object is spawned. Used when debugging to see which server this object belongs to.")]
        public ServerObjectManager ServerObjectManager;

        /// <summary>
        /// The NetworkClient associated with this NetworkIdentity.
        /// </summary>
        public NetworkClient Client { get; internal set; }

        /// <summary>
        /// The ClientObjectManager is present only for client instances.
        /// </summary>
        [ReadOnlyInspector]
        [Tooltip("Reference to Client set after the object is spawned. Used when debugging to see which client this object belongs to.")]
        public ClientObjectManager ClientObjectManager;
        private INetworkPlayer _owner;

        /// <summary>
        /// The INetworkPlayer associated with this <see cref="NetworkIdentity">NetworkIdentity</see>. This property is only valid on server
        /// <para>Use it to return details such as the connection&apos;s identity, IP address and ready status.</para>
        /// </summary>
        public INetworkPlayer Owner => _owner;

        internal void SetOwner(INetworkPlayer player)
        {
            // do nothing if value is the same
            if (_owner == player)
                return;

            // if owner is already set, then we can only set it to null
            if (_owner != null && player != null)
                throw new InvalidOperationException($"Object '{this}' (NetID {NetId}) already has an owner. Please call RemoveClientAuthority() first.");

            if (_owner != null)
            {
                // invoke OnAuthority for remove owner and then again if there is new owner
                // world can be null if owner is set before object is spawned
                World?.InvokeOnAuthorityChanged(this, false, _owner);
                _owner.RemoveOwnedObject(this);
            }

            _owner = player;
            _owner?.AddOwnedObject(this);

            // if authority changes, we need to check if we are still allowed to sync to/from this instance
            foreach (var comp in NetworkBehaviours)
                comp.UpdateSyncObjectShouldSync();

            _onOwnerChanged.Invoke(_owner);

            // only invoke again if new owner is not null
            if (_owner != null)
                World?.InvokeOnAuthorityChanged(this, true, _owner);
        }

        /// <summary>
        /// Array of NetworkBehaviours associated with this NetworkIdentity. Can be in child GameObjects.
        /// </summary>
        public NetworkBehaviour[] NetworkBehaviours
        {
            get
            {
                if (_networkBehavioursCache is null)
                {
                    var components = FindBehaviourForThisIdentity();

                    // we write component index as byte
                    // check if components are in byte.MaxRange just to be 100% sure that we avoid overflows
                    if (components.Length > byte.MaxValue)
                        throw new InvalidOperationException("Only 255 NetworkBehaviours are allowed per GameObject.");

                    _networkBehavioursCache = components;
                }

                return _networkBehavioursCache;
            }
        }

        // cache list to call GetComponentsInChildren() with no allocations
        private static readonly List<NetworkBehaviour> childNetworkBehavioursCache = new List<NetworkBehaviour>();

        /// <summary>
        /// Removes NetworkBehaviour that belong to another NetworkIdentity from the components array
        /// <para>
        ///     If there are nested NetworkIdentities then Behaviour that belong to those Identities will be found by GetComponentsInChildren if the child object is added
        ///     before the Array is intialized. This method will check each Behaviour to make sure that the Identity is the same as the current Identity, and if it is not
        ///     remove it from the array.
        /// </para>
        /// </summary>
        /// <param name="components"></param>
        private NetworkBehaviour[] FindBehaviourForThisIdentity()
        {
            GetComponentsInChildren<NetworkBehaviour>(true, childNetworkBehavioursCache);

            // start at last so we can remove from end of array instead of start
            for (var i = childNetworkBehavioursCache.Count - 1; i >= 0; i--)
            {
                var item = childNetworkBehavioursCache[i];
                if (item.Identity != this)
                {
                    childNetworkBehavioursCache.RemoveAt(i);
                }
            }

            var components = childNetworkBehavioursCache.ToArray();

#if DEBUG
            // validate the results here (just incase they are wrong)
            // we only need to do this in debug mode because results should be right
            foreach (var item in components)
            {
                // assert
                if (item.Identity != this)
                {
                    logger.LogError($"Child NetworkBehaviour had a different Identity, this:{name}, Child Identity:{item.Identity.name}");
                }
            }
#endif
            return components;
        }

        private INetworkVisibility _visibility;
        /// <summary>
        /// Returns the NetworkVisibility behaviour on this gameObject, or a default visibility where all objects are visible.
        /// <para>Note: NetworkVisibility must be on same gameObject has NetworkIdentity, not on a child object</para>
        /// </summary>
        public INetworkVisibility Visibility
        {
            get
            {
                if (_visibility is null)
                {
                    // try get behaviour, otherwise just set default class
                    if (TryGetComponent<NetworkVisibility>(out var visibilityBehaviour))
                        _visibility = visibilityBehaviour;
                    else
                    {
                        if (ServerObjectManager == null)
                            throw new InvalidOperationException("Can't get default Visibility before object is spawned");

                        var defaultVisibility = ServerObjectManager.DefaultVisibility;
                        if (defaultVisibility is null)
                            throw new InvalidOperationException("DefaultVisibility was null on ObjectManager, make sure ServerObjectManager has referecne to server, and server has started");

                        _visibility = defaultVisibility;
                    }
                }

                return _visibility;
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
                if (value == 0)
                {
                    throw new ArgumentException($"Cannot set PrefabHash to 0 on '{name}'. Old PrefabHash '{_prefabHash}'.");
                }

                var old = _prefabHash;
                _prefabHash = value;

                if (logger.LogEnabled()) logger.Log($"Setting PrefabHash on '{name}' to '{value}', Old PrefabHash:{old}");
            }
        }

#if UNITY_EDITOR || MIRAGE_TESTS
        /// <summary>
        /// Gets PrefabHash avoiding runtime checks
        /// <para>used by NetworkIdentityIdGenerator</para>
        /// </summary>
        internal int Editor_PrefabHash
        {
            get => _prefabHash;
            set => _prefabHash = value;
        }

        /// <summary>
        /// Gets SceneId avoiding runtime checks
        /// <para>used by NetworkIdentityIdGenerator</para>
        /// </summary>
        internal ulong Editor_SceneId
        {
            get => _sceneId;
            set => _sceneId = value;
        }
#endif


        [Header("Events")]
        [SerializeField] private AddLateEvent _onStartServer = new AddLateEvent();
        [SerializeField] private AddLateEvent _onStartClient = new AddLateEvent();
        [SerializeField] private AddLateEvent _onStartLocalPlayer = new AddLateEvent();
        [SerializeField] private BoolAddLateEvent _onAuthorityChanged = new BoolAddLateEvent();
        [SerializeField] private NetworkPlayerAddLateEvent _onOwnerChanged = new NetworkPlayerAddLateEvent();
        [SerializeField] private AddLateEvent _onStopClient = new AddLateEvent();
        [SerializeField] private AddLateEvent _onStopServer = new AddLateEvent();
        private bool _clientStarted;
        private bool _localPlayerStarted;
        private bool _hadAuthority;

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Start() for objects in the scene, or by ServerObjectManager.Spawn() for objects you spawn at runtime.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// <para>OnStartServer is invoked before this object is added to collection of spawned objects</para>
        /// </summary>
        public IAddLateEvent OnStartServer => _onStartServer;

        /// <summary>
        /// Called on every NetworkBehaviour when it is activated on a client.
        /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized
        /// correctly with the latest state from the server when this function is called on the client.</para>
        /// </summary>
        public IAddLateEvent OnStartClient => _onStartClient;

        /// <summary>
        /// Called when the local player object has been set up.
        /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or
        /// functionality that should only be active for the local player, such as cameras and input.</para>
        /// </summary>
        public IAddLateEvent OnStartLocalPlayer => _onStartLocalPlayer;

        /// <summary>
        /// This is invoked on behaviours that have authority given or removed, see <see cref="HasAuthority">NetworkIdentity.hasAuthority</see>
        /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
        /// <para>
        /// When <see cref="AssignClientAuthority"/> or <see cref="RemoveClientAuthority"/> is called on the server, this will be called on the client that owns the object.
        /// </para>
        /// <para>
        /// When an object is spawned with <see cref="ServerObjectManager.Spawn">ServerObjectManager.Spawn</see> with a NetworkConnection parameter included,
        /// this will be called on the client that owns the object.
        /// </para>
        /// <para>NOTE: this even is only called for client and host</para>
        /// </summary>
        public IAddLateEvent<bool> OnAuthorityChanged => _onAuthorityChanged;

        /// <summary>
        /// This is invoked on behaviours that have an owner assigned.
        /// <para>This even is only called on server</para>
        /// <para>See <see cref="OnAuthorityChanged"/> for more comments on owner and authority</para>
        /// </summary>
        public IAddLateEvent<INetworkPlayer> OnOwnerChanged => _onOwnerChanged;

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
        /// this is used when a connection is destroyed, since the "observers" property is read-only
        /// </summary>
        /// <param name="player"></param>
        internal void RemoveObserverInternal(INetworkPlayer player)
        {
            observers.Remove(player);
        }

        /// <summary>
        /// hasSpawned should always be false before runtime
        /// </summary>
        [SerializeField, HideInInspector] private bool _hasSpawned;
        public bool SpawnedFromInstantiate { get; private set; }

        private void Awake()
        {
            if (_hasSpawned)
            {
                logger.LogError($"Object '{name}' (NetID {NetId}) has already been spawned. Don't call Instantiate for NetworkIdentities that were in the scene " +
                    $"since the beginning (aka scene objects). Otherwise the client won't know which object to use for a SpawnSceneObject message.");

                SpawnedFromInstantiate = true;
                Destroy(gameObject);
            }

            _hasSpawned = true;
        }

        private void OnValidate()
        {
            // OnValidate is not called when using Instantiate, so we can use
            // it to make sure that hasSpawned is false
            _hasSpawned = false;

#if UNITY_EDITOR
            NetworkIdentityIdGenerator.SetupIDs(this);
#endif
        }

        /// <summary>
        /// Unity will Destroy all networked objects on Scene Change, so we have to handle that here silently.
        /// That means we cannot have any warning or logging in this method.
        /// </summary>
        private void OnDestroy()
        {
            // Objects spawned from Instantiate are not allowed so are destroyed right away
            // we don't want to call ServerObjectManager.Destroy if this is the case
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
            if (logger.LogEnabled()) logger.Log($"OnStartServer invoked on '{this}' (NetId: {NetId}, SceneId: {SceneId:X})");

            // update sync direction before invoking start callback
            // need to do this because IsServer might now be set when it previosuly wasn't
            foreach (var comp in NetworkBehaviours)
                comp.UpdateSyncObjectShouldSync();

            _onStartServer.Invoke();
        }

        internal void StopServer()
        {
            _onStopServer.Invoke();
        }

        internal void StartClient()
        {
            if (_clientStarted)
                return;

            // update sync direction before invoking start callback
            // need to do this because IsClient/Owner might now be set when it previosuly wasn't
            foreach (var comp in NetworkBehaviours)
                comp.UpdateSyncObjectShouldSync();

            _clientStarted = true;
            _onStartClient.Invoke();
        }

        internal void StartLocalPlayer()
        {
            if (_localPlayerStarted)
                return;
            _localPlayerStarted = true;

            _onStartLocalPlayer.Invoke();
        }

        internal void NotifyAuthority()
        {
            // if authority changes, we need to check if we are still allowed to sync to/from this instance
            foreach (var comp in NetworkBehaviours)
                comp.UpdateSyncObjectShouldSync();

            if (!_hadAuthority && HasAuthority)
                CallStartAuthority();
            if (_hadAuthority && !HasAuthority)
                CallStopAuthority();
            _hadAuthority = HasAuthority;
        }

        internal void CallStartAuthority()
        {
            _onAuthorityChanged.Invoke(true);

            // dont invoke in host mode, server will invoke it when owner is changed
            if (!IsServer)
                World.InvokeOnAuthorityChanged(this, true, null);
        }

        internal void CallStopAuthority()
        {
            _onAuthorityChanged.Invoke(false);

            // dont invoke in host mode, server will invoke it when owner is changed
            if (!IsServer)
                World.InvokeOnAuthorityChanged(this, false, null);
        }

        /// <summary>
        /// Check if observer can be seen by player.
        /// <returns></returns>
        internal bool OnCheckObserver(INetworkPlayer player)
        {
            return Visibility.OnCheckObserver(player);
        }

        internal void StopClient()
        {
            _onStopClient.Invoke();
        }

        // random number that is unlikely to appear in a regular data stream
        private const byte BARRIER = 171;

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
        private void OnSerialize(int i, NetworkBehaviour comp, NetworkWriter writer, bool initialState)
        {
            // write index as byte [0..255]
            writer.WriteByte((byte)i);

            comp.OnSerialize(writer, initialState);
            if (logger.LogEnabled()) logger.Log($"OnSerializeSafely written for '{comp.name}', Component '{comp.GetType()}', SceneId {SceneId:X}");

            // serialize a barrier to be checked by the deserializer
            writer.WriteByte(BARRIER);
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
            // how many times it written to (NOT BYTES)
            var ownerWritten = 0;
            var observersWritten = 0;


            var components = NetworkBehaviours;
            // store time as variable so we dont have to call property for each component
            var now = Time.time;

            // serialize all components
            for (var i = 0; i < components.Length; ++i)
            {
                var comp = components[i];

                // always sync for initial
                // so check ShouldSync if initial is false
                if (!initialState && !comp.ShouldSync(now))
                    continue;

                // check if we should be writing this components
                if (!comp.SyncSettings.ShouldSyncFrom(this))
                    continue;

                if (logger.LogEnabled()) logger.Log($"OnSerializeAllSafely: '{name}', component '{comp.GetType()}', initial state: '{initialState}'");


                // if only observers, then just write directly to observersWriter
                if (comp.SyncSettings.ToObserverWriterOnly(this))
                {
                    OnSerialize(i, comp, observersWriter, initialState);
                    observersWritten++;
                }
                // else write to ownerWriter (for owner or server) then coy into observer writer 
                else
                {
                    // remember start position in case we need to copy it into
                    // observers writer too
                    var startBitPosition = ownerWriter.BitPosition;

                    OnSerialize(i, comp, ownerWriter, initialState);
                    ownerWritten++;

                    // should copy to observer writer
                    if (comp.SyncSettings.CopyToObservers(this))
                    {
                        // copy into observersWriter too if SyncMode.Observers
                        // -> faster than doing full serialize again
                        // -> we copy instead of calling OnSerialize again because
                        //    we don't know what magic the user does in OnSerialize.
                        // -> it's not guaranteed that calling it twice gets the
                        //    same result
                        // -> it's not guaranteed that calling it twice doesn't mess
                        //    with the user's OnSerialize timing code etc.
                        // => so we just copy the result without touching
                        //    OnSerialize again
                        var bitLength = ownerWriter.BitPosition - startBitPosition;
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
            foreach (var behaviour in NetworkBehaviours)
            {
                if (behaviour.AnyDirtyBits())
                    return true;
            }
            return false;
        }

        internal void OnDeserializeAll(NetworkReader reader, bool initialState)
        {
            // set InitialState before deserializing so that syncvar hooks and other methods can check it
            InitialState = initialState;

            PooledNetworkWriter observerWriter = null;
            try
            {
                // deserialize all components that were received
                var components = NetworkBehaviours;
                // check if we can read at least 1 byte
                while (reader.CanReadBytes(1))
                {
                    var startPosition = reader.BitPosition;

                    // read & check index [0..255]
                    var index = reader.ReadByte();
                    ThrowIfIndexOutOfRange(components, index);

                    // deserialize this component
                    var comp = components[index];
                    OnDeserialize(comp, reader, initialState);

                    // check if owner values should be forwarded to observer
                    CheckForwardToObservers(comp, reader, startPosition, ref observerWriter);
                }

                if (observerWriter != null)
                    ForwardToObservers(observerWriter);
            }
            finally
            {
                // make sure writer is released even if there is Exception
                // note: we dont send message here, we send inside try.
                //       we dont want to send a message if an exception was thrown while deserializing
                observerWriter?.Release();

                // reset flag at end,
                // just incase people are checking initial at wrong time, in that case we want it to be false
                InitialState = false;
            }
        }

        private void OnDeserialize(NetworkBehaviour comp, NetworkReader reader, bool initialState)
        {
            // check we are allow to read for this comp
            ThrowIfInvalidSendToServer(comp);

            comp.OnDeserialize(reader, initialState);

            // check if Barrier is at end of Deserialize, if it is then the Deserialize was likely a success
            var barrierData = reader.ReadByte();
            ThrowIfBarrierByteIncorrect(comp, barrierData);

            // clear bits, local values would have been overwritten if previously changed
            // do this after Deserialize is successful, we dont want too clear bits if there was an exception
            comp.ClearDirtyBit(comp._deserializeMask);
        }

        private void ThrowIfIndexOutOfRange(NetworkBehaviour[] components, byte index)
        {
            if (index >= components.Length)
            {
                throw new DeserializeFailedException($"Deserialization failure component index out of range on networked object '{name}' (NetId {NetId}, SceneId {SceneId:X})." +
                    $" Possible Reasons:\n" +
                    $"  * Component added at runtime causing behaviour array to be mismatched\n" +
                    $"  * out dated version of prefab on either server or client, Try rebuilding both.\n\n");
            }
        }
        private void ThrowIfInvalidSendToServer(NetworkBehaviour comp)
        {
            // if we are server, but SyncSettings is not To.Server, then we should not be reading or appling any values
            // this is a problem we can't resolve (because we dont known length
            // so we have to throw
            var notToServer = (comp.SyncSettings.To & SyncTo.Server) == 0;
            if (notToServer && IsServer)
            {
                throw new DeserializeFailedException($"Invalid sync settings on '{comp.GetType()}' on networked object '{name}' (NetId {NetId}, SceneId {SceneId:X})." +
                   $" Possible Reason: SyncSettings was changed to SyncTo.Server at runtime, but not udpated on server.\n" +
                   $"Ensure SyncSettings is same on both Server and Client, this may rebuilding both.");
            }
        }
        private void ThrowIfBarrierByteIncorrect(NetworkBehaviour comp, byte barrierData)
        {
            if (barrierData != BARRIER)
            {
                throw new DeserializeFailedException($"Deserialization failure for component '{comp.GetType()}' on networked object '{name}' (NetId {NetId}, SceneId {SceneId:X})." +
                    $" Possible Reasons:\n" +
                    $"  * Do {comp.GetType()}'s OnSerialize and OnDeserialize calls write the same amount of data?\n" +
                    $"  * Did something fail in {comp.GetType()}'s OnSerialize/OnDeserialize code?\n" +
                    $"  * Are the server and client instances built from the exact same project?\n" +
                    $"  * Maybe this OnDeserialize call was meant for another GameObject? The sceneIds can easily get out of sync if the Hierarchy was modified only on the client " +
                    $"OR the server. Try rebuilding both.\n\n");
            }
        }

        private unsafe void CheckForwardToObservers(NetworkBehaviour comp, NetworkReader reader, int startPosition, ref PooledNetworkWriter writer)
        {
            // if we are not server, we can't forward to anywhere
            if (!comp.IsServer)
                return;

            var toObserver = (comp.SyncSettings.To & SyncTo.ObserversOnly) != 0;
            if (!toObserver)
                return;

            // then copy reader data into writer that will be send at end of OnDeserializeAll
            // we need to copy into writer instead of all using ther whole segment because only some component might have To.Observer

            // if we dont have writer yet, get now
            if (writer == null)
                writer = NetworkWriterPool.GetWriter();

            // copy the data we just read for this component into writer
            var endPosition = reader.BitPosition;
            var length = endPosition - startPosition;
            writer.CopyFromPointer(reader.BufferPointer, startPosition, length);
        }

        private void ForwardToObservers(NetworkWriter writer)
        {
            var varsMessage = new UpdateVarsMessage
            {
                NetId = NetId,
                Payload = writer.ToArraySegment(),
            };

            this.SendToRemoteObservers(varsMessage, includeOwner: false);
        }

        internal void SetServerValues(NetworkServer networkServer, ServerObjectManager serverObjectManager)
        {
            Server = networkServer;
            ServerObjectManager = serverObjectManager;
            World = networkServer.World;
            SyncVarSender = networkServer.SyncVarSender;
            Client = networkServer.LocalClient;

            foreach (var behaviour in NetworkBehaviours)
                behaviour.InitializeSyncObjects();
        }

        internal void SetClientValues(ClientObjectManager clientObjectManager, SpawnMessage msg)
        {
            var spawnValues = msg.SpawnValues;
            if (spawnValues.Position.HasValue) transform.localPosition = spawnValues.Position.Value;
            if (spawnValues.Rotation.HasValue) transform.localRotation = spawnValues.Rotation.Value;
            if (spawnValues.Scale.HasValue) transform.localScale = spawnValues.Scale.Value;
            if (!string.IsNullOrEmpty(spawnValues.Name)) gameObject.name = spawnValues.Name;
            if (spawnValues.SelfActive.HasValue) gameObject.SetActive(spawnValues.SelfActive.Value);

            NetId = msg.NetId;
            HasAuthority = msg.IsOwner;
            ClientObjectManager = clientObjectManager;
            Client = ClientObjectManager.Client;

            if (!IsServer)
            {
                World = Client.World;
                SyncVarSender = Client.SyncVarSender;
            }

            foreach (var behaviour in NetworkBehaviours)
                behaviour.InitializeSyncObjects();
        }

        /// <summary>
        /// Called when NetworkIdentity is destroyed
        /// </summary>
        internal void ClearObservers()
        {
            foreach (var player in observers)
            {
                player.RemoveFromVisList(this);
            }
            observers.Clear();
        }

        internal void AddObserver(INetworkPlayer player)
        {
            if (observers.Contains(player))
            {
                // if we try to add a connectionId that was already added, then
                // we may have generated one that was already in use.
                return;
            }

            if (logger.LogEnabled()) logger.Log($"Adding '{player.Connection.EndPoint}' as an observer for {gameObject}");
            observers.Add(player);
            player.AddToVisList(this);

            // spawn identity for this conn
            ServerObjectManager.ShowToPlayer(this, player);
        }

        /// <summary>
        /// Helper function to call OnRebuildObservers on <see cref="Visibility"/> using <see cref="NetworkServer.Players"/> 
        /// </summary>
        /// <param name="observersSet">set to clear and fill with new observers</param>
        /// <param name="initialize">If Object is being first spawned or refreshed later on</param>
        internal void GetNewObservers(HashSet<INetworkPlayer> observersSet, bool initialize)
        {
            observersSet.Clear();

            Visibility.OnRebuildObservers(observersSet, initialize);
        }

        private static readonly HashSet<INetworkPlayer> newObservers = new HashSet<INetworkPlayer>();

        /// <summary>
        /// This causes the set of players that can see this object to be rebuild.
        /// The OnRebuildObservers callback function will be invoked on each NetworkBehaviour.
        /// </summary>
        /// <param name="initialize">True if this is the first time.</param>
        public void RebuildObservers(bool initialize)
        {
            // call OnRebuildObservers function
            GetNewObservers(newObservers, initialize);

            // ensure player always sees objects they own
            if (Owner != null && Owner.SceneIsReady)
            {
                newObservers.Add(Owner);
            }

            var added = AddNewObservers(initialize);
            var removed = RemoveOldObservers();
            var changed = added || removed;

            if (changed)
            {
                observers.Clear();
                foreach (var player in newObservers)
                {
                    if (player != null && player.SceneIsReady)
                        observers.Add(player);
                }
            }
        }

        // remove all old .observers that aren't in newObservers anymore
        private bool RemoveOldObservers()
        {
            var changed = false;
            foreach (var player in observers)
            {
                if (!newObservers.Contains(player))
                {
                    // removed observer
                    player.RemoveFromVisList(this);
                    ServerObjectManager.HideToPlayer(this, player);

                    if (logger.LogEnabled()) logger.Log($"Removed observer '{player}' for {gameObject}");
                    changed = true;
                }
            }

            return changed;
        }

        // add all newObservers that aren't in .observers yet
        private bool AddNewObservers(bool initialize)
        {
            var changed = false;
            foreach (var player in newObservers)
            {
                // only add ready connections.
                // otherwise the player might not be in the world yet or anymore
                if (player != null && player.SceneIsReady && (initialize || !observers.Contains(player)))
                {
                    // new observer
                    player.AddToVisList(this);
                    // spawn identity for this conn
                    ServerObjectManager.ShowToPlayer(this, player);
                    if (logger.LogEnabled()) logger.Log($"Added new observer '{player}' for {gameObject}");
                    changed = true;
                }
            }

            return changed;
        }

        /// <summary>
        /// Assign control of an object to a client via the client's <see cref="NetworkPlayer">NetworkConnection.</see>
        /// <para>This causes hasAuthority to be set on the client that owns the object, and NetworkBehaviour.OnStartAuthority will be called on that client. This object then will be
        /// in the NetworkConnection.clientOwnedObjects list for the connection.</para>
        /// <para>Authority can be removed with RemoveClientAuthority. Only one client can own an object at any time. This does not need to be called for player objects, as their
        /// authority is setup automatically.</para>
        /// </summary>
        /// <param name="player">	The connection of the client to assign authority to.</param>
        public void AssignClientAuthority(INetworkPlayer player)
        {
            if (!IsServer)
            {
                // You can't do that, you're not a server.
                throw new InvalidOperationException("Only the server can call AssignClientAuthority on spawned objects.");
            }

            if (player == null)
            {
                // The player is null. How'd that happen? Are we trying to deassign the owner? (If so, tell them to use RemoveClientAuthority instead).
                throw new ArgumentNullException($"Cannot assign a null owner to '{gameObject}'. Please use RemoveClientAuthority() instead.");
            }

            if (Owner != null && player != Owner)
            {
                // Trying to assign another owner to an already owned object.
                throw new InvalidOperationException($"Cannot assign a new owner to '{gameObject}' as it already has an owner. Please call RemoveClientAuthority() first.");
            }

            SetOwner(player);

            // The client will match to the existing object
            // update all variables and assign authority
            ServerObjectManager.SendSpawnMessage(this, player);
        }

        /// <summary>
        /// Removes ownership for an object.
        /// <para>This applies to objects that had authority set by AssignClientAuthority, or <see cref="ServerObjectManager.Spawn">ServerObjectManager.Spawn</see> with a NetworkConnection
        /// parameter included.</para>
        /// <para>Authority cannot be removed for player objects.</para>
        /// </summary>
        public void RemoveClientAuthority()
        {
            if (!IsServer)
            {
                // You can't do that, you're not a server.
                throw new InvalidOperationException("Only the server can call RemoveClientAuthority on spawned objects.");
            }

            if (Owner?.Identity == this)
            {
                // You can't remove your own authority.
                throw new InvalidOperationException($"RemoveClientAuthority cannot remove authority from the player's character '{gameObject}'.");
            }

            if (Owner != null)
            {
                var previousOwner = Owner;

                SetOwner(null);

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

            _hasSpawned = false;
            _clientStarted = false;
            _localPlayerStarted = false;
            NetId = 0;
            Server = null;
            Client = null;
            ServerObjectManager = null;
            ClientObjectManager = null;
            _owner = null;
            _networkBehavioursCache = null;

            ClearObservers();
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

        /// <summary>
        /// Clears dirty bits and sets the next sync time on each Component 
        /// </summary>
        internal void ClearShouldSync()
        {
            // store time as variable so we dont have to call property for each component
            var now = Time.time;

            foreach (var comp in NetworkBehaviours)
            {
                comp.ClearShouldSync(now);
            }
        }

        /// <summary>
        /// Clear dirty bits of Component only if it is after syncInterval
        /// <para>
        /// Note: generally this is called after syncing to clear dirty bits of components we just synced
        /// </para>
        /// </summary>
        internal void ClearShouldSyncDirtyOnly()
        {
            // store time as variable so we dont have to call property for each component
            var now = Time.time;

            foreach (var comp in NetworkBehaviours)
            {
                // todo this seems weird, should we be clearing this somewhere else?
                if (comp.TimeToSync(now))
                {
                    comp.ClearShouldSync(now);
                }
            }
        }

        private void ResetSyncObjects()
        {
            foreach (var comp in NetworkBehaviours)
            {
                comp.ResetSyncObjects();
            }
        }

        public override string ToString()
        {
            return $"Identity[{NetId}, {name}]";
        }


        // todo update comment
        /// <summary>
        /// Collection that holds information about all RPC in this networkbehaviour (including derived classes)
        /// <para>Can be used to get RPC name from its index</para>
        /// <para>NOTE: Weaver uses this collection to add rpcs, If adding your own rpc do at your own risk</para>
        /// </summary>
        [NonSerialized]
        private RemoteCallCollection _remoteCallCollection;
        internal RemoteCallCollection RemoteCallCollection
        {
            get
            {
                if (_remoteCallCollection == null)
                {
                    // we should be save to lazy init
                    // we only need to register RPCs when we receive them
                    // when sending the index is baked in by weaver
                    _remoteCallCollection = new RemoteCallCollection();
                    _remoteCallCollection.RegisterAll(NetworkBehaviours);
                }
                return _remoteCallCollection;
            }
        }
    }
}
