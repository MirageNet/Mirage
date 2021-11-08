using System.Collections.Generic;
using Mirage.Logging;
using Unity.Profiling;
using UnityEngine;

namespace Mirage.InterestManagement
{
    public class InterestManager
    {
        static readonly ILogger Logger = LogFactory.GetLogger(typeof(InterestManager));

        #region Fields

        public readonly ServerObjectManager ServerObjectManager;
        private readonly List<ObserverData> _visibilitySystems = new List<ObserverData>();
        private List<INetworkPlayer> _observers = new List<INetworkPlayer>();

        private static readonly ProfilerMarker ObserverProfilerMarker = new ProfilerMarker(nameof(Observers));
        private static readonly ProfilerMarker OnAuthenticatedProfilerMarker = new ProfilerMarker(nameof(OnAuthenticated));
        private static readonly ProfilerMarker OnSpawnInWorldProfilerMarker = new ProfilerMarker(nameof(OnSpawnInWorld));
        private static readonly ProfilerMarker OnUpdateProfilerMarker = new ProfilerMarker(nameof(Update));
        private static readonly ProfilerMarker OnSendProfilerMarker = new ProfilerMarker(nameof(Send));

        #endregion

        #region Properties

        public IReadOnlyCollection<ObserverData> ObserverSystems => _visibilitySystems;

        #endregion

        #region Callback Listener's

        /// <summary>
        ///     When server stops we will un-register and clean up stuff.
        /// </summary>
        private void OnServerStopped()
        {
            ServerObjectManager.Server.World.onSpawn -= OnSpawnInWorld;
            ServerObjectManager.Server.Authenticated.RemoveListener(OnAuthenticated);
        }

        /// <summary>
        ///     When server starts up we will register our event listener's.
        /// </summary>
        private void OnServerStarted()
        {
            ServerObjectManager.Server.World.onSpawn += OnSpawnInWorld;
            ServerObjectManager.Server.Authenticated.AddListener(OnAuthenticated);
        }

        /// <summary>
        ///     When player's finally authenticate to server we will check for visibility systems
        ///     and if any we will use that otherwise we will default to global system.
        /// </summary>
        /// <param name="player">The player we want to show or hide objects to.</param>
        private void OnAuthenticated(INetworkPlayer player)
        {
            OnAuthenticatedProfilerMarker.Begin();

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

            OnAuthenticatedProfilerMarker.End();
        }

        /// <summary>
        ///     Object has spawned in. We should now notify all systems with the intended info so
        ///     each system can do what they need or want with the info.
        /// </summary>
        /// <param name="identity">The newly spawned object.</param>
        private void OnSpawnInWorld(NetworkIdentity identity)
        {
            OnSpawnInWorldProfilerMarker.Begin();

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

            OnSpawnInWorldProfilerMarker.End();
        }

        #endregion

        #region Class Specific

        /// <summary>
        ///     Central system to control and maintain checking for data for all observer visibility systems.
        /// </summary>
        /// <param name="serverObjectManager">The server object manager so we can pull info from it or send info from it.</param>
        public InterestManager(ServerObjectManager serverObjectManager)
        {
            ServerObjectManager = serverObjectManager;

            ServerObjectManager.Server?.Started.AddListener(OnServerStarted);
            ServerObjectManager.Server?.Stopped.AddListener(OnServerStopped);
        }

        internal void Update()
        {
            if (_visibilitySystems.Count == 0) return;

            OnUpdateProfilerMarker.Begin();

            foreach (ObserverData observerData in _visibilitySystems)
            {
                observerData.System.CheckForObservers();
            }

            OnUpdateProfilerMarker.End();
        }

        /// <summary>
        /// Send a message to all observers of an identity
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="msg"></param>
        /// <param name="channelId"></param>
        /// <param name="skip"></param>
        protected internal void Send<T>(NetworkIdentity identity, T msg, int channelId = Channel.Reliable, INetworkPlayer skip = null)
        {
            OnSendProfilerMarker.Begin();

            Observers(identity);

            // remove skipped player. No need to send to them.
            _observers.Remove(skip);

            if (_observers.Count == 0)
            {
                OnSendProfilerMarker.End();

                return;
            }

            NetworkServer.SendToMany(_observers, msg, channelId);

            OnSendProfilerMarker.End();
        }

        /// <summary>
        ///     Register a specific interest management system to the interest manager.
        /// </summary>
        /// <param name="system">The system we want to register in the interest manager.</param>
        internal void RegisterVisibilitySystem(ref ObserverData system)
        {
            if (_visibilitySystems.Contains(system))
            {
                Logger.LogWarning(
                    "[InterestManager] - System already register to interest manager. Please check if this was correct.");

                return;
            }

            if (Logger.logEnabled)
                Logger.Log($"[Interest Manager] - Registering system {system} to our manager.");

            _visibilitySystems.Add(system);
        }

        /// <summary>
        ///     Un-register a specific interest management system from the interest manager.
        /// </summary>
        /// <param name="system">The system we want to un-register from the interest manager.</param>
        internal void UnRegisterVisibilitySystem(ref ObserverData system)
        {
            if (!_visibilitySystems.Contains(system))
            {
                if (Logger.logEnabled)
                    Logger.Log($"[Interest Manager] - Un-Registering system {system} from our manager.");
                return;
            }

            Logger.LogWarning(
                "[InterestManager] - Cannot find system in interest manager. Please check make sure it was registered.");

            _visibilitySystems.Remove(system);
        }


        /// <summary>
        ///     Find out all the players that can see an object
        /// </summary>
        /// <param name="identity">The identity of the object we want to check if player's can see it or not.</param>
        /// <returns></returns>
        private void Observers(NetworkIdentity identity)
        {
            ObserverProfilerMarker.Begin();

            _observers.Clear();

            if (_visibilitySystems.Count == 0)
            {
                ObserverProfilerMarker.End();

                _observers.AddRange(ServerObjectManager.Server.Players);
            }

            foreach (ObserverData visibilitySystem in _visibilitySystems)
            {
                if (!visibilitySystem.Observers.ContainsKey(identity)) continue;

                foreach (KeyValuePair<NetworkIdentity, HashSet<INetworkPlayer>> observer in visibilitySystem.Observers)
                {
                    _observers.AddRange(observer.Value);
                }
            }

            ObserverProfilerMarker.End();
        }

        #endregion
    }
}
