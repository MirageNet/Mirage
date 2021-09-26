namespace Mirage.InterestManagement
{
    public abstract class NetworkVisibility : INetworkVisibility
    {
        #region Fields

        private readonly InterestManager _interestManager;
        private ObserverData visibilitySystemData;

        #endregion

        #region Properties

        public InterestManager InterestManager => _interestManager;

        #endregion

        protected NetworkVisibility(InterestManager interestManager)
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
        /// <param name="player"></param>
        public abstract void OnAuthenticated(INetworkPlayer player);

        /// <summary>
        ///     
        /// </summary>
        public abstract void CheckForObservers();

        #endregion
    }
}
