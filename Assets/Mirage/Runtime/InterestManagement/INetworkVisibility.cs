using System.Collections.Generic;
using UnityEngine;

namespace Mirage.InterestManagement
{
    public interface INetworkVisibility
    {

        /// <summary>
        /// 
        /// </summary>
        void Startup();

        /// <summary>
        /// 
        /// </summary>
        void ShutDown();

        /// <summary>
        /// Invoked when an object is spawned in the server
        /// It should show that object to all relevant players
        /// </summary>
        /// <param name="identity">The object just spawned</param>
        void OnSpawned(NetworkIdentity identity);

        /// <summary>
        ///     
        /// </summary>
        void CheckForObservers();
    }
}
