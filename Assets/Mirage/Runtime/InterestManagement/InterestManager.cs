using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage.InterestManagement
{
    public class InterestManager
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(InterestManager));

        #region Fields

        public readonly ServerObjectManager ServerObjectManager;
        private List<ObserverData> _visibilitySystems = new List<ObserverData>();
        private readonly List<ObserverData> _observerSystems = new List<ObserverData>();

        #endregion

        #region Properties

        public List<ObserverData> ObserverSystems => _observerSystems;

        #endregion

        #region Callback Listener's

        internal void Update()
        {
            if (_visibilitySystems.Count == 0) return;

            foreach (ObserverData observerData in _visibilitySystems)
            {
                observerData.System.CheckForObservers();
            }
        }

        /// <summary>
        ///     When server stops we will un-register and clean up stuff.
        /// </summary>
        protected virtual void OnServerStopped()
        {
            ServerObjectManager.Server.World.onSpawn -= OnSpawnInWorld;
            ServerObjectManager.Server.Authenticated.RemoveListener(OnAuthenticated);
        }

        /// <summary>
        ///     When server starts up we will register our event listener's.
        /// </summary>
        protected virtual void OnServerStarted()
        {
            ServerObjectManager.Server.World.onSpawn += OnSpawnInWorld;
            ServerObjectManager.Server.Authenticated.AddListener(OnAuthenticated);
        }

        private void OnAuthenticated(INetworkPlayer player)
        {
            if (_visibilitySystems.Count == 0)
            {
                foreach (NetworkIdentity identity in ServerObjectManager.Server.World.SpawnedIdentities)
                {
                    ServerObjectManager.ShowToPlayer(identity, player);
                }
            }
            else
            {
                foreach (ObserverData systemData in _visibilitySystems)
                {
                    systemData.System.OnAuthenticated(player);
                }
            }
        }

        /// <summary>
        ///     Object has spawned in. We should now notify all systems with the intended info so
        ///     each system can do what they need or want with the info.
        /// </summary>
        /// <param name="identity"></param>
        private void OnSpawnInWorld(NetworkIdentity identity)
        {
            if (_visibilitySystems.Count == 0)
            {
                foreach (INetworkPlayer player in ServerObjectManager.Server.Players)
                {
                    ServerObjectManager.ShowToPlayer(identity, player);
                }
            }
            else
            {
                foreach (ObserverData systemData in _visibilitySystems)
                {
                    systemData.System.OnSpawned(identity);
                }
            }
        }

        #endregion

        #region Class Specific

        /// <summary>
        ///     Central system to control and maintain checking for data for all observer visibility systems.
        /// </summary>
        /// <param name="serverObjectManager">The server object manager so we can pull info from it or send info from it.</param>
        /// <param name="initialSystems">The number of initial systems you will be using.</param>
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
            HashSet<INetworkPlayer> observers = Observers(identity);

            if (observers.Count == 0)
                return;

            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                // pack message into byte[] once
                MessagePacker.Pack(msg, writer);
                var segment = writer.ToArraySegment();
                int count = Send(observers, segment, channelId, skip);

                if (count > 0)
                    NetworkDiagnostics.OnSend(msg, segment.Count, count);
            }
        }

        /// <summary>
        /// Send a message to all observers of an identity
        /// </summary>
        /// <remarks>Override if you wish to provide
        /// an allocation free send method</remarks>
        /// <param name="players">The player's we want to send the message to.</param>
        /// <param name="data">the data to send</param>
        /// <param name="channelId">the channel to send it on</param>
        /// <param name="skip">a player who should not receive the message</param>
        /// <returns>Total amounts of messages sent</returns>
        private int Send(HashSet<INetworkPlayer> players, ArraySegment<byte> data, int channelId = Channel.Reliable, INetworkPlayer skip = null)
        {
            int count = 0;

            foreach (INetworkPlayer player in players)
            {
                if (player == null) continue;

                if (player != skip)
                {
                    // send to all connections, but don't wait for them
                    player.Send(data, channelId);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        ///     Register a specific interest management system to the interest manager.
        /// </summary>
        /// <param name="system">The system we want to register in the interest manager.</param>
        internal void RegisterVisibilitySystem(ref ObserverData system)
        {
            if (_visibilitySystems.Contains(system))
            {
                logger.LogWarning(
                    "[InterestManager] - System already register to interest manager. Please check if this was correct.");

                return;
            }

            if(logger.logEnabled)
                logger.Log($"[Interest Manager] - Registering system {system} to our manager.");

            _visibilitySystems.Add(system);
        }

        /// <summary>
        ///     Un-register a specific interest management system from the interest manager.
        /// </summary>
        /// <param name="system">The system we want to un-register from the interest manager.</param>
        internal void UnRegisterVisibilitySystem(ref ObserverData system)
        {
            if(!_visibilitySystems.Contains(system))
            {
                if (logger.logEnabled)
                    logger.Log($"[Interest Manager] - Un-Registering system {system} from our manager.");
                return;
            }

            logger.LogWarning(
                "[InterestManager] - Cannot find system in interest manager. Please check make sure it was registered.");

            _visibilitySystems.Remove(system);
        }

        /// <summary>
        ///     Find out all the players that can see an object
        /// </summary>
        /// <param name="identity">The identity of the object we want to check if player's can see it or not.</param>
        /// <returns></returns>
        internal HashSet<INetworkPlayer> Observers(NetworkIdentity identity)
        {
            if (_visibilitySystems.Count == 0)
                return ServerObjectManager.Server.Players;

            HashSet<INetworkPlayer> observers = new HashSet<INetworkPlayer>();

            return observers;
        }

        #endregion
    }
}
