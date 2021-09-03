using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirage.InterestManagement
{
    public abstract class NetworkVisibility : MonoBehaviour, INetworkVisibility
    {
        #region Fields

        public InterestManager InterestManager;
        private ServerObjectManager _serverObjectManager;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _serverObjectManager = FindObjectOfType<ServerObjectManager>();

            if (_serverObjectManager.Server == null || !_serverObjectManager.Server.Active) return;

            InterestManager = _serverObjectManager.InterestManager;

            if (InterestManager == null)
            {
                throw new ArgumentNullException(nameof(InterestManager),
                    $"Cannot find {nameof(InterestManager)}. Please manually assign or make sure you have one in scene.");
            }

            InterestManager.RegisterVisibilitySystem(this);
        }

        private void OnDestroy()
        {
            InterestManager?.UnRegisterVisibilitySystem(this);
        }

        #endregion

        #region Implementation of INetworkVisibility

        /// <summary>
        /// Invoked when an object is spawned in the server
        /// It should show that object to all relevant players
        /// </summary>
        /// <param name="identity">The object just spawned</param>
        public abstract void OnSpawned(NetworkIdentity identity);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="identity">The identity of the object that has spawned in.</param>
        /// <param name="position">The position in which the object spawned in. We use <see cref="Transform.localPosition"/></param>
        /// <param name="players"></param>
        public abstract void CheckForObservers(NetworkIdentity identity, Vector3 position, out HashSet<INetworkPlayer> players);

        #endregion
    }
}
