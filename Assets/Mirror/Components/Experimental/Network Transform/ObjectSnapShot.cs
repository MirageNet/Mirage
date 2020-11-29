using UnityEngine;

namespace Mirror.Components.Experimental
{
    public class ObjectSnapShot
    {
        public byte NetId;
        public byte SequenceData;
        public Vector3 PositionData;
        public Quaternion RotationData;
        public Vector3 ScaleData;
        public float Velocity;
        public float AngularVelocity;
    }
}
