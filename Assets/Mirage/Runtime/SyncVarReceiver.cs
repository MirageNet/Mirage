using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Class that handles syncvar message and passes it to correct <see cref="NetworkIdentity"/>
    /// </summary>
    public class SyncVarReceiver
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(SyncVarReceiver));

        private readonly IObjectLocator objectLocator;

        public SyncVarReceiver(NetworkClient client, IObjectLocator objectLocator)
        {
            this.objectLocator = objectLocator;
            if (client.IsConnected)
            {
                AddHandlers(client);
            }
            else
            {
                // todo replace this with RunOnceEvent
                client.Connected.AddListener(_ => AddHandlers(client));
            }
        }

        private void AddHandlers(NetworkClient client)
        {
            if (client.IsLocalClient)
            {
                client.MessageHandler.RegisterHandler<UpdateVarsMessage>(_ => { });
                client.MessageHandler.RegisterHandler<GroupedSyncVars>(_ => { });
            }
            else
            {
                client.MessageHandler.RegisterHandler<UpdateVarsMessage>(OnUpdateVarsMessage);
                client.MessageHandler.RegisterHandler<GroupedSyncVars>(OnGroupedSyncVars);
            }
        }

        void OnUpdateVarsMessage(UpdateVarsMessage msg)
        {
            if (logger.LogEnabled()) logger.Log("ClientScene.OnUpdateVarsMessage " + msg.netId);

            if (objectLocator.TryGetIdentity(msg.netId, out NetworkIdentity identity))
            {
                using (PooledNetworkReader reader = NetworkReaderPool.GetReader(msg.payload))
                    identity.OnDeserializeAll(reader, false);
            }
            else
            {
                if (logger.WarnEnabled()) logger.LogWarning($"Did not find target for sync message for {msg.netId}. Note: this can be completely normal because UDP messages may arrive out of order, so this message might have arrived after a Destroy message.");
            }
        }

        void OnGroupedSyncVars(GroupedSyncVars msg)
        {
            using (PooledNetworkReader reader = NetworkReaderPool.GetReader(msg.payload))
            {
                uint netId = reader.ReadPackedUInt32();
                if (objectLocator.TryGetIdentity(netId, out NetworkIdentity identity))
                {
                    identity.OnDeserializeAll(reader, false);
                }
                else
                {
                    if (logger.WarnEnabled()) logger.LogWarning($"Did not find target for sync message for {netId}. Note: this can be completely normal because UDP messages may arrive out of order, so this message might have arrived after a Destroy message.");
                }
            }
        }
    }
}
