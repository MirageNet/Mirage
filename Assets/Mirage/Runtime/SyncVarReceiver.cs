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
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(SyncVarReceiver));

        private readonly IObjectLocator _objectLocator;

        public SyncVarReceiver(NetworkClient client, IObjectLocator objectLocator)
        {
            _objectLocator = objectLocator;
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
            }
            else
            {
                client.MessageHandler.RegisterHandler<UpdateVarsMessage>(OnUpdateVarsMessage);
            }
        }

        private void OnUpdateVarsMessage(UpdateVarsMessage msg)
        {
            if (logger.LogEnabled()) logger.Log("ClientScene.OnUpdateVarsMessage " + msg.netId);

            if (_objectLocator.TryGetIdentity(msg.netId, out var localObject))
            {
                using (var networkReader = NetworkReaderPool.GetReader(msg.payload, _objectLocator))
                    localObject.OnDeserializeAll(networkReader, false);
            }
            else
            {
                if (logger.WarnEnabled()) logger.LogWarning("Did not find target for sync message for " + msg.netId + " . Note: this can be completely normal because UDP messages may arrive out of order, so this message might have arrived after a Destroy message.");
            }
        }
    }
}
