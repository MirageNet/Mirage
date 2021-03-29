using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Mirage.Examples.InterestManagement
{
    public class Wander : MonoBehaviour
    {
        public NavMeshAgent agent;

        public Bounds bounds;
        private Coroutine coroutine;

        // Start is called before the first frame update
        public void StartMoving()
        {
            coroutine = StartCoroutine(Move());
        }

        private void OnDestroy()
        {
            StopCoroutine(coroutine);
        }

        public IEnumerator Move()
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
                if (!agent.isActiveAndEnabled) { yield break; }
            }
        }
    }
}
