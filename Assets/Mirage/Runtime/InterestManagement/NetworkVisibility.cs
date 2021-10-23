using System.Collections.Generic;

namespace Mirage.InterestManagement
{
    public abstract class NetworkVisibility : INetworkVisibility
    {
        #region Fields

        private readonly ServerObjectManager _serverObjectManager;
        private ObserverData _visibilitySystemData;

        #endregion

        #region Properties

        public InterestManager InterestManager => _serverObjectManager.InterestManager;

        public Dictionary<INetworkPlayer, HashSet<NetworkIdentity>> VisibilitySystemData => _visibilitySystemData.Observers;

        #endregion

        protected NetworkVisibility(ServerObjectManager serverObjectManager)
        {
            _serverObjectManager = serverObjectManager;
        }

        public void Startup()
        {
            _visibilitySystemData = new ObserverData(this, new Dictionary<INetworkPlayer, HashSet<NetworkIdentity>>());

            _serverObjectManager.InterestManager?.RegisterVisibilitySystem(ref _visibilitySystemData);
        }

        public void ShutDown()
        {
            _serverObjectManager.InterestManager?.UnRegisterVisibilitySystem(ref _visibilitySystemData);
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
        /// <param name="player"></param>
        public abstract void OnAuthenticated(INetworkPlayer player);

        /// <summary>
        ///     
        /// </summary>
        public abstract void CheckForObservers();

        #endregion
    }
}
