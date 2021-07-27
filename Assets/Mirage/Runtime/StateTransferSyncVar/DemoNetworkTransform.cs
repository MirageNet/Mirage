using System.Collections;
using UnityEngine;

namespace Mirage.Experimental.StateSyncVar
{
    public class DemoNetworkTransform : NetworkBehaviour
    {
        [SyncVar] Vector3 _position;
        [SyncVar] Quaternion _rotation;

        public Vector3 Position { get => _position; set => _position = value; }

        public Quaternion Rotation { get => _rotation; set => _rotation = value; }

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

                Position = transform.position;
                Rotation = transform.rotation;

                yield return null;
            }
        }

        private void Update()
        {
            if (IsServer)
            {
                Position = transform.position;
                Rotation = transform.rotation;
            }
            if (IsClient)
            {
                transform.position = Position;
                transform.rotation = Rotation;
            }
        }
    }
}
