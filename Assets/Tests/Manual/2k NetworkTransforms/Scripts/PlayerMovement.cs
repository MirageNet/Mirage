using UnityEngine;

namespace Mirage.Examples.OneK
{
    public class PlayerMovement : NetworkBehaviour
    {
        public float speed = 5;

        private void Update()
        {
            if (!IsLocalPlayer) return;

            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");

            var dir = new Vector3(h, 0, v);
            transform.position += dir.normalized * (Time.deltaTime * speed);
        }
    }
}
