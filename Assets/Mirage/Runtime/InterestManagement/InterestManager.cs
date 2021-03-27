using System;
using System.Collections.Generic;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage.InterestManagement
{
    /// <summary>
    /// An interest manager is responsible for showing and hiding objects to players
    /// based on an area if interest policy.
    ///
    /// Implement this class to provide a interest management policy and assign
    /// it to the <see cref="ServerObjectManager"/>
    /// </summary>
    public abstract class InterestManager : MonoBehaviour
    {
        public ServerObjectManager ServerObjectManager;

        public void Start()
        {
            if (ServerObjectManager == null)
                ServerObjectManager = GetComponent<ServerObjectManager>();

            ServerObjectManager.Spawned.AddListener(OnSpawned);

            NetworkServer server = ServerObjectManager.Server;
            if (server == null)
                server = GetComponent<NetworkServer>();

            server.Authenticated.AddListener(OnAuthenticated);
        }

        /// <summary>
        /// Invoked when a player joins the server
        /// It should show all objects relevant to that player
        /// </summary>
        /// <param name="identity"></param>
        protected abstract void OnAuthenticated(INetworkPlayer player);

        /// <summary>
        /// Invoked when an object is spawned in the server
        /// It should show that object to all relevant players
        /// </summary>
        /// <param name="identity">The object just spawned</param>
        protected abstract void OnSpawned(NetworkIdentity identity);

        /// <summary>
        /// Find out all the players that can see an object
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public abstract IReadOnlyCollection<INetworkPlayer> Observers(NetworkIdentity identity);


        /// <summary>
        /// Send a message to all observers of an identity
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="msg"></param>
        /// <param name="channelId"></param>
        protected internal virtual void Send<T>(NetworkIdentity identity, T msg, int channelId = Channel.Reliable, INetworkPlayer skip = null) 
        {
            IReadOnlyCollection<INetworkPlayer> observers = Observers(identity);

            if (observers.Count == 0)
                return;

            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                // pack message into byte[] once
                MessagePacker.Pack(msg, writer);
                var segment = writer.ToArraySegment();
                int count = Send(identity, segment, channelId, skip);

                if (count > 0)
                    NetworkDiagnostics.OnSend(msg, channelId, segment.Count, count);
            }
        }

        /// <summary>
        /// Send a message to all observers of an identity
        /// </summary>
        /// <remarks>Override if you wish to provide
        /// an allocation free send method</remarks>
        /// <param name="identity">the object that wants to send a message</param>
        /// <param name="data">the data to send</param>
        /// <param name="channelId">the channel to send it on</param>
        /// <param name="skip">a player who should not receive the message</param>
        /// <returns>Total amounts of messages sent</returns>
        protected virtual int Send(NetworkIdentity identity, ArraySegment<byte> data, int channelId = Channel.Reliable, INetworkPlayer skip = null)
        {
            int count = 0;

            foreach (INetworkPlayer player in Observers(identity))
            {
                if (player != skip)
                {
                    // send to all connections, but don't wait for them
                    player.Send(data, channelId);
                    count++;
                }
            }
            return count;
        }
    }
}
