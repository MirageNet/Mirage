using System;
using Mirage.SocketLayer;

namespace Mirage.Tests.Performance.Runtime.SpatialHashBenchmark
{
    public class Monster : NetworkBehaviour
    {
        [SyncVar]
        public float Speed { get; set; }
        [SyncVar]
        public int Health { get; set; }


        public bool TakeDamage(int damage)
        {
            // already dead
            if (Health < 0)
                return false;

            if (!IsServer)
                throw new InvalidOperationException("TakeDamage called when server not active");

            Health -= damage;
            // alive
            if (Health > 0)
                return false;

            // dead
            UnSpawn();
            return true;
        }

        public Pool<Monster> pool;

        public void UnSpawn()
        {
            ServerObjectManager.Destroy(Identity, false);
            gameObject.SetActive(false);
            pool.Put(this);
        }
    }
}
