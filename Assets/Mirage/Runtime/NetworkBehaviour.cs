using System;
using System.Collections.Generic;
using Mirage.Collections;
using Mirage.Logging;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Sync to everyone, or only to owner.
    /// </summary>
    public enum SyncMode { Observers, Owner }

    /// <summary>
    /// Base class which should be inherited by scripts which contain networking functionality.
    ///
    /// </summary>
    /// <remarks>
    /// <para>This is a MonoBehaviour class so scripts which need to use the networking feature should inherit this class instead of MonoBehaviour. It allows you to invoke networked actions, receive various callbacks, and automatically synchronize state from server-to-client.</para>
    /// <para>The NetworkBehaviour component requires a NetworkIdentity on the same game object or any of its parents. There can be multiple NetworkBehaviours on a single game object. For an object with sub-components in a hierarchy, the NetworkIdentity must be on the root object, and NetworkBehaviour scripts can be on the root object or any of its children.</para>
    /// <para>Some of the built-in components of the networking system are derived from NetworkBehaviour, including NetworkTransport, NetworkAnimator and NetworkProximityChecker.</para>
    /// </remarks>
    [AddComponentMenu("")]
    [HelpURL("https://miragenet.github.io/Mirage/docs/guides/game-objects/network-behaviour")]
    public abstract class NetworkBehaviour : MonoBehaviour
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkBehaviour));

        internal float _lastSyncTime;

        // hidden because NetworkBehaviourInspector shows it only if has OnSerialize.
        /// <summary>
        /// sync mode for OnSerialize
        /// </summary>
        [HideInInspector] public SyncMode syncMode = SyncMode.Observers;

        // hidden because NetworkBehaviourInspector shows it only if has OnSerialize.
        /// <summary>
        /// sync interval for OnSerialize (in seconds)
        /// </summary>
        [Tooltip("Time in seconds until next change is synchronized to the client. '0' means send immediately if changed. '0.5' means only send changes every 500ms.\n(This is for state synchronization like SyncVars, SyncLists, OnSerialize. Not for Cmds, Rpcs, etc.)")]
        // [0,2] should be enough. anything >2s is too laggy anyway.
        [Range(0, 2)]
        [HideInInspector] public float syncInterval = 0.1f;

        /// <summary>
        /// Returns true if this object is active on an active server.
        /// <para>This is only true if the object has been spawned. This is different from NetworkServer.active, which is true if the server itself is active rather than this object being active.</para>
        /// </summary>
        public bool IsServer => Identity.IsServer;

        /// <summary>
        /// Returns true if running as a client and this object was spawned by a server.
        /// </summary>
        public bool IsClient => Identity.IsClient;

        /// <summary>
        /// Returns true if we're on host mode.
        /// </summary>
        public bool IsLocalClient => Identity.IsLocalClient;

        /// <summary>
        /// This returns true if this object is the one that represents the player on the local machine.
        /// <para>In multiplayer games, there are multiple instances of the Player object. The client needs to know which one is for "themselves" so that only that player processes input and potentially has a camera attached. The IsLocalPlayer function will return true only for the player instance that belongs to the player on the local machine, so it can be used to filter out input for non-local players.</para>
        /// </summary>
        public bool IsLocalPlayer => Identity.IsLocalPlayer;

        /// <summary>
        /// True if this object only exists on the server
        /// </summary>
        public bool IsServerOnly => IsServer && !IsClient;

        /// <summary>
        /// True if this object exists on a client that is not also acting as a server
        /// </summary>
        public bool IsClientOnly => IsClient && !IsServer;

        /// <summary>
        /// This returns true if this object is the authoritative version of the object in the distributed network application.
        /// <para>The <see cref="NetworkIdentity.HasAuthority">NetworkIdentity.hasAuthority</see> value on the NetworkIdentity determines how authority is determined. For most objects, authority is held by the server. For objects with <see cref="NetworkIdentity.HasAuthority">NetworkIdentity.hasAuthority</see> set, authority is held by the client of that player.</para>
        /// </summary>
        public bool HasAuthority => Identity.HasAuthority;

        /// <summary>
        /// The unique network Id of this object.
        /// <para>This is assigned at runtime by the network server and will be unique for all objects for that network session.</para>
        /// </summary>
        public uint NetId => Identity.NetId;

        /// <summary>
        /// The <see cref="NetworkServer">NetworkClient</see> associated to this object.
        /// </summary>
        public INetworkServer Server => Identity.Server;

        /// <summary>
        /// Quick Reference to the NetworkIdentities ServerObjectManager. Present only for server/host instances.
        /// </summary>
        public ServerObjectManager ServerObjectManager => Identity.ServerObjectManager;

        /// <summary>
        /// The <see cref="NetworkClient">NetworkClient</see> associated to this object.
        /// </summary>
        public INetworkClient Client => Identity.Client;

        /// <summary>
        /// Quick Reference to the NetworkIdentities ClientObjectManager. Present only for instances instances.
        /// </summary>
        public ClientObjectManager ClientObjectManager => Identity.ClientObjectManager;

        /// <summary>
        /// The <see cref="NetworkPlayer"/> associated with this <see cref="NetworkIdentity" /> This is only valid for player objects on the server.
        /// </summary>
        public INetworkPlayer Owner => Identity.Owner;

        public NetworkWorld World => Identity.World;

        /// <summary>
        /// Returns the appropriate NetworkTime instance based on if this NetworkBehaviour is running as a Server or Client.
        /// </summary>
        public NetworkTime NetworkTime => World.Time;

        protected internal ulong SyncVarDirtyBits { get; private set; }

        private ulong _syncVarHookGuard;

        protected internal bool GetSyncVarHookGuard(ulong dirtyBit)
        {
            return (_syncVarHookGuard & dirtyBit) != 0UL;
        }

        protected internal void SetSyncVarHookGuard(ulong dirtyBit, bool value)
        {
            if (value)
                _syncVarHookGuard |= dirtyBit;
            else
                _syncVarHookGuard &= ~dirtyBit;
        }

        /// <summary>
        /// objects that can synchronize themselves, such as synclists
        /// </summary>
        protected readonly List<ISyncObject> syncObjects = new List<ISyncObject>();

        /// <summary>
        /// NetworkIdentity component caching for easier access
        /// </summary>
        private NetworkIdentity _identity;


        // TODO: remove this bit once Unity drops support for 2019 LTS
#if !UNITY_2020_1_OR_NEWER
        /// <summary>
        /// Cache list for the results of GetComponentsInParent calls when looking up NetworkIdentity for NetworkBehaviour.
        /// Used to avoid runtime allocations.
        /// </summary>
        /// <remarks>
        /// This is required only for pre-2020.1 Unity versions, as they don't have an option to include inactive objects when calling non-allocating GetComponentInParent().
        /// Setting the list's initial size to 1 prevents new allocations because GetComponentsInParent() can never find more than one NetworkIdentity
        /// (given the fact that only 1 NetworkIdentity per hierarchy is currently allowed in Mirage).
        /// </remarks>
        private static List<NetworkIdentity> networkIdentityGetComponentCacheList = new List<NetworkIdentity>(1);
#endif

        /// <summary>
        /// Returns the NetworkIdentity of this object
        /// </summary>
        public NetworkIdentity Identity
        {
            get
            {
                // in this specific case,  we want to know if we have set it before
                // so we can compare if the reference is null
                // instead of calling unity's MonoBehaviour == operator
                if (_identity is null)
                {
                    _identity = TryFindIdentity();

                    // do this 2nd check inside first if so that we are not checking == twice on unity Object
                    if (_identity is null)
                    {
                        throw new InvalidOperationException($"Could not find NetworkIdentity for {name}.");
                    }
                }
                return _identity;
            }
        }

        private int? _componentIndex;
        public const int COMPONENT_INDEX_NOT_FOUND = -1;
        /// <summary>
        /// Returns the index of the component on this object
        /// </summary>
        public int ComponentIndex
        {
            get
            {
                if (_componentIndex.HasValue)
                    return _componentIndex.Value;

                // note: FindIndex causes allocations, we search manually instead
                for (var i = 0; i < Identity.NetworkBehaviours.Length; i++)
                {
                    var component = Identity.NetworkBehaviours[i];
                    if (component == this)
                    {
                        _componentIndex = i;
                        return i;
                    }
                }

                // this should never happen
                logger.LogError("Could not find component in GameObject. You should not add/remove components in networked objects dynamically", this);

                return COMPONENT_INDEX_NOT_FOUND;
            }
        }

        /// <summary>
        /// Tries to find <see cref="NetworkIdentity"/> which is responsible for this <see cref="NetworkBehaviour"/>.
        /// </summary>
        /// <remarks>We look up NetworkIdentity on this GameObject or any of its parents. This allows placing NetworkBehaviours on children of NetworkIdentity.</remarks>
        /// <returns><see cref="NetworkIdentity"/> if found, null otherwise.</returns>
        private NetworkIdentity TryFindIdentity()
        {
#if UNITY_2021_2_OR_NEWER
            var identity = GetComponentInParent<NetworkIdentity>(true);
#elif UNITY_2020_1_OR_NEWER
            var identity = gameObject.GetComponentInParent<NetworkIdentity>(true);
#else
            // TODO: remove this bit once Unity drops support for 2019 LTS
            GetComponentsInParent<NetworkIdentity>(true, networkIdentityGetComponentCacheList);
            // if empty, return null other function will throw
            if (networkIdentityGetComponentCacheList.Count == 0) return null;
            NetworkIdentity identity = networkIdentityGetComponentCacheList[0];
#endif

            return identity;
        }

        // this gets called in the constructor by the weaver
        // for every SyncObject in the component (e.g. SyncLists).
        // We collect all of them and we synchronize them with OnSerialize/OnDeserialize
        protected internal void InitSyncObject(ISyncObject syncObject)
        {
            syncObjects.Add(syncObject);
            syncObject.OnChange += SyncObject_OnChange;
        }

        private void SyncObject_OnChange()
        {
            if (IsServer)
            {
                Server.SyncVarSender.AddDirtyObject(Identity);
            }
        }

        protected internal bool SyncVarEqual<T>(T value, T fieldValue)
        {
            // newly initialized or changed value?
            return EqualityComparer<T>.Default.Equals(value, fieldValue);
        }

        /// <summary>
        /// Used to set the behaviour as dirty, so that a network update will be sent for the object.
        /// these are masks, not bit numbers, ie. 0x004 not 2
        /// </summary>
        /// <param name="dirtyBit">Bit mask to set.</param>
        public void SetDirtyBit(ulong dirtyBit)
        {
            SyncVarDirtyBits |= dirtyBit;
            if (IsServer)
                Server.SyncVarSender.AddDirtyObject(Identity);
        }

        /// <summary>
        /// This clears all the dirty bits that were set on this script by SetDirtyBits();
        /// <para>This is automatically invoked when an update is sent for this object, but can be called manually as well.</para>
        /// </summary>
        public void ClearAllDirtyBits()
        {
            _lastSyncTime = Time.time;
            SyncVarDirtyBits = 0L;

            // flush all unsynchronized changes in syncobjects
            // note: don't use List.ForEach here, this is a hot path
            //   List.ForEach: 432b/frame
            //   for: 231b/frame
            for (var i = 0; i < syncObjects.Count; ++i)
            {
                syncObjects[i].Flush();
            }
        }

        private bool AnySyncObjectDirty()
        {
            // note: don't use Linq here. 1200 networked objects:
            //   Linq: 187KB GC/frame;, 2.66ms time
            //   for: 8KB GC/frame; 1.28ms time
            for (var i = 0; i < syncObjects.Count; ++i)
            {
                if (syncObjects[i].IsDirty)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsDirty()
        {
            if (Time.time - _lastSyncTime >= syncInterval)
            {
                return SyncVarDirtyBits != 0L || AnySyncObjectDirty();
            }
            return false;
        }

        // true if this component has data that has not been
        // synchronized.  Note that it may not synchronize
        // right away because of syncInterval
        public bool StillDirty()
        {
            return SyncVarDirtyBits != 0L || AnySyncObjectDirty();
        }

        /// <summary>
        /// Virtual function to override to send custom serialization data. The corresponding function to send serialization data is OnDeserialize().
        /// </summary>
        /// <remarks>
        /// <para>The initialState flag is useful to differentiate between the first time an object is serialized and when incremental updates can be sent. The first time an object is sent to a client, it must include a full state snapshot, but subsequent updates can save on bandwidth by including only incremental changes. Note that SyncVar hook functions are not called when initialState is true, only for incremental updates.</para>
        /// <para>If a class has SyncVars, then an implementation of this function and OnDeserialize() are added automatically to the class. So a class that has SyncVars cannot also have custom serialization functions.</para>
        /// <para>The OnSerialize function should return true to indicate that an update should be sent. If it returns true, then the dirty bits for that script are set to zero, if it returns false then the dirty bits are not changed. This allows multiple changes to a script to be accumulated over time and sent when the system is ready, instead of every frame.</para>
        /// </remarks>
        /// <param name="writer">Writer to use to write to the stream.</param>
        /// <param name="initialState">If this is being called to send initial state.</param>
        /// <returns>True if data was written.</returns>
        public virtual bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            bool objectWritten;
            // if initialState: write all SyncVars.
            // otherwise write dirtyBits+dirty SyncVars
            if (initialState)
            {
                objectWritten = SerializeObjectsAll(writer);
            }
            else
            {
                objectWritten = SerializeObjectsDelta(writer);
            }

            var syncVarWritten = SerializeSyncVars(writer, initialState);

            return objectWritten || syncVarWritten;
        }


        /// <summary>
        /// Virtual function to override to receive custom serialization data. The corresponding function to send serialization data is OnSerialize().
        /// </summary>
        /// <param name="reader">Reader to read from the stream.</param>
        /// <param name="initialState">True if being sent initial state.</param>
        public virtual void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                DeSerializeObjectsAll(reader);
            }
            else
            {
                DeSerializeObjectsDelta(reader);
            }

            DeserializeSyncVars(reader, initialState);
        }

        // Don't rename. Weaver uses this exact function name.
        public virtual bool SerializeSyncVars(NetworkWriter writer, bool initialState)
        {
            return false;

            // SyncVar are written here in subclass

            // if initialState
            //   write all SyncVars
            // else
            //   write syncVarDirtyBits
            //   write dirty SyncVars
        }

        // Don't rename. Weaver uses this exact function name.
        public virtual void DeserializeSyncVars(NetworkReader reader, bool initialState)
        {
            // SyncVars are read here in subclass

            // if initialState
            //   read all SyncVars
            // else
            //   read syncVarDirtyBits
            //   read dirty SyncVars
        }

        internal ulong DirtyObjectBits()
        {
            ulong dirtyObjects = 0;
            for (var i = 0; i < syncObjects.Count; i++)
            {
                var syncObject = syncObjects[i];
                if (syncObject.IsDirty)
                {
                    dirtyObjects |= 1UL << i;
                }
            }
            return dirtyObjects;
        }

        public bool SerializeObjectsAll(NetworkWriter writer)
        {
            var dirty = false;
            for (var i = 0; i < syncObjects.Count; i++)
            {
                var syncObject = syncObjects[i];
                syncObject.OnSerializeAll(writer);
                dirty = true;
            }
            return dirty;
        }

        public bool SerializeObjectsDelta(NetworkWriter writer)
        {
            var dirty = false;
            // write the mask
            writer.WritePackedUInt64(DirtyObjectBits());
            // serializable objects, such as synclists
            for (var i = 0; i < syncObjects.Count; i++)
            {
                var syncObject = syncObjects[i];
                if (syncObject.IsDirty)
                {
                    syncObject.OnSerializeDelta(writer);
                    dirty = true;
                }
            }
            return dirty;
        }

        internal void DeSerializeObjectsAll(NetworkReader reader)
        {
            for (var i = 0; i < syncObjects.Count; i++)
            {
                var syncObject = syncObjects[i];
                syncObject.OnDeserializeAll(reader);
            }
        }

        internal void DeSerializeObjectsDelta(NetworkReader reader)
        {
            var dirty = reader.ReadPackedUInt64();
            for (var i = 0; i < syncObjects.Count; i++)
            {
                var syncObject = syncObjects[i];
                if ((dirty & (1UL << i)) != 0)
                {
                    syncObject.OnDeserializeDelta(reader);
                }
            }
        }

        internal void ResetSyncObjects()
        {
            foreach (var syncObject in syncObjects)
            {
                syncObject.Reset();
            }
        }

        #region RPC
        // todo move this to NetworkIdentity to optimize (add a registermethod on NB that NI will call)

        // overriden by weaver
        protected internal virtual int GetRpcCount() => 0;

        /// <summary>
        /// Collection that holds information about all RPC in this networkbehaviour (including derived classes)
        /// <para>Can be used to get RPC name from its index</para>
        /// <para>NOTE: Weaver uses this collection to add rpcs, If adding your own rpc do at your own risk</para>
        /// </summary>
        public readonly RemoteCallCollection RemoteCallCollection;

        public NetworkBehaviour()
        {
            RemoteCallCollection = new RemoteCallCollection(this);
        }
        #endregion
    }
}
