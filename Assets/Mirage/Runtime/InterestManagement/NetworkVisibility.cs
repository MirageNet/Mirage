using System.Collections.Generic;
using UnityEngine;

namespace Mirage.InterestManagement
{
    public abstract class NetworkVisibility : INetworkVisibility
    {
        #region Fields

        private readonly InterestManager _interestManager;
        private ObserverData visibilitySystemData;
        private NetworkWorld _networkWorld = new NetworkWorld();

        #endregion

        #region Properties

        public InterestManager InterestManager => _interestManager;

        public NetworkWorld NetworkWorld => _networkWorld;

        #endregion

        public NetworkVisibility(InterestManager interestManager)
        {
            _interestManager = interestManager;
        }

        public void Startup()
        {
            visibilitySystemData = new ObserverData(this, null);

            _interestManager?.RegisterVisibilitySystem(ref visibilitySystemData);
        }

        public void ShutDown()
        {
            _interestManager?.UnRegisterVisibilitySystem(ref visibilitySystemData);
        }

        #region Implementation of INetworkVisibility

        /// <summary>
        ///     Invoked when an object is spawned in the server
        ///     It should show that object to all relevant players
        /// </summary>
        /// <param name="identity">The object just spawned</param>
        public abstract void OnSpawned(NetworkIdentity identity);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="identity">The identity of the object that has spawned in.</param>
        /// <param name="position">The position in which the player spawned in at. We use <see cref="Transform.localPosition"/></param>
        /// <param name="players"></param>
        public abstract void CheckForObservers(NetworkIdentity identity, Vector3 position, out HashSet<INetworkPlayer> players);

        #endregion
    }
}
