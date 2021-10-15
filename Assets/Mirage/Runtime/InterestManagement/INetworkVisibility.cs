namespace Mirage.InterestManagement
{
    public interface INetworkVisibility
    {
        void Startup();

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
        ///     
        /// </summary>
        void CheckForObservers();
    }
}
