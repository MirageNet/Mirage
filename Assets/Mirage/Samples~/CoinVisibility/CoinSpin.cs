using Mirage;
using Mirage.Serialization;
using UnityEngine;

namespace Examples.SpatialHash
{
    public class CoinSpin : NetworkBehaviour
    {
        static Transform Parent;

        public float Speed;

        [SyncVar(hook = nameof(OnRotationChanged)), FloatPack(180, 0.1f)] float Rotation;

        private void OnRotationChanged(float _)
        {
            if (IsClientOnly) // dont change if again if host
                transform.rotation = Quaternion.AngleAxis(Rotation, Vector3.up);
        }

        private void Awake()
        {
            // start random so not all coins have same angle
            Rotation = Random.Range(-180f, 180f);

            if (Parent == null)
                Parent = new GameObject("Coin Parent").transform;

            this.transform.parent = Parent;
        }

        private void FixedUpdate()
        {
            if (IsServer)
            {
                Rotation += (Speed * Time.fixedDeltaTime);
                if (Rotation > 180)
                    Rotation -= 360f;
                transform.rotation = Quaternion.AngleAxis(Rotation, Vector3.up);
            }
        }
    }
}
