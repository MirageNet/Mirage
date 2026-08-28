using UnityEngine;

namespace Mirage.Examples.MultipleAdditiveScenes
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsCollision : NetworkBehaviour
    {
        [Tooltip("how forcefully to push this object")]
        public float force = 12;

        public Rigidbody rigidbody3D;

        private void OnValidate()
        {
            if (rigidbody3D == null)
                rigidbody3D = GetComponent<Rigidbody>();
        }

        private void Awake()
        {
            // only the server simulates physics, clients just show the synced position.
            // IsServer is not set until the scene object is spawned, so wait for OnStartServer instead of checking in Start
            rigidbody3D.isKinematic = true;
            Identity.OnStartServer.AddListener(OnStartServer);
        }

        private void OnStartServer()
        {
            rigidbody3D.isKinematic = false;
        }

        [Server(error = false)]
        private void OnCollisionStay(Collision other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                // get direction from which player is contacting object
                var direction = other.contacts[0].normal;

                // zero the y and normalize so we don't shove this through the floor or launch this over the wall
                direction.y = 0;
                direction = direction.normalized;

                // push this away from player...a bit less force for host player
                if (other.gameObject.GetComponent<NetworkIdentity>().Owner == Server.LocalPlayer)
                    rigidbody3D.AddForce(direction * force * .5f);
                else
                    rigidbody3D.AddForce(direction * force);
            }
        }
    }
}
