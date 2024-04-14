using System.Runtime.CompilerServices;
using Mirage.Serialization;

namespace Mirage
{

    /// <summary>
    /// backing struct for a NetworkIdentity when used as a syncvar
    /// the weaver will replace the syncvar with this struct.
    /// </summary>
    public struct NetworkBehaviorSyncvar
    {
        /// <summary>
        /// The network client that spawned the parent object
        /// used to lookup the identity if it exists
        /// </summary>
        internal IObjectLocator _objectLocator;
        internal uint _netId;
        internal int _componentId;

        internal NetworkBehaviour _component;

        public NetworkBehaviorSyncvar(NetworkBehaviour behaviour) : this()
        {
            _component = behaviour;
        }

        internal uint NetId => _component != null ? _component.NetId : _netId;
        internal int ComponentId => _component != null ? _component.ComponentIndex : _componentId;

        public NetworkBehaviour Value
        {
            get
            {
                if (_component != null)
                    return _component;

                if (_objectLocator != null && _objectLocator.TryGetIdentity(NetId, out var result))
                {
                    return result.NetworkBehaviours[_componentId];
                }


                return null;
            }

            set
            {
                if (value == null)
                {
                    _netId = 0;
                    _componentId = 0;
                }
                _component = value;
            }
        }

        /// <summary>
        /// returns Value cast as T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="System.InvalidCastException"></exception>
        /// /// <returns></returns>
        public T GetAs<T>() where T : NetworkBehaviour
        {
            var value = Value;
            if (value is null)
                return null;
            else
                return (T)value;
        }

        public static implicit operator NetworkBehaviorSyncvar(NetworkBehaviour behaviour) => new NetworkBehaviorSyncvar(behaviour);
    }

    public struct NetworkBehaviorSyncvar<T> where T : NetworkBehaviour
    {
        private NetworkBehaviorSyncvar inner;

        public NetworkBehaviorSyncvar(T behaviour) : this()
        {
            inner = new NetworkBehaviorSyncvar(behaviour);
        }

        internal uint NetId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => inner.NetId;
        }

        internal int ComponentId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => inner.ComponentId;
        }

        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T)inner.Value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => inner.Value = value;
        }

        public static implicit operator NetworkBehaviorSyncvar<T>(T behaviour) => new NetworkBehaviorSyncvar<T>(behaviour);
        public static implicit operator NetworkBehaviorSyncvar(NetworkBehaviorSyncvar<T> generic) => generic.inner;
        public static explicit operator NetworkBehaviorSyncvar<T>(NetworkBehaviorSyncvar syncvar) => new NetworkBehaviorSyncvar<T>() { inner = syncvar };
    }


    public static class NetworkBehaviorSerializers
    {
        public static void WriteNetworkBehaviorSyncVar(this NetworkWriter writer, NetworkBehaviorSyncvar id)
        {
            writer.WritePackedUInt32(id.NetId);
            writer.WritePackedInt32(id.ComponentId);
        }

        public static NetworkBehaviorSyncvar ReadNetworkBehaviourSyncVar(this NetworkReader reader)
        {
            var mirageReader = reader.ToMirageReader();

            var netId = reader.ReadPackedUInt32();
            var componentId = reader.ReadPackedInt32();

            NetworkIdentity identity = null;
            var hasValue = mirageReader.ObjectLocator?.TryGetIdentity(netId, out identity) ?? false;

            return new NetworkBehaviorSyncvar
            {
                _objectLocator = mirageReader.ObjectLocator,
                _netId = netId,
                _componentId = componentId,
                _component = hasValue ? identity.NetworkBehaviours[componentId] : null
            };
        }

        [WeaverSerializeCollection]
        public static void WriteGenericNetworkBehaviorSyncVar<T>(this NetworkWriter writer, NetworkBehaviorSyncvar<T> id) where T : NetworkBehaviour
        {
            WriteNetworkBehaviorSyncVar(writer, id);
        }

        [WeaverSerializeCollection]
        public static NetworkBehaviorSyncvar<T> ReadGenericNetworkBehaviourSyncVar<T>(this NetworkReader reader) where T : NetworkBehaviour
        {
            var syncvar = ReadNetworkBehaviourSyncVar(reader);
            return (NetworkBehaviorSyncvar<T>)syncvar;
        }
    }
}
