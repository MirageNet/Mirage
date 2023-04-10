using System.Collections.Generic;

namespace Mirage
{
    public interface INetworkVisibility
    {
        bool OnCheckObserver(INetworkPlayer player);
        void OnRebuildObservers(HashSet<INetworkPlayer> newObservers, bool initialize);
    }

    /// <summary>
    /// Default visible when no NetworkVisibility is added to the gameobject
    /// </summary>
    internal class AlwaysVisible : INetworkVisibility
    {
        private readonly ServerObjectManager _objectManager;
        private readonly NetworkServer _server;

        public AlwaysVisible(ServerObjectManager serverObjectManager)
        {
            _objectManager = serverObjectManager;
            _server = serverObjectManager.Server;
        }

        public bool OnCheckObserver(INetworkPlayer player) => true;

        public void OnRebuildObservers(HashSet<INetworkPlayer> newObservers, bool initialize)
        {
            // add all server connections
            foreach (var player in _server.Players)
            {
                if (!player.SceneIsReady)
                    continue;

                // todo replace this with a better visibility system (where default checks auth/scene ready)
                if (_objectManager.OnlySpawnOnAuthenticated && !player.IsAuthenticated)
                    continue;

                newObservers.Add(player);
            }

            // add local host connection (if any)
            if (_server.LocalPlayer != null && _server.LocalPlayer.SceneIsReady)
            {
                newObservers.Add(_server.LocalPlayer);
            }
        }
    }
}
