using Mirage;
using UnityEngine;

namespace Mirage.Snippets.GameObjects
{
    public class CustomSpawnExample : MonoBehaviour
    {
        public ClientObjectManager ClientObjectManager;
        public ServerObjectManager ServerObjectManager;
        public NetworkIdentity coin;
        public NetworkIdentity m_CoinPrefab;
        public NetworkIdentity prefab;
        public ClientObjectManager clientObjectManager;

        // CodeEmbed-Start: spawn-handler-delegate
        NetworkIdentity SpawnDelegate(SpawnMessage msg) 
        {
            // do stuff here
            return null;
        }
        // CodeEmbed-End: spawn-handler-delegate

        // CodeEmbed-Start: unspawn-handler-delegate
        void UnSpawnDelegate(NetworkIdentity spawned) 
        {
            // do stuff here
        }
        // CodeEmbed-End: unspawn-handler-delegate

        public void SetupHandlers()
        {
            // CodeEmbed-Start: generate-prefab-runtime
            // Create a hash that can be generated on both server and client
            // using a string and GetStableHashCode is a good way to do this
            int coinHash = "MyCoin".GetStableHashCode();

            // register handlers using hash
            ClientObjectManager.RegisterSpawnHandler(coinHash, SpawnCoin, UnSpawnCoin);
            // CodeEmbed-End: generate-prefab-runtime

            // CodeEmbed-Start: use-existing-prefab
            // register handlers using prefab
            ClientObjectManager.RegisterPrefab(coin, SpawnCoin, UnSpawnCoin);
            // CodeEmbed-End: use-existing-prefab
        }

        public void SpawnOnServer()
        {
            // CodeEmbed-Start: spawn-on-server
            int coinHash = "MyCoin".GetStableHashCode();

            // spawn a coin - SpawnCoin is called on client
            // pass in coinHash so that it is set on the Identity before it is sent to client
            ServerObjectManager.Spawn(gameObject, coinHash);
            // CodeEmbed-End: spawn-on-server
        }

        // CodeEmbed-Start: spawn-coin-methods
        public NetworkIdentity SpawnCoin(SpawnMessage msg)
        {
            return Instantiate(m_CoinPrefab, msg.SpawnValues.Position ?? m_CoinPrefab.transform.position, msg.SpawnValues.Rotation ?? m_CoinPrefab.transform.rotation);
        }
        public void UnSpawnCoin(NetworkIdentity spawned)
        {
            Destroy(spawned);
        }
        // CodeEmbed-End: spawn-coin-methods

        // CodeEmbed-Start: pool-spawn-handlers
        void ClientConnected() 
        {
            clientObjectManager.RegisterPrefab(prefab, PoolSpawnHandler, PoolUnspawnHandler);
        }

        // used by clientObjectManager.RegisterPrefab
        NetworkIdentity PoolSpawnHandler(SpawnMessage msg)
        {
            return GetFromPool(msg.SpawnValues.Position ?? prefab.transform.position, msg.SpawnValues.Rotation ?? prefab.transform.rotation);
        }

        // used by clientObjectManager.RegisterPrefab
        void PoolUnspawnHandler(NetworkIdentity spawned)
        {
            PutBackInPool(spawned);
        }
        // CodeEmbed-End: pool-spawn-handlers

        private NetworkIdentity GetFromPool(Vector3 position, Quaternion rotation)
        {
            return null;
        }

        private void PutBackInPool(NetworkIdentity spawned)
        {
        }
    }

    public static class ClientObjectManagerExtensions
    {
        public static void RegisterPrefab(this ClientObjectManager manager, NetworkIdentity identity, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
        {
        }
    }
}
