using System.Collections.Generic;

namespace Mirage.Visibility
{
    /// <summary>
    /// Always shows an object, even if player is loading scene
    /// <para>This can be used to make sure that Managers in DontDestoryOnLoad dont get despawned</para>
    /// </summary>
    public class AlwaysNetworkVisibility : NetworkVisibility
    {
        public override bool OnCheckObserver(INetworkPlayer player)
        {
            return true;
        }

        public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize)
        {
            observers.UnionWith(Server.AuthenticatedPlayers);
        }
    }
}
