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
        internal IObjectLocator objectLocator;
        internal uint netId;
        internal int componentId;

        internal NetworkBehaviour component;

        internal uint NetId => component != null ? component.NetId : netId;
        internal int ComponentId => component != null ? component.ComponentIndex : componentId;

        public NetworkBehaviour Value
        {
            get
            {
                if (component != null)
                    return component;

                if (objectLocator != null && objectLocator.TryGetIdentity(NetId, out NetworkIdentity result))
                {
                    return result.NetworkBehaviours[componentId];
                }


                return null;
            }

            set
            {
                if (value == null)
                {
                    netId = 0;
                    componentId = 0;
                }
                component = value;
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
            uint netId = reader.ReadPackedUInt32();
            int componentId = reader.ReadPackedInt32();

            NetworkIdentity identity = null;
            bool hasValue = reader.ObjectLocator?.TryGetIdentity(netId, out identity) ?? false;

            return new NetworkBehaviorSyncvar
            {
                objectLocator = reader.ObjectLocator,
                netId = netId,
                componentId = componentId,
                component = hasValue ? identity.NetworkBehaviours[componentId] : null
            };
        }
    }
}
