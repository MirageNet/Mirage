using System.Collections.Generic;

namespace Mirage.InterestManagement
{
    internal class GlobalNetworkVisibilitySystem : NetworkVisibility
    {
        private readonly HashSet<BaseSettings> _globalObjects = new HashSet<BaseSettings>();

        public GlobalNetworkVisibilitySystem(ServerObjectManager serverObjectManager) : base(serverObjectManager)
        {
        }

        #region Overrides of NetworkVisibility

        /// <summary>
        ///     Invoked when an object is spawned in the server
        ///     It should show that object to all relevant players
        /// </summary>
        /// <param name="identity">The object just spawned</param>
        public override void OnSpawned(NetworkIdentity identity)
        {
            // does object have owner?
            if (identity.Owner != null)
            {
                OnAuthenticated(identity.Owner);
            }
        }

        /// <summary>
        ///     When new player authenticates we need to show them objects they should see.
        /// </summary>
        /// <param name="player">The player that just authenticated and we need to show objects to.</param>
        public override void OnAuthenticated(INetworkPlayer player)
        {
            foreach (BaseSettings setting in _globalObjects)
            {
                if (!VisibilitySystemData.ContainsKey(setting.Identity))
                    VisibilitySystemData.Add(setting.Identity, new HashSet<INetworkPlayer>());
                else if (VisibilitySystemData.ContainsKey(setting.Identity) && !VisibilitySystemData[setting.Identity].Contains(player))
                    VisibilitySystemData[setting.Identity].Add(player);

                InterestManager.ServerObjectManager.ShowToPlayer(setting.Identity, player);
            }
        }

        /// <summary>
        ///     Checks for observers for each registered network object.
        /// </summary>
        public override void CheckForObservers()
        {
            // NOOP
        }

        /// <summary>
        ///     Controls register new objects to this network visibility system
        /// </summary>
        /// <para>Passing in specific settings for this network object.</para>
        public override void RegisterObject(BaseSettings settings)
        {
            _globalObjects.Add(settings);
        }

        /// <summary>
        ///     Controls un-register objects from this network visibility system
        /// </summary>
        public override void UnRegisterObject(BaseSettings settings)
        {
            _globalObjects.Remove(settings);
        }

        #endregion
    }
}
