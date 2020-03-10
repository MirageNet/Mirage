using UnityEngine;
using UnityEngine.AI;

namespace Mirror.Examples.Tanks
{
    public class Tank : NetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private NavMeshAgent agent = null;
        [SerializeField] private Animator animator = null;

        [Header("Movement")]
        [SerializeField] private float rotationSpeed = 100f;

        [Header("Firing")]
        [SerializeField] private KeyCode shootKey = KeyCode.Space;
        [SerializeField] private GameObject projectilePrefab = null;
        [SerializeField] private Transform projectileMount = null;

        private static readonly int moving = Animator.StringToHash("Moving");
        private static readonly int shoot = Animator.StringToHash("Shoot");

        void Update()
        {
            // movement only for local player
            if (!isLocalPlayer)
                return;

            // rotate
            float horizontal = Input.GetAxis("Horizontal");
            transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);

            // move
            float vertical = Input.GetAxis("Vertical");
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            agent.velocity = forward * (Mathf.Max(vertical, 0) * agent.speed);
            animator.SetBool(moving, agent.velocity != Vector3.zero);

            // shoot
            if (Input.GetKeyDown(shootKey))
            {
                CmdFire();
            }
        }

        // this is called on the server
        [Command]
        void CmdFire()
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileMount.position, transform.rotation);
            server.Spawn(projectile);
            RpcOnFire();
        }

        // this is called on the tank that fired for all observers
        [ClientRpc]
        void RpcOnFire()
        {
            animator.SetTrigger(shoot);
        }
    }
}
