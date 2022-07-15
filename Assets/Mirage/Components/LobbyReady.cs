using System.Collections.Generic;
using Mirage.Logging;
using UnityEngine;

namespace Mirage
{
    public class LobbyReady : MonoBehaviour
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkServer));

        public List<ObjectReady> ObjectReadyList = new List<ObjectReady>();

        // just a cached memory area where we can collect connections
        // for broadcasting messages
        private static readonly List<INetworkPlayer> playerCache = new List<INetworkPlayer>();

        public void SetAllClientsNotReady()
        {
            foreach (var obj in ObjectReadyList)
            {
                obj.SetClientNotReady();
            }
        }

        public void SendToReady<T>(NetworkIdentity identity, T msg, bool includeOwner = true, int channelId = Channel.Reliable)
        {
            if (logger.LogEnabled()) logger.Log("Server.SendToReady msgType:" + typeof(T));

            playerCache.Clear();

            foreach (var objectReady in ObjectReadyList)
            {
                var isOwner = objectReady.Identity == identity;
                if ((!isOwner || includeOwner) && objectReady.IsReady)
                {
                    playerCache.Add(objectReady.Identity.Owner);
                }
            }

            NetworkServer.SendToMany(playerCache, msg, channelId);
        }
    }
}
