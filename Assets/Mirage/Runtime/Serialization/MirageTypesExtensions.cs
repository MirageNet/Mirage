using System;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Serialization
{
    public static class MirageTypesExtensions
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(MirageTypesExtensions));

        public static void WriteNetworkIdentity(this NetworkWriter writer, NetworkIdentity value)
        {
            if (value == null)
            {
                writer.WritePackedUInt32(0);
                return;
            }
            writer.WritePackedUInt32(value.NetId);
        }
        public static void WriteNetworkBehaviour(this NetworkWriter writer, NetworkBehaviour value)
        {
            if (value == null)
            {
                writer.WriteNetworkIdentity(null);
                return;
            }
            writer.WriteNetworkIdentity(value.Identity);
            writer.WriteByte((byte)value.ComponentIndex);
        }
        public static void WriteGameObject(this NetworkWriter writer, GameObject value)
        {
            if (value == null)
            {
                writer.WriteNetworkIdentity(null);
                return;
            }
            var identity = value.GetComponent<NetworkIdentity>();
            if (identity == null)
                throw new InvalidOperationException($"Cannot send GameObject without a NetworkIdentity {value.name}");
            writer.WriteNetworkIdentity(identity);
        }

        /// <summary>
        /// Casts reader to <see cref="MirageNetworkReader"/>, throw if cast is invalid
        /// </summary>
        /// <param name=""></param>
        public static MirageNetworkReader ToMirageReader(this NetworkReader reader)
        {
            if (reader is MirageNetworkReader mirageReader)
                return mirageReader;
            else
                throw new InvalidOperationException("Reader is not MirageNetworkReader");
        }

        public static NetworkIdentity ReadNetworkIdentity(this NetworkReader reader)
        {
            var mirageReader = reader.ToMirageReader();

            var netId = reader.ReadPackedUInt32();
            if (netId == 0)
                return null;

            return FindNetworkIdentity(mirageReader.ObjectLocator, netId);
        }

        private static NetworkIdentity FindNetworkIdentity(IObjectLocator objectLocator, uint netId)
        {
            if (objectLocator == null)
            {
                if (logger.WarnEnabled()) logger.LogWarning($"Could not find NetworkIdentity because ObjectLocator is null");
                return null;
            }

            // if not found return c# null
            return objectLocator.TryGetIdentity(netId, out var identity)
                ? identity
                : null;
        }

        public static NetworkBehaviour ReadNetworkBehaviour(this NetworkReader reader)
        {
            var mirageReader = reader.ToMirageReader();

            // we can't use ReadNetworkIdentity here, because we need to know if netid was 0 or not
            // if it is not 0 we need to read component index even if NI is null, or it'll fail to deserilize next part
            var netId = reader.ReadPackedUInt32();
            if (netId == 0)
                return null;

            // always read index if netid is not 0
            var componentIndex = reader.ReadByte();

            var identity = FindNetworkIdentity(mirageReader.ObjectLocator, netId);
            if (identity is null)
                return null;

            return identity.NetworkBehaviours[componentIndex];
        }

        public static T ReadNetworkBehaviour<T>(this NetworkReader reader) where T : NetworkBehaviour
        {
            return reader.ReadNetworkBehaviour() as T;
        }

        public static GameObject ReadGameObject(this NetworkReader reader)
        {
            var identity = reader.ReadNetworkIdentity();
            if (identity == null)
            {
                return null;
            }
            return identity.gameObject;
        }
    }
}
