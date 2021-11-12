using System.Collections.Generic;
using Mirage.Logging;
using Unity.Profiling;
using UnityEngine;

namespace Mirage.InterestManagement
{
    public class InterestManager
    {
        private class SystemComparer : IEqualityComparer<ObserverData>
        {
            public bool Equals(ObserverData x, ObserverData y)
            {
                return x.System.GetType().Name.GetStableHashCode() == y.System.GetType().Name.GetStableHashCode();
            }

            public int GetHashCode(ObserverData obj)
            {
                int hash = obj.System.GetType().Name.GetStableHashCode();

                return hash;
            }
        }

        static readonly ILogger Logger = LogFactory.GetLogger(typeof(InterestManager));

        #region Fields

        public readonly ServerObjectManager ServerObjectManager;
        private readonly HashSet<ObserverData> _visibilitySystems = new HashSet<ObserverData>(new SystemComparer());
        private HashSet<INetworkPlayer> _observers = new HashSet<INetworkPlayer>();

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
                foreach (ObserverData observer in _visibilitySystems)
                {
                    observer.System.OnAuthenticated(player);
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
                foreach (ObserverData observer in _visibilitySystems)
                {
                    observer.System.OnSpawned(identity);
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

        /// <summary>
        ///     Check to see if certain system has already been registered.
        /// </summary>
        /// <returns>Returns true if we have already registered the system.</returns>
        public bool IsRegisteredAlready(ref ObserverData observer)
        {
            return _visibilitySystems.Contains(observer);
        }

        internal void Update()
        {
            if (_visibilitySystems.Count == 0) return;

            OnUpdateProfilerMarker.Begin();

            foreach (ObserverData observer in _visibilitySystems)
            {
                observer.System.CheckForObservers();
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
        /// <param name="observer">The system we want to register in the interest manager.</param>
        internal void RegisterVisibilitySystem(ref ObserverData observer)
        {
            if (_visibilitySystems.Contains(observer))
            {
                Logger.LogWarning(
                    "[InterestManager] - System already register to interest manager. Please check if this was correct.");

                return;
            }

            if (Logger.logEnabled)
                Logger.Log($"[Interest Manager] - Registering system {observer} to our manager.");

            _visibilitySystems.Add(observer);
        }

        /// <summary>
        ///     Un-register a specific interest management system from the interest manager.
        /// </summary>
        /// <param name="observer">The system we want to un-register from the interest manager.</param>
        internal void UnRegisterVisibilitySystem(ref ObserverData observer)
        {
            if (!_visibilitySystems.Contains(observer))
            {
                Logger.LogWarning(
                    "[InterestManager] - Cannot find system in interest manager. Please check make sure it was registered.");

                return;
            }

            if (Logger.logEnabled)
                Logger.Log($"[Interest Manager] - Un-Registering system {observer} from our manager.");

            _visibilitySystems.Remove(observer);
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
                _observers = new HashSet<INetworkPlayer>(ServerObjectManager.Server.Players);

                ObserverProfilerMarker.End();

                return;
            }

            int inSystemsCount = 0;

            foreach (ObserverData visibilitySystem in _visibilitySystems)
            {
                if (!visibilitySystem.Observers.ContainsKey(identity)) continue;

                if(visibilitySystem.Observers[identity].Count <= 0) continue;

                inSystemsCount++;

                foreach (KeyValuePair<NetworkIdentity, HashSet<INetworkPlayer>> observer in visibilitySystem.Observers)
                {
                    _observers.UnionWith(observer.Value);
                }
            }

            // Multiple systems have been registered. We need to make sure the object is in all system observers
            // to know that it is actually should be sending data.
            if(inSystemsCount != _visibilitySystems.Count)
                _observers.Clear();

            ObserverProfilerMarker.End();
        }

        #endregion
    }
}
