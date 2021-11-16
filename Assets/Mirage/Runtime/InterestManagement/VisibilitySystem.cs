using System;
using System.Collections.Generic;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.InterestManagement
{
    public abstract class VisibilitySystem
    {
        private static readonly ILogger logger = LogFactory.GetLogger<VisibilitySystem>();
        protected readonly ServerObjectManager ServerObjectManager;
        protected readonly InterestManager InterestManager;

        public readonly Dictionary<NetworkIdentity, HashSet<INetworkPlayer>> Observers = new Dictionary<NetworkIdentity, HashSet<INetworkPlayer>>();

        protected VisibilitySystem(ServerObjectManager serverObjectManager)
        {
            ServerObjectManager = serverObjectManager ?? throw new ArgumentNullException(nameof(serverObjectManager));
            InterestManager = serverObjectManager.InterestManager;
            InterestManager.RegisterSystem(this);
        }

        public void ShutDown()
        {
            InterestManager.UnregisterSystem(this);
        }

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
        public abstract void RebuildForPlayer(INetworkPlayer player);

        /// <summary>
        ///     Checks for observers for each registered network object.
        /// </summary>
        public abstract void RebuildAll();

        /// <summary>
        ///     Controls register new objects to this network visibility system
        /// </summary>
        /// <para>Passing in specific settings for this network object.</para>
        public abstract void RegisterObject<TSettings>(NetworkIdentity identity, TSettings settings);

        /// <summary>
        ///     Controls un-register objects from this network visibility system
        /// </summary>
        public abstract void UnregisterObject(NetworkIdentity identity);
    }
}
