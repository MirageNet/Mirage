using UnityEngine;

namespace Mirage.Examples.RigidbodyPhysics
{
    public class AddForce : NetworkBehaviour
    {
        [SerializeField] private float force = 500f;

        private void Update()
        {
            if (IsServer && Input.GetKeyDown(KeyCode.Space))
            {
                GetComponent<Rigidbody>().AddForce(Vector3.up * force);
            }
        }
    }
}
