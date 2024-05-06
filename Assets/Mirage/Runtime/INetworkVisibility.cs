using System.Collections.Generic;

namespace Mirage
{
    public interface INetworkVisibility
    {
        bool OnCheckObserver(INetworkPlayer player);
        void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize);
    }

    /// <summary>
    /// Default visible when no NetworkVisibility is added to the gameobject
    /// </summary>
    internal class AlwaysVisible : INetworkVisibility
    {
        private readonly NetworkServer _server;

        public AlwaysVisible(NetworkServer server)
        {
            _server = server;
        }

        public bool OnCheckObserver(INetworkPlayer player) => true;

        public void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize)
        {
            // add all server connections
            foreach (var player in _server.AuthenticatedPlayers)
            {
                // skip players that are loading a scene
                if (!player.SceneIsReady)
                    continue;

                observers.Add(player);
            }
        }
    }
}
