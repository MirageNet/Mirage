using System.Collections.Generic;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Components
{
    public class LobbyReady : MonoBehaviour
    {
        private static readonly ILogger logger = LogFactory.GetLogger<LobbyReady>();
        private static readonly List<INetworkPlayer> sendCache = new List<INetworkPlayer>();

        public NetworkServer Server;
        public Dictionary<NetworkIdentity, ReadyCheck> Players = new Dictionary<NetworkIdentity, ReadyCheck>();

        private void Start()
        {
            Server.Started.AddListener(OnServerStarted);
        }

        private void OnServerStarted()
        {
            Server.World.AddAndInvokeOnSpawn(OnSpawn);
            Server.World.onUnspawn += OnUnspawn;
        }

        private void OnSpawn(NetworkIdentity obj)
        {
            if (obj.TryGetComponent<ReadyCheck>(out var readyCheck))
            {
                Players.Add(obj, readyCheck);
            }
        }

        private void OnUnspawn(NetworkIdentity obj)
        {
            Players.Remove(obj);
        }

        public void SetAllClientsNotReady()
        {
            foreach (var obj in Players.Values)
            {
                obj.SetReady(false);
            }
        }

        /// <summary>
        /// Send a message to players that are ready on check, or not ready if <paramref name="sendToReady"/> fakse
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <param name="sendToReady">Use to send message no not ready players instead, not this doesn't check server for players with out character, only players with PlayerReadyCheck on their character</param>
        /// <param name="exclude">Add Identity to exclude here, useful when you want to send to all players except the owner</param>
        /// <param name="channelId"></param>
        public void SendToReady<T>(T msg, bool sendToReady = true, NetworkIdentity exclude = null, Channel channelId = Channel.Reliable)
        {
            if (logger.LogEnabled()) logger.Log("LobbyReady.SendToReady msgType:" + typeof(T));

            sendCache.Clear();

            foreach (var kvp in Players)
            {
                var identity = kvp.Key;
                var readyCheck = kvp.Value;

                if (identity == exclude)
                    continue;
                if (identity.Owner == null)
                    continue;

                // check if IsReady is matching sendToReady
                // if both are false we also want to send, eg sending too all not ready
                if (readyCheck.IsReady == sendToReady)
                {
                    sendCache.Add(identity.Owner);
                }
            }

            NetworkServer.SendToMany(sendCache, msg, channelId);
        }
    }
}
