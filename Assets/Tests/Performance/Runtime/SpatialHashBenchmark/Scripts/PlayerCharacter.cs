using UnityEngine;
using Random = UnityEngine.Random;

namespace Mirage.Tests.Performance.Runtime.SpatialHashBenchmark
{
    public class PlayerCharacter : NetworkBehaviour
    {
        public float SpawnRadius;

        [SyncVar]
        public float Speed { get; set; } = 3;
        [SyncVar]
        public int Damage { get; set; } = 1;
        [SyncVar]
        public int Level { get; set; } = 1;
        [SyncVar]
        public int XP { get; set; }


        private void Awake()
        {
            Identity.OnStartServer.AddListener(onStartServer);
            Identity.OnStartClient.AddListener(OnStartClient);
        }

        private void OnStartClient()
        {
            var mat = GetComponent<Renderer>().material;
            if (IsLocalPlayer)
            {
                mat.color = new Color(0, 0, 0.6f);
            }
            else
            {
                mat.color = new Color(0.6f, 0, 0);
            }
        }

        private void onStartServer()
        {
            transform.SetPositionAndRotation(Helper.GetRandomPosition(SpawnRadius), Quaternion.Euler(0, Random.value * 360, 0));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer)
                return;

            if (other.TryGetComponent(out Monster monster))
            {
                var killed = monster.TakeDamage(Damage);
                if (killed)
                {
                    XP++;
                    if (XP > Level * Level * Level)
                    {
                        LevelUp();
                    }
                }
                // if not dead, shove monster to right/left
                else
                {
                    monster.transform.position += transform.right * (Random.value < 0.5f ? 2 : -2);
                }
            }
        }

        private void LevelUp()
        {
            XP = 0;
            Level++;
            if (Random.value > 0.2)
            {
                Damage += (int)Mathf.Sqrt(Level);
            }
            else
            {
                Speed += (int)Mathf.Sqrt(Mathf.Sqrt(Level));
            }
        }
    }
}
