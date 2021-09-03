using System;
using System.Collections.Generic;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage.InterestManagement
{
    /// <summary>
    ///     Interest Manager will manage sending correct data to all clients. Based on 1 or many different interest systems.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class InterestManager
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(InterestManager));

        public readonly ServerObjectManager ServerObjectManager;
        private readonly List<INetworkVisibility> _visibilitySystems = new List<INetworkVisibility>();

        #region Callback Listener's

        /// <summary>
        ///     When server stops we will un-register and clean up stuff.
        /// </summary>
        protected virtual void OnServerStopped()
        {
            ServerObjectManager.Server.World.onSpawn -= OnSpawnInWorld;
        }

        /// <summary>
        ///     When server starts up we will register our event listener's.
        /// </summary>
        protected virtual void OnServerStarted()
        {
            ServerObjectManager.Server.World.onSpawn += OnSpawnInWorld;
        }

        /// <summary>
        ///     Object has spawned in. We should now notify all systems with the intended info so
        ///     each system can do what they need or want with the info.
        /// </summary>
        /// <param name="identity"></param>
        private void OnSpawnInWorld(NetworkIdentity identity)
        {
            if(_visibilitySystems.Count == 0)
            {
                foreach (INetworkPlayer player in ServerObjectManager.Server.Players)
                {
                    ServerObjectManager.ShowForConnection(identity, player);
                }
            }
            else
            {
                foreach (INetworkVisibility system in _visibilitySystems)
                {
                    system.OnSpawned(identity);
                }
            }
        }

        #endregion

        #region Class Specific

        public InterestManager(ServerObjectManager serverObjectManager)
        {
            ServerObjectManager = serverObjectManager;

            ServerObjectManager.Server.Started.AddListener(OnServerStarted);
            ServerObjectManager.Server.Stopped.AddListener(OnServerStopped);
        }

        /// <summary>
        /// Send a message to all observers of an identity
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="msg"></param>
        /// <param name="channelId"></param>
        protected internal virtual void Send<T>(NetworkIdentity identity, T msg, int channelId = Channel.Reliable, INetworkPlayer skip = null)
        {
            IReadOnlyCollection<INetworkPlayer>[] observers = Observers(identity);

            if (observers.Length == 0)
                return;

            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                // pack message into byte[] once
                MessagePacker.Pack(msg, writer);
                var segment = writer.ToArraySegment();
                int count = Send(identity, segment, channelId, skip);

                if (count > 0)
                    NetworkDiagnostics.OnSend(msg, segment.Count, count);
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

            foreach (IReadOnlyCollection<INetworkPlayer> observer in Observers(identity))
            {
                if(observer == null) continue;

                foreach (INetworkPlayer player in observer)
                {
                    if (player != skip)
                    {
                        // send to all connections, but don't wait for them
                        player.Send(data, channelId);
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        ///     Register a specific interest management system to the interest manager.
        /// </summary>
        /// <param name="system">The system we want to register in the interest manager.</param>
        internal void RegisterVisibilitySystem(INetworkVisibility system)
        {
            if (_visibilitySystems.Contains(system))
                logger.LogWarning(
                    "[InterestManager] - System already register to interest manager. Please check if this was correct.");

            _visibilitySystems.Add(system);
        }

        /// <summary>
        ///     Un-register a specific interest management system from the interest manager.
        /// </summary>
        /// <param name="system">The system we want to un-register from the interest manager.</param>
        internal void UnRegisterVisibilitySystem(INetworkVisibility system)
        {
            if (!_visibilitySystems.Remove(system))
                logger.LogWarning(
                    "[InterestManager] - Cannot find system in interest manager. Please check make sure it was registered.");
        }

        /// <summary>
        ///     Find out all the players that can see an object
        /// </summary>
        /// <param name="identity">The identity of the object we want to check if player's can see it or not.</param>
        /// <returns></returns>
        internal IReadOnlyCollection<INetworkPlayer>[] Observers(NetworkIdentity identity)
        {
            var observers = new IReadOnlyCollection<INetworkPlayer>[_visibilitySystems.Count];

            for (int i = 0; i < _visibilitySystems.Count; i++)
            {
                _visibilitySystems[i].CheckForObservers(identity, identity.transform.localPosition, out HashSet<INetworkPlayer> players);

                if(players.Count > 0)
                {
                    identity.VisibilitySystems.Add(_visibilitySystems[i]);
                    observers[i] = players;
                }
                else if (identity.VisibilitySystems.Contains(_visibilitySystems[i]))
                    identity.VisibilitySystems.Remove(_visibilitySystems[i]);
            }

            return observers;
        }

        #endregion
    }
}
