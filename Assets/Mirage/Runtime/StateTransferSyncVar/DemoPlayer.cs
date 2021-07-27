using UnityEngine;

namespace Mirage.Experimental.StateSyncVar
{
    public class DemoPlayer : NetworkBehaviour
    {
        [SyncVar] int _money;
        [SyncVar] int _damage;

        public int Money
        {
            get => _money;
            set { _money = value; }
        }
        public int Damage
        {
            get => _damage;
            set { _damage = value; }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<DemoHealth>(out DemoHealth health))
            {
                bool died = health.Harm(Damage);
                if (died)
                {
                    Money++;
                    if (Money > Damage)
                    {
                        Damage++;
                        Money -= Damage;
                    }
                }
            }
        }

        internal void OnDeath()
        {
            Money = 0;
            Damage = 1;
        }
    }
}
