using UnityEngine;

namespace Mirror.Examples.Pong
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private float speed = 30;
        [SerializeField] private Rigidbody2D rigidbody2d = null;

        // need to use FixedUpdate for rigidbody
        private void FixedUpdate()
        {
            // only let the local player control the racket.
            // don't control other player's rackets
            if (isLocalPlayer)
                rigidbody2d.velocity = new Vector2(0, Input.GetAxisRaw("Vertical")) * (speed * Time.fixedDeltaTime);
        }
    }
}
