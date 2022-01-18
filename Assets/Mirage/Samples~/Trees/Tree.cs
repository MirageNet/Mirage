using Mirage.Serialization;
using UnityEngine;

namespace Mirage.Examples.Trees
{
    public class Tree : NetworkBehaviour
    {
        [SyncVar(hook = nameof(HealthChanged)), BitCountFromRange(0, 100)]
        public int health;

        public GameObject model;
        bool serverStarted = false;

        private void Awake()
        {
            Identity.OnStartServer.AddListener(OnStartServer);
        }

        private void OnStartServer()
        {
            // trees start with random health
            health = UnityEngine.Random.Range(20, 100);
            serverStarted = true;
        }

        void HealthChanged(int _, int health)
        {
            model.SetActive(true);

            // scale with health^2
            int healthSize = health / 20;
            model.transform.localScale = new Vector3(1, 5, 1) * (healthSize * healthSize);
        }
    }
}
