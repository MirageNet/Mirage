using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirage.Examples.Additive
{
    /// <summary>
    /// Proximity checker that also hides objects in the sub scene from players that have not loaded it.
    /// <para>Without this, the server could send a spawn message for a scene object that the client hasn't loaded yet.</para>
    /// <para>Objects in the main scene are only checked by distance, the same as <see cref="NetworkProximityChecker"/></para>
    /// </summary>
    public class SubSceneProximityChecker : NetworkProximityChecker
    {
        private SubSceneLoader _loader;
        // cached so RemoveWhere doesn't allocate a new delegate each rebuild
        private Predicate<INetworkPlayer> _hasNotLoaded;

        // found lazily because Server is not set in Awake, and observers can be checked before OnStartServer is invoked
        private SubSceneLoader Loader
        {
            get
            {
                if (_loader == null)
                {
                    _loader = Server.GetComponent<SubSceneLoader>();
                    Debug.Assert(_loader != null, "SubSceneLoader should be on the same GameObject as NetworkServer");
                    _hasNotLoaded = player => !_loader.HasLoadedSubScene(player);
                }
                return _loader;
            }
        }

        public override bool OnCheckObserver(INetworkPlayer player)
        {
            if (Loader.IsInSubScene(gameObject) && !Loader.HasLoadedSubScene(player))
                return false;

            return base.OnCheckObserver(player);
        }

        public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize)
        {
            base.OnRebuildObservers(observers, initialize);

            if (Loader.IsInSubScene(gameObject))
                observers.RemoveWhere(_hasNotLoaded);
        }
    }
}
