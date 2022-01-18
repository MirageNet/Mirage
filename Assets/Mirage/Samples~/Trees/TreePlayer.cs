using UnityEngine;

namespace Mirage.Examples.Trees
{
    public class TreePlayer : NetworkBehaviour
    {
        public float speed;

        [HasAuthority(error = false)]
        private void Update()
        {
            move();
            //attack();
        }
        private void Awake()
        {
            TreeSpawner.start = true;
        }

        private void move()
        {
            // rotate
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 dir = new Vector3(horizontal, 0, vertical).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }

        //private void attack()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
