using System.Collections.Generic;

namespace Mirage.InterestManagement
{
    public abstract class VisibilitySystem : INetworkVisibility
    {
        #region Fields

        private readonly ServerObjectManager _serverObjectManager;
        private VisibilitySystemData _visibilitySystemData;

        #endregion

        #region Properties

        public InterestManager InterestManager => _serverObjectManager.InterestManager;

        public Dictionary<NetworkIdentity, HashSet<INetworkPlayer>> VisibilitySystemData => _visibilitySystemData.Observers;

        #endregion

        protected VisibilitySystem(ServerObjectManager serverObjectManager)
        {
            _serverObjectManager = serverObjectManager;
        }

        public void Startup()
        {
            _visibilitySystemData = new VisibilitySystemData(this, new Dictionary<NetworkIdentity, HashSet<INetworkPlayer>>());

            if (!InterestManager.IsRegisteredAlready(ref _visibilitySystemData))
                InterestManager?.RegisterVisibilitySystem(ref _visibilitySystemData);
        }

        public void ShutDown()
        {
            if (InterestManager.IsRegisteredAlready(ref _visibilitySystemData))
                InterestManager?.UnRegisterVisibilitySystem(ref _visibilitySystemData);
        }

        #region Implementation of INetworkVisibility

        /// <summary>
        ///     Invoked when an object is spawned in the server
        ///     It should show that object to all relevant players
        /// </summary>
        /// <param name="identity">The object just spawned</param>
        public abstract void OnSpawned(NetworkIdentity identity);

        /// <summary>
        ///     When new player authenticates we need to show them objects they should see.
        /// </summary>
        /// <param name="player">The player that just authenticated and we need to show objects to.</param>
        public abstract void OnAuthenticated(INetworkPlayer player);

        /// <summary>
        ///     Checks for observers for each registered network object.
        /// </summary>
        public abstract void CheckForObservers();

        /// <summary>
        ///     Controls register new objects to this network visibility system
        /// </summary>
        /// <para>Passing in specific settings for this network object.</para>
        public abstract void RegisterObject(BaseSettings settings);

        /// <summary>
        ///     Controls un-register objects from this network visibility system
        /// </summary>
        public abstract void UnRegisterObject(BaseSettings settings);

        #endregion
    }
}
