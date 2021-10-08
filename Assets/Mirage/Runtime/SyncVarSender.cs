using System;
using System.Collections.Generic;
using System.ComponentModel;
using Mirage.Serialization;

namespace Mirage
{
    public enum SyncVarSendMode
    {
        /// <summary>
        /// SyncVar system that has been in Mirror and Mirage for a long time. Should work in call cases but might not be most efficient
        /// </summary>
        Legacy = 1,
        /// <summary>
        /// Groups Objects changes together before sending, reducing overall bandwidth because message headers are sent
        /// </summary>
        Grouped = 2,
        // DeltaSnapshot_Experimental = 3,
        // EventualConsistency_Experimental = 4,
    }

    /// <summary>
    /// Class that Syncs syncvar and other <see cref="NetworkIdentity"/> State
    /// </summary>
    public abstract class SyncVarSenderBase
    {
        protected readonly HashSet<NetworkIdentity> DirtyObjects = new HashSet<NetworkIdentity>();
        protected readonly List<NetworkIdentity> DirtyObjectsTmp = new List<NetworkIdentity>();

        public void AddDirtyObject(NetworkIdentity dirty)
        {
            DirtyObjects.Add(dirty);
        }

        internal abstract void Update();

        internal static SyncVarSenderBase Create(SyncVarSendMode mode, NetworkServer server, int maxMessageBytes)
        {
            switch (mode)
            {
                case SyncVarSendMode.Legacy:
                    return new SyncVarSender_Legacy();
                case SyncVarSendMode.Grouped:
                    return new SyncVarSender_Grouped(server, maxMessageBytes);
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        public sealed class SyncVarSender_Legacy : SyncVarSenderBase
        {
            internal override void Update()
            {
                DirtyObjectsTmp.Clear();

                foreach (NetworkIdentity identity in DirtyObjects)
                {
                    if (identity != null)
                    {
                        identity.UpdateVars_Legacy();

                        if (identity.AnyBehaviourDirty())
                            DirtyObjectsTmp.Add(identity);
                    }
                }

                DirtyObjects.Clear();

                foreach (NetworkIdentity obj in DirtyObjectsTmp)
                    DirtyObjects.Add(obj);
            }
        }

        public sealed class SyncVarSender_Grouped : SyncVarSenderBase
        {
            private readonly NetworkServer server;
            private readonly int maxMessageBitLength;
            /// <summary>
            /// net id is var int, so could be 5 bytes long
            /// </summary>
            const int MAX_NET_ID_SIZE = 5 * 8;

            /// <summary>
            /// 2 bytes for message hash, 2 bytes for segment length
            /// <para>note: segment legnth uses var-int, so might only be sent as 1 byte</para>
            /// </summary>
            const int MESSAGE_HEADER_SIZE = 4;

            public SyncVarSender_Grouped(NetworkServer server, int maxMessageBytes)
            {
                this.server = server;
                // 
                maxMessageBitLength = (maxMessageBytes - MESSAGE_HEADER_SIZE) * 8;
            }

            internal override void Update()
            {
                DirtyObjectsTmp.Clear();
                // copy to temp to make sure there are no null objects
                foreach (NetworkIdentity identity in DirtyObjects)
                {
                    if (identity != null)
                    {
                        DirtyObjectsTmp.Add(identity);
                    }
                }


                using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
                {
                    foreach (INetworkPlayer player in server.Players)
                    {
                        // skip if scene not ready
                        if (!player.SceneIsReady) continue;

                        SendToPlayer(player, writer);
                    }
                }

                DirtyObjects.Clear();
                foreach (NetworkIdentity identity in DirtyObjectsTmp)
                {
                    identity.ClearCachedUpdate();

                    if (identity.AnyBehaviourDirty())
                        DirtyObjectsTmp.Add(identity);
                }
            }

            private void SendToPlayer(INetworkPlayer player, PooledNetworkWriter writer)
            {
                foreach (NetworkIdentity identity in DirtyObjectsTmp)
                {
                    // skip if not visibile
                    if (!player.IsVisible(identity))
                        continue;

                    NetworkWriter cachedWriter = identity.GetCachedUpdate(player);

                    // there might be nothing to write, in this case we can skip 
                    if (cachedWriter == null)
                        continue;

                    ThrowIfOverMaxSize(identity, cachedWriter);

                    if (TooBig(writer, cachedWriter))
                    {
                        SendGroupSyncVarMessage(player, writer);
                    }

                    writer.WritePackedUInt32(identity.NetId);
                    writer.CopyFromWriter(cachedWriter, 0, cachedWriter.BitPosition);
                }


                if (writer.BitPosition > 0)
                {
                    SendGroupSyncVarMessage(player, writer);
                }
            }

            private void ThrowIfOverMaxSize(NetworkIdentity identity, NetworkWriter cachedWriter)
            {
                if (cachedWriter.BitPosition + MAX_NET_ID_SIZE > maxMessageBitLength)
                {
                    throw new InvalidOperationException($"Message size for '{identity.name}' was over max ({maxMessageBitLength / 8} bytes). Size was {cachedWriter.BitPosition / 8} bytes");
                }
            }

            private bool TooBig(PooledNetworkWriter writer, NetworkWriter cachedWriter)
            {
                return writer.BitPosition + cachedWriter.BitPosition > maxMessageBitLength;
            }

            private void SendGroupSyncVarMessage(INetworkPlayer player, PooledNetworkWriter writer)
            {
                player.Send(new GroupedSyncVars { payload = writer.ToArraySegment() });
                writer.Reset();
            }
        }

    }
    [NetworkMessage]
    public struct GroupedSyncVars
    {
        public ArraySegment<byte> payload;
    }
}
