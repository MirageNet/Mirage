using UnityEngine;

namespace Mirage.Experimental
{
    public class DemoPlayer : MonoBehaviour
    {
        private int _money;
        public int Money { get => _money; set => _money = value; }

        private int _damage;
        public int Damage { get => _damage; set => _damage = value; }




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
