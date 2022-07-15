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
    }
}
