using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Mirage.InterestManagement
{
    public class InterestManager
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(InterestManager));

        #region Fields

        protected internal readonly ServerObjectManager ServerObjectManager;
        private ObserverData[] _visibilitySystems;
        private readonly int _initialSystems;
        private List<ObserverData> _observerSystems = new List<ObserverData>();

        #endregion

        #region Properties

        public List<ObserverData> ObserverSystems => _observerSystems;

        #endregion

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
            if (_visibilitySystems == null)
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
        public InterestManager(ServerObjectManager serverObjectManager, int initialSystems = 0)
        {
            _initialSystems = initialSystems;

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
            var stopWatch = Stopwatch.StartNew();

            HashSet<INetworkPlayer> observers = Observers(identity);

            if (observers.Count == 0)
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

            Debug.Log($"[Interest Manager] - Send all observers Method Execution Time: {stopWatch.Elapsed.TotalMilliseconds} ms");
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckGrowVisibilitySystem()
        {
            var stopWatch = Stopwatch.StartNew();

            if (_visibilitySystems == null)
            {
                _visibilitySystems = new ObserverData[_initialSystems];
            }
            else if (_visibilitySystems[_visibilitySystems.Length - 1].System == default)
            {
                // We need to copy and expand our visibility system.
                if (logger.logEnabled)
                    logger.Log("[Interest Manager] - Visibility system is expanding array. If this is happening often. Please set initial systems in constructor.");

                var newData = new ObserverData[_initialSystems + _initialSystems];

                _visibilitySystems.CopyTo(newData, 0);

                _visibilitySystems = newData;
            }

            stopWatch.Stop();

            Debug.Log($"[Interest Manager] - Check Grow Visibility Method Execution Time: {stopWatch.Elapsed.TotalMilliseconds} ms");
        }

        /// <summary>
        ///     Register a specific interest management system to the interest manager.
        /// </summary>
        /// <param name="system">The system we want to register in the interest manager.</param>
        internal void RegisterVisibilitySystem(ref ObserverData system)
        {
            CheckGrowVisibilitySystem();

            var stopWatch = Stopwatch.StartNew();

            if (_visibilitySystems.Equals(system))
            {
                logger.LogWarning(
                    "[InterestManager] - System already register to interest manager. Please check if this was correct.");

                return;
            }

            if(logger.logEnabled)
                logger.Log($"[Interest Manager] - Registering system {system} to our manager.");

            for (int i = 0; i < _visibilitySystems.Length; i++)
            {
                if(!_visibilitySystems[i].Equals(default)) continue;

                _visibilitySystems[i] = system;

                stopWatch.Stop();

                Debug.Log($"[Interest Manager] - Registering Method Execution Time: {stopWatch.Elapsed.TotalMilliseconds} ms");

                return;
            }

            stopWatch.Stop();

            Debug.Log($"[Interest Manager] - Registering Method Execution Time: {stopWatch.Elapsed.TotalMilliseconds} ms");
        }

        /// <summary>
        ///     Un-register a specific interest management system from the interest manager.
        /// </summary>
        /// <param name="system">The system we want to un-register from the interest manager.</param>
        internal void UnRegisterVisibilitySystem(ref ObserverData system)
        {
            var stopWatch = Stopwatch.StartNew();

            for (int i = 0; i < _visibilitySystems.Length; i++)
            {
                if (!_visibilitySystems[i].Equals(system)) continue;

                _visibilitySystems[i] = default;

                if (logger.logEnabled)
                    logger.Log($"[Interest Manager] - Un-Registering system {system} from our manager.");

                stopWatch.Stop();

                Debug.Log($"[Interest Manager] - Un-Registering  Method Execution Time: {stopWatch.Elapsed.TotalMilliseconds} ms");

                return;
            }

            logger.LogWarning(
                "[InterestManager] - Cannot find system in interest manager. Please check make sure it was registered.");

            stopWatch.Stop();

            Debug.Log($"[Interest Manager] - Un-Registering Method Execution Time: {stopWatch.Elapsed.TotalMilliseconds} ms");
        }

        /// <summary>
        ///     Find out all the players that can see an object
        /// </summary>
        /// <param name="identity">The identity of the object we want to check if player's can see it or not.</param>
        /// <returns></returns>
        internal HashSet<INetworkPlayer> Observers(NetworkIdentity identity)
        {
            var stopWatch = Stopwatch.StartNew();

            if (_visibilitySystems == null)
                return ServerObjectManager.Server.Players;

            var observers = new HashSet<INetworkPlayer>();

            stopWatch.Stop();

            Debug.Log($"[Interest Manager] - Observers Method Execution Time: {stopWatch.Elapsed.TotalMilliseconds} ms");

            return observers;
        }

        #endregion
    }
}
