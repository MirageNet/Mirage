using System.Collections;
using UnityEngine;

namespace Mirage.HeadlessBenchmark
{
    public class RandomMove : NetworkBehaviour
    {
        [SerializeField] private float movementDuration = 5.0f;
        [SerializeField] private float waitBeforeMoving = 5.0f;

        private static Vector3 GetRandomTarget()
        {
            float randX = Random.Range(-15.0f, 15.0f);
            float randY = Random.Range(-15.0f, 15.0f);
            var target = new Vector3(randX, randY, 0);
            return target;
        }

        private void Awake()
        {
            Identity.OnStartServer.AddListener(() => StartCoroutine(Runner()));
            Identity.OnStartLocalPlayer.AddListener(() => StartCoroutine(Runner()));
        }

        private IEnumerator Runner()
        {
            while (true)
            {
                // if owner is set, break
                if (IsServer && Owner != null)
                    yield break;


                float timer = 0.0f;
                Vector3 startPos = transform.position;
                Vector3 targetPos = GetRandomTarget();

                while (timer < movementDuration)
                {
                    timer += Time.deltaTime;

                    float t = timer / movementDuration;
                    // todo what is this??
                    //t = t * t * t * (t * (6f * t - 15f) + 10f);
                    transform.position = Vector3.Lerp(startPos, targetPos, t);

                    yield return null;
                }

                yield return new WaitForSeconds(waitBeforeMoving);
            }
        }
    }
}
