using System.Collections.Generic;
using System.Linq;
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
        private readonly HashSet<VisibilitySystem> _visibilitySystems = new HashSet<VisibilitySystem>();
        private HashSet<INetworkPlayer> _observers = new HashSet<INetworkPlayer>();

        private static readonly ProfilerMarker ObserverProfilerMarker = new ProfilerMarker(nameof(Observers));
        private static readonly ProfilerMarker OnAuthenticatedProfilerMarker = new ProfilerMarker(nameof(OnAuthenticated));
        private static readonly ProfilerMarker OnSpawnInWorldProfilerMarker = new ProfilerMarker(nameof(OnSpawnInWorld));
        private static readonly ProfilerMarker OnUpdateProfilerMarker = new ProfilerMarker(nameof(Update));
        private static readonly ProfilerMarker OnSendProfilerMarker = new ProfilerMarker(nameof(Send));

        #endregion

        #region Properties

        public IReadOnlyCollection<VisibilitySystem> ObserverSystems => _visibilitySystems;

        #endregion

        #region Callback Listener's

        /// <summary>
        ///     When server stops we will un-register and clean up stuff.
        /// </summary>
        private void OnServerStopped()
        {
            ServerObjectManager.Server.World.onSpawn -= OnSpawnInWorld;
            ServerObjectManager.Server.Authenticated.RemoveListener(OnAuthenticated);

            _visibilitySystems.Clear();
            _observers.Clear();
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

            bool found = false;

            foreach (VisibilitySystem system in _visibilitySystems)
            {
                found = system.Observers.Any(x => x.Value.Contains(player));

                system.OnAuthenticated(player);
            }

            if (!found)
            {
                foreach (NetworkIdentity identity in ServerObjectManager.Server.World.SpawnedIdentities)
                {
                    ServerObjectManager.ShowToPlayer(identity, player);
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

            bool found = false;

            foreach (VisibilitySystem system in _visibilitySystems)
            {
                if (system.Observers.ContainsKey(identity))
                    found = true;

                system.OnSpawned(identity);
            }

            if (!found)
            {
                foreach (INetworkPlayer player in ServerObjectManager.Server.Players)
                {
                    ServerObjectManager.ShowToPlayer(identity, player);
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
            OnUpdateProfilerMarker.Begin();

            foreach (VisibilitySystem system in _visibilitySystems)
            {
                system.CheckForObservers();
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
        internal bool RegisterSystem(VisibilitySystem system)
        {
            bool wasAdded = _visibilitySystems.Add(system);

            if (wasAdded)
            {
                if (Logger.logEnabled) Logger.Log($"[Interest Manager] - Registering system {system} to our manager.");
            }
            else
            {
                Logger.LogWarning("[InterestManager] - System already register to interest manager. Please check if this was correct.");
            }

            return wasAdded;
        }

        /// <summary>
        ///     Un-register a specific interest management system from the interest manager.
        /// </summary>
        /// <param name="system">The system we want to un-register from the interest manager.</param>
        internal bool UnregisterSystem(VisibilitySystem system)
        {
            bool wasRemoved = _visibilitySystems.Remove(system);

            if (wasRemoved)
            {
                if (Logger.logEnabled) Logger.Log($"[Interest Manager] - Un-Registering system {system} from our manager.");
            }
            else
            {
                Logger.LogWarning("[InterestManager] - Cannot find system in interest manager. Please check make sure it was registered.");
            }

            return wasRemoved;
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

            switch (_visibilitySystems.Count == 0)
            {
                case true:
                    _observers = new HashSet<INetworkPlayer>(ServerObjectManager.Server.Players);
                    break;
                default:
                    int inSystemsCount = 0;

                    foreach (VisibilitySystem system in _visibilitySystems)
                    {
                        if (!system.Observers.ContainsKey(identity)) continue;

                        inSystemsCount++;

                        _observers.UnionWith(system.Observers[identity]);
                    }

                    if (inSystemsCount <= 0)
                    {
                        _observers = new HashSet<INetworkPlayer>(ServerObjectManager.Server.Players);
                    }
                    else
                    {
                        // Multiple systems have been registered. We need to make sure the object is in all system observers
                        // to know that it is actually should be sending data. Always -1 because global is default.
                        if (inSystemsCount != _visibilitySystems.Count)
                            _observers.Clear();
                    }

                    break;
            }

            ObserverProfilerMarker.End();
        }

        #endregion
    }
}
