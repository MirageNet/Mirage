using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// A component to synchronize the position of child transforms of networked objects.
    /// <para>There must be a NetworkTransform on the root object of the hierarchy. There can be multiple NetworkTransformChild components on an object. This does not use physics for synchronization, it simply synchronizes the localPosition and localRotation of the child transform and lerps towards the received values.</para>
    /// </summary>
    [AddComponentMenu("Network/NetworkTransformChild")]
    [HelpURL("https://miragenet.github.io/Mirage/docs/components/network-transform-child")]
    public class NetworkTransformChild : NetworkTransformBase
    {
        [Header("Target")]
        public Transform Target;

        protected override Transform TargetComponent => Target;
    }
}
