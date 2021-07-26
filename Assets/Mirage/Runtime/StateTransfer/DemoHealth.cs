using UnityEngine;

namespace Mirage.Experimental
{
    public class DemoHealth : MonoBehaviour
    {
        DemoNetworkIdentity _id;
        DemoNetworkIdentity Identity => _id ?? (_id = GetComponent<DemoNetworkIdentity>());

        private int _health;
        public DemoMonster monster;

        public int Health { get => _health; set => _health = value; }

        public bool Harm(int damage)
        {
            Health = Mathf.Max(0, Health - damage);
            if (Health <= 0)
            {
                Death();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Death()
        {
            if (Identity.player != null)
            {
                Identity.player.OnDeath();
                Health = 20;
            }
            else if (Identity.monster != null)
            {
                // destroy if monster
                GameObject.Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("unknown object");
            }
        }
    }
}
