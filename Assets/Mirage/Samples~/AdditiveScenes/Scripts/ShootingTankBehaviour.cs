using UnityEngine;

namespace Mirage.Examples.Additive
{
    // This script demonstrates how to trigger animations using ClientRpc
    // as a modern alternative to the legacy NetworkAnimator.
    public class ShootingTankBehaviour : NetworkBehaviour
    {
        [SyncVar]
        public Quaternion rotation { get; set; }
        
        public Animator animator;

        [Range(0, 1)]
        public float turnSpeed = 0.1f;

        private void Update()
        {
            if (IsServer && Identity.observers.Count > 0)
                ShootNearestPlayer();

            if (IsClient)
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, turnSpeed);
        }

        [Server]
        private void ShootNearestPlayer()
        {
            GameObject target = null;
            var distance = 100f;

            foreach (var networkConnection in Identity.observers)
            {
                var tempTarget = networkConnection.Identity.gameObject;
                var tempDistance = Vector3.Distance(tempTarget.transform.position, transform.position);

                if (target == null || distance > tempDistance)
                {
                    target = tempTarget;
                    distance = tempDistance;
                }
            }

            if (target != null)
            {
                transform.LookAt(target.transform.position + Vector3.down);
                rotation = transform.rotation;
                
                // Modern approach: trigger animation via ClientRpc
                RpcFire();
            }
        }

        [ClientRpc]
        private void RpcFire()
        {
            if (animator != null)
                animator.SetTrigger("Fire");
        }
    }
}
