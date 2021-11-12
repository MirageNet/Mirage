using System;
using UnityEngine;

namespace Mirage.InterestManagement
{
    [Serializable]
    public class BaseSettings
    {
        [HideInInspector]
        public NetworkIdentity Identity;
    }

    public interface INetworkVisibility
    {
        /// <summary>
        ///     Perform any initialization here.
        /// </summary>
        void Startup();

        /// <summary>
        ///     perform any cleanup we need to do.
        /// </summary>
        void ShutDown();

        /// <summary>
        /// Invoked when an object is spawned in the server
        /// It should show that object to all relevant players
        /// </summary>
        /// <param name="identity">The object just spawned</param>
        void OnSpawned(NetworkIdentity identity);

        /// <summary>
        ///     Invoked when a player has authenticated on server.
        /// </summary>
        /// <param name="player">The player who just authenticated.</param>
        void OnAuthenticated(INetworkPlayer player);

        /// <summary>
        ///     Perform your own checks to see if player's can see other objects.
        /// </summary>
        void CheckForObservers();

        /// <summary>
        ///     Register network object to visibility system.
        /// </summary>
        void RegisterObject(BaseSettings settings);

        /// <summary>
        ///     Un-Register network object from visibility system.
        /// </summary>
        void UnRegisterObject(BaseSettings settings);
    }
}
