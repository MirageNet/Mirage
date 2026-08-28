using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Mirage.Examples.InterestManagement
{
    /// <summary>
    /// Moves the NPC to random points. Only runs on the server, the position is sent to clients by NetworkTransform
    /// </summary>
    public class Wander : NetworkBehaviour
    {
        public NavMeshAgent agent;

        public Bounds bounds;

        private void Awake()
        {
            Identity.OnStartServer.AddListener(StartMoving);
        }

        private void StartMoving()
        {
            StartCoroutine(Move());
        }

        private IEnumerator Move()
        {
            while (true)
            {

                var position = new Vector3(
                    (Random.value - 0.5f) * bounds.size.x + bounds.center.x,
                    (Random.value - 0.5f) * bounds.size.y + bounds.center.y,
                    (Random.value - 0.5f) * bounds.size.z + bounds.center.z
                );

                agent.destination = position;

                yield return new WaitForSeconds(Random.Range(1.0f, 5.0f));
            }
        }
    }
}
