using System.Collections;
using UnityEngine;

namespace Mirage.Experimental
{
    public class DemoNetworkTransform : MonoBehaviour
    {
        public Vector3 Position { get => transform.position; set => transform.position = value; }

        public Quaternion Rotation { get => transform.rotation; set => transform.rotation = value; }

        private float speed;
        float moveRadius;
        Vector3 current;
        Vector3 target;
        internal void StartAutoMove(float radius, float speed = 2)
        {
            this.speed = speed;
            moveRadius = radius;
            StartCoroutine(_autoMove());
        }

        private IEnumerator _autoMove()
        {
            while (true)
            {
                if (Vector3.Distance(current, target) < moveRadius / 100)
                {
                    Vector2 point = Random.insideUnitCircle * moveRadius;
                    target = new Vector3(point.x, 0, point.y);
                }


                current = Vector3.MoveTowards(current, target, speed * Time.deltaTime);
                transform.position = current;
                transform.forward = (target - current).normalized;
                yield return null;
            }
        }
    }
}
