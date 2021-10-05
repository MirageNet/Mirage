using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Mirage
{
    public class NetworkManagerSceneChecker : NetworkVisibility
    {
        private NetworkSceneManager manager;

        private void Awake()
        {
            Identity.OnStartServer.AddListener(OnStartServer);
        }

        private void OnStartServer()
        {
            manager = ServerObjectManager.GetComponent<NetworkSceneManager>();
        }

        public override bool OnCheckObserver(INetworkPlayer player)
        {
            Scene scene = gameObject.scene;
            return manager.IsPlayerInScene(scene, player);
        }

        public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize)
        {
            Scene scene = gameObject.scene;

            foreach (INetworkPlayer player in Server.Players)
            {
                if (manager.IsPlayerInScene(scene, player))
                {
                    observers.Add(player);
                }
            }
        }
    }
}
