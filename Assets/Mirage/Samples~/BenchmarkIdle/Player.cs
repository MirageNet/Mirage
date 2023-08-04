using Mirage;
using UnityEngine;

namespace Mirror.Examples.BenchmarkIdle
{
    public class Player : NetworkBehaviour
    {
        // automated movement.
        // player may switch to manual movement any time
        [Header("Automated Movement")]
        public bool autoMove = true;
        public float autoSpeed = 2;
        public float movementProbability = 0.5f;
        public float movementDistance = 20;
        private bool moving;
        private Vector3 start;
        private Vector3 destination;

        [Header("Manual Movement")]
        public float manualSpeed = 10;

        // cache .transform for benchmark demo.
        // Component.get_transform shows in profiler otherwise.
        private Transform tf;

        private void Awake()
        {
            Identity.OnStartLocalPlayer.AddListener(OnStartLocalPlayer);
        }

        private void OnStartLocalPlayer()
        {
            tf = transform;
            start = tf.position;
        }

        private void AutoMove()
        {
            if (moving)
            {
                if (Vector3.Distance(tf.position, destination) <= 0.01f)
                {
                    moving = false;
                }
                else
                {
                    tf.position = Vector3.MoveTowards(tf.position, destination, autoSpeed * Time.deltaTime);
                }
            }
            else
            {
                var r = Random.value;
                if (r < movementProbability * Time.deltaTime)
                {
                    // calculate a random position in a circle
                    var circleX = Mathf.Cos(Random.value * Mathf.PI);
                    var circleZ = Mathf.Sin(Random.value * Mathf.PI);
                    var circlePos = new Vector2(circleX, circleZ);
                    var dir = new Vector3(circlePos.x, 0, circlePos.y);

                    // set destination on random pos in a circle around start.
                    // (don't want to wander off)
                    destination = start + (dir * movementDistance);
                    moving = true;
                }
            }
        }

        private void ManualMove()
        {
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");

            var direction = new Vector3(h, 0, v);
            transform.position += direction.normalized * (Time.deltaTime * manualSpeed);
        }

        private static bool Interrupted() =>
            Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

        private void Update()
        {
            if (!IsLocalPlayer) return;

            // player may interrupt auto movement to switch to manual
            if (Interrupted()) autoMove = false;

            // move
            if (autoMove) AutoMove();
            else ManualMove();
        }
    }
}
