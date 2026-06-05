using Mirage;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mirage.Tests.Performance.Runtime.SpatialHashBenchmark
{
    public class PlayerInput : NetworkBehaviour
    {
        private PlayerCharacter character;
        private bool headless;
        private Vector3 target;

        // faster for human
        private float Speed => character.Speed * (headless ? 10 : 20);

        private float RotateSpeed => Speed * 5;


        private void Awake()
        {
            headless = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
            character = GetComponent<PlayerCharacter>();
        }

        private void Update()
        {
            if (!IsLocalPlayer)
                return;

            if (headless)
            {
                headlessMove();
            }
            else
            {
                humanMove();
            }
        }

        private void humanMove()
        {
            // rotate
            var horizontal = Input.GetAxis("Horizontal");
            transform.Rotate(0, horizontal * RotateSpeed * Time.deltaTime, 0);

            // move
            var vertical = Input.GetAxis("Vertical");
            var move = Speed * Time.deltaTime * vertical * Vector3.forward;
            transform.Translate(move, Space.Self);

            // force in bounds
            if (Vector3.Distance(transform.position, Vector3.zero) > character.SpawnRadius)
            {
                transform.position = transform.position.normalized * character.SpawnRadius;
            }
        }

        private void headlessMove()
        {
            var position = transform.position;
            if (Vector3.Distance(target, position) < 0.1f || target == Vector3.zero)
                target = Helper.GetRandomPosition(character.SpawnRadius);

            var forward = transform.forward;
            // rotate first, so that position and target are never equal
            var direction = (target - position).normalized;
            Debug.Assert(direction != Vector3.zero, "Direction zero");
            transform.forward = Vector3.RotateTowards(forward, direction, RotateSpeed, RotateSpeed);
            transform.position = Vector3.MoveTowards(position, target, Speed * Time.deltaTime);
        }
    }
}
