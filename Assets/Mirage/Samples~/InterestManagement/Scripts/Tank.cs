using UnityEngine;
using UnityEngine.AI;

namespace Mirage.Examples.InterestManagement
{
    public class Tank : NetworkBehaviour
    {
        [Header("Components")]
        public NavMeshAgent agent;
        public Animator animator;

        [Header("Movement")]
        public float rotationSpeed = 100;

        [Header("Game Stats")]
        [SyncVar]
        public string playerName;

        public TextMesh nameText;

        [Server]
        public void SetRandomName()
        {
            playerName = "PLAYER" + Random.Range(1, 99);
        }

        void Update()
        {
            if (Camera.main)
            {
                nameText.text = playerName;
                nameText.transform.rotation = Camera.main.transform.rotation;
            }

            // movement for local player
            if (!IsLocalPlayer)
                return;

            //Set local players name color to green
            nameText.color = Color.green;

            // rotate
            float horizontal = Input.GetAxis("Horizontal");
            transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);

            // move
            float vertical = Input.GetAxis("Vertical");
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            agent.velocity = forward * Mathf.Max(vertical, 0) * agent.speed;
            animator.SetBool("Moving", agent.velocity != Vector3.zero);

        }

    }
}
