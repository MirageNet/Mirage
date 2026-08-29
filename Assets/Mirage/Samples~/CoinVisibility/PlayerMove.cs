using Mirage;
using UnityEngine;

namespace Examples.SpatialHash
{
    public class PlayerMove : NetworkBehaviour
    {
        [SerializeField] float Speed;
        [SerializeField] float RotateSpeed;

        // Update is called once per frame
        void Update()
        {
            transform.position += Input.GetAxis("Vertical") * Speed * transform.forward * Time.deltaTime;
            transform.Rotate(Input.GetAxis("Horizontal") * RotateSpeed * Vector3.up * Time.deltaTime, Space.Self);
        }
    }
}
